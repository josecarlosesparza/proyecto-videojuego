using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace NexoAvance
{
    public class MainMenuController : MonoBehaviour
    {
        public GameObject home, controls, options, credits, exitConfirm, loading;
        public Button playButton, controlsButton, optionsButton, creditsButton, exitButton;
        public Button controlsBack, optionsBack, creditsBack, cancelExit, confirmExit, defaultsButton;
        public Slider volume;
        public Toggle fullscreen;
        public Text volumeValue, loadMessage;
        public AudioSource uiAudio;
        public string gameScene = "PrimerAvance";
        public bool IsLoading { get; private set; }
        public string CurrentPage { get; private set; } = "Inicio";
        public HeroSelectionScreen characterSelection;
        AudioClip click;
        GameObject returnSelection;
        bool applying;
        public static bool Testing => System.Array.IndexOf(System.Environment.GetCommandLineArgs(), "-nexoMenuTest") >= 0;
        public static string PreferencePrefix => Testing ? "NEXO.Test.Menu." : "NEXO.Menu.";

        void Awake()
        {
            Time.timeScale = 1;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            CreateClick();
            playButton.onClick.AddListener(Play);
            controlsButton.onClick.AddListener(() => Show("Controles"));
            optionsButton.onClick.AddListener(() => Show("Opciones"));
            creditsButton.onClick.AddListener(() => Show("Créditos"));
            exitButton.onClick.AddListener(() => Show("Salir"));
            controlsBack.onClick.AddListener(Home);
            optionsBack.onClick.AddListener(Home);
            creditsBack.onClick.AddListener(Home);
            cancelExit.onClick.AddListener(Home);
            confirmExit.onClick.AddListener(ExitGame);
            defaultsButton.onClick.AddListener(RestoreDefaults);
            volume.onValueChanged.AddListener(ChangeVolume);
            fullscreen.onValueChanged.AddListener(ChangeFullscreen);
            var chooser=new GameObject("Selección de protagonista");chooser.transform.SetParent(home.transform.parent,false);characterSelection=chooser.AddComponent<HeroSelectionScreen>();characterSelection.Build(this);
            ApplySavedOptions();
            SetPage("Inicio");
        }

        IEnumerator Start() { yield return null; Select(playButton.gameObject); }
        void Update()
        {
            if (IsLoading) return;
            bool escape = false;
#if ENABLE_INPUT_SYSTEM
            escape = Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame;
#elif ENABLE_LEGACY_INPUT_MANAGER
            escape = Input.GetKeyDown(KeyCode.Escape);
#endif
            if (escape) { if (CurrentPage == "Inicio") Show("Salir"); else Home(); }
            // Si se pulsa una zona vacía, el teclado conserva una selección útil.
            if (EventSystem.current && !EventSystem.current.currentSelectedGameObject)
                Select(FirstSelection().gameObject);
        }

        public void Show(string page)
        {
            if (IsLoading) return;
            if (page != "Controles" && page != "Opciones" && page != "Créditos" && page != "Salir") return;
            returnSelection = EventSystem.current ? EventSystem.current.currentSelectedGameObject : playButton.gameObject;
            Sound(); SetPage(page); Select(FirstSelection().gameObject);
        }
        public void Home()
        {
            if (IsLoading) return;
            Sound(); SaveOptions(); SetPage("Inicio");
            Select(returnSelection && returnSelection.activeInHierarchy ? returnSelection : playButton.gameObject);
        }
        void SetPage(string page)
        {
            CurrentPage = page;
            if(characterSelection)characterSelection.gameObject.SetActive(page == "Personajes");
            home.SetActive(page == "Inicio"); controls.SetActive(page == "Controles");
            options.SetActive(page == "Opciones"); credits.SetActive(page == "Créditos");
            exitConfirm.SetActive(page == "Salir"); loading.SetActive(page == "Cargando");
        }
        Selectable FirstSelection()
        {
            switch (CurrentPage)
            {
                case "Personajes": return characterSelection.cards[characterSelection.Current];
                case "Controles": return controlsBack;
                case "Opciones": return volume;
                case "Créditos": return creditsBack;
                case "Salir": return cancelExit;
                default: return playButton;
            }
        }
        static void Select(GameObject item) { if (EventSystem.current) EventSystem.current.SetSelectedGameObject(item); }

        public void Play() { if(IsLoading)return;Sound();SetPage("Personajes");characterSelection.Open(); }
        public void StartSelectedGame(){if(!IsLoading)StartCoroutine(LoadGame());}
        IEnumerator LoadGame()
        {
            if (!Application.CanStreamedLevelBeLoaded(gameScene))
            {
                Debug.LogError("Falta la escena " + gameScene + " en la lista de escenas de la compilación.");
                yield break;
            }
            IsLoading = true; Sound(); SaveOptions(); Time.timeScale = 1;
            SetPage("Cargando"); loadMessage.text = "Entrando a Calles bajo control…";
            yield return null;
            var operation = SceneManager.LoadSceneAsync(gameScene, LoadSceneMode.Single);
            while (!operation.isDone) yield return null;
        }

        void ApplySavedOptions()
        {
            applying = true;
            volume.value = Mathf.Clamp01(PlayerPrefs.GetFloat(PreferencePrefix + "Volume", .65f));
            fullscreen.isOn = PlayerPrefs.GetInt(PreferencePrefix + "Fullscreen", 0) == 1;
            AudioListener.volume = volume.value;
            volumeValue.text = Mathf.RoundToInt(volume.value * 100) + "%";
            ApplyScreen(fullscreen.isOn);
            applying = false;
        }
        void ChangeVolume(float value)
        {
            AudioListener.volume = value;
            volumeValue.text = Mathf.RoundToInt(value * 100) + "%";
            if (!applying) PlayerPrefs.SetFloat(PreferencePrefix + "Volume", value);
        }
        void ChangeFullscreen(bool value)
        {
            if (applying) return;
            PlayerPrefs.SetInt(PreferencePrefix + "Fullscreen", value ? 1 : 0);
            ApplyScreen(value); Sound();
        }
        static void ApplyScreen(bool value)
        {
            if (!Application.isEditor && !Testing)
                Screen.fullScreenMode = value ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;
        }
        public void RestoreDefaults()
        {
            volume.value = .65f; fullscreen.isOn = false; SaveOptions(); Sound();
        }
        public void SaveOptions() { PlayerPrefs.Save(); }
        void OnApplicationQuit() { SaveOptions(); }
        void ExitGame()
        {
            SaveOptions(); Time.timeScale = 1;
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
        void Sound() { if (uiAudio && click) uiAudio.PlayOneShot(click, .35f); }
        void CreateClick()
        {
            const int count = 2205;
            var samples = new float[count];
            for (int i = 0; i < count; i++)
                samples[i] = Mathf.Sin(2 * Mathf.PI * 740 * i / 44100f) * Mathf.Pow(1 - i / (float)count, 3) * .3f;
            click = AudioClip.Create("Confirmación de menú", count, 1, 44100, false);
            click.SetData(samples, 0);
        }
        void OnDestroy() { if (click) Destroy(click); }
    }
}
