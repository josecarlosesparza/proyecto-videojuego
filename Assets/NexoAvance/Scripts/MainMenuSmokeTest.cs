using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
namespace NexoAvance {
public class MainMenuSmokeTest : MonoBehaviour {
    static bool running;
    bool failed;
    string output;
    IEnumerator Start() {
        if (!MainMenuController.Testing || running) { Destroy(gameObject); yield break; }
        running=true; DontDestroyOnLoad(gameObject); Application.runInBackground=true;
        var args=Environment.GetCommandLineArgs(); int index=Array.IndexOf(args,"-nexoOutput");
        output=index>=0 && index+1<args.Length ? args[index+1] : Application.persistentDataPath;
        Directory.CreateDirectory(output); File.WriteAllText(Path.Combine(output,"pruebas-menu.txt"),"");
        yield return null; yield return null;
        var menu=FindFirstObjectByType<MainMenuController>();
        Check(menu && SceneManager.GetActiveScene().buildIndex==0 && menu.CurrentPage=="Inicio","El juego inicia en el menú principal");
        Check(EventSystem.current && EventSystem.current.currentSelectedGameObject==menu.playButton.gameObject,"Jugar tiene el foco inicial del teclado");
        Check(menu.playButton.FindSelectableOnDown()==menu.controlsButton && menu.controlsButton.FindSelectableOnRight()==menu.optionsButton,"Navegación con flechas entre opciones");
        Capture("menu-principal.png");
        menu.controlsButton.onClick.Invoke();
        Check(menu.controls.activeSelf && !menu.home.activeSelf,"Controles abre la ayuda y oculta el inicio");
        Capture("menu-controles.png"); menu.controlsBack.onClick.Invoke();
        menu.optionsButton.onClick.Invoke();
        Check(menu.options.activeSelf && EventSystem.current.currentSelectedGameObject==menu.volume.gameObject,"Opciones selecciona el control de volumen");
        menu.volume.value=.3f;
        Check(Mathf.Abs(AudioListener.volume-.3f)<.001f && menu.volumeValue.text=="30%","El volumen cambia y muestra su valor");
        menu.fullscreen.isOn=true; menu.optionsBack.onClick.Invoke();
        Check(PlayerPrefs.GetInt(MainMenuController.PreferencePrefix+"Fullscreen")==1,"La preferencia de pantalla completa se guarda");
        yield return SceneManager.LoadSceneAsync("MenuPrincipal"); yield return null;
        menu=FindFirstObjectByType<MainMenuController>();
        Check(Mathf.Abs(menu.volume.value-.3f)<.001f && menu.fullscreen.isOn,"Los ajustes sobreviven a la recarga del menú");
        menu.optionsButton.onClick.Invoke(); menu.defaultsButton.onClick.Invoke();
        Check(Mathf.Abs(menu.volume.value-.65f)<.001f && !menu.fullscreen.isOn,"Restablecer recupera los ajustes iniciales");
        Capture("menu-opciones.png"); menu.optionsBack.onClick.Invoke();
        menu.creditsButton.onClick.Invoke(); Check(menu.credits.activeSelf,"Créditos muestra la autoría");
        Capture("menu-creditos.png"); menu.creditsBack.onClick.Invoke();
        menu.exitButton.onClick.Invoke(); Check(menu.exitConfirm.activeSelf,"Salir solicita confirmación");
        Capture("menu-salir.png"); menu.cancelExit.onClick.Invoke();
        Check(menu.CurrentPage=="Inicio" && !menu.exitConfirm.activeSelf,"Cancelar salida vuelve al inicio");
        menu.playButton.onClick.Invoke(); menu.characterSelection.confirm.onClick.Invoke(); menu.characterSelection.confirm.onClick.Invoke();
        Check(menu.IsLoading,"Jugar bloquea las pulsaciones repetidas durante la carga");
        float deadline=Time.realtimeSinceStartup+20;
        while(SceneManager.GetActiveScene().name!="CallesBajoControl" && Time.realtimeSinceStartup<deadline) yield return null;
        yield return null;
        var player=FindFirstObjectByType<ResistancePlayer>();
        Check(player && FindObjectsByType<ResistancePlayer>(FindObjectsSortMode.None).Length==1,"Jugar carga un único personaje en CallesBajoControl");
        if(player) {
            player.TestMode=true; player.SetPaused(false);
            yield return new WaitForSeconds(.5f);
            var view=FindFirstObjectByType<AvanceView>(); player.SetPaused(true); view.RequestMenu();
            Check(view.ConfirmingMenu && Time.timeScale==0,"Volver desde pausa requiere confirmar");
            view.CancelReturn(); Check(!view.ConfirmingMenu && player.Paused,"Cancelar mantiene el juego pausado");
            view.RequestMenu(); view.ConfirmReturn();
            deadline=Time.realtimeSinceStartup+20;
            while(SceneManager.GetActiveScene().name!="MenuPrincipal" && Time.realtimeSinceStartup<deadline) yield return null;
            yield return null;
            Check(SceneManager.GetActiveScene().name=="MenuPrincipal" && Time.timeScale==1,"Volver al menú restablece el tiempo del juego");
        }
        PlayerPrefs.DeleteKey(MainMenuController.PreferencePrefix+"Volume");
        PlayerPrefs.DeleteKey(MainMenuController.PreferencePrefix+"Fullscreen"); PlayerPrefs.Save();
        File.AppendAllText(Path.Combine(output,"pruebas-menu.txt"),failed?"RESULTADO: ERROR\n":"RESULTADO: CORRECTO\n");
        Application.Quit(failed?1:0);
    }
    void Check(bool ok,string description) {
        if(!ok) failed=true;
        File.AppendAllText(Path.Combine(output,"pruebas-menu.txt"),(ok?"OK  ":"ERROR  ")+description+"\n");
    }
    void Capture(string filename) {
        Canvas.ForceUpdateCanvases();
        var camera=Camera.main; var rt=new RenderTexture(1280,720,24,RenderTextureFormat.ARGB32); rt.Create();
        var old=RenderTexture.active;
        try {
            if(GraphicsSettings.currentRenderPipeline!=null)
                RenderPipeline.SubmitRenderRequest(camera,new RenderPipeline.StandardRequest{destination=rt});
            else { camera.targetTexture=rt;camera.Render();camera.targetTexture=null; }
            RenderTexture.active=rt;
            var tex=new Texture2D(1280,720,TextureFormat.RGB24,false);
            tex.ReadPixels(new Rect(0,0,1280,720),0,0);tex.Apply();
            File.WriteAllBytes(Path.Combine(output,filename),tex.EncodeToPNG()); Destroy(tex);
        } finally { RenderTexture.active=old; rt.Release(); Destroy(rt); }
    }
}
}
