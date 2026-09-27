using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
namespace NexoAvance {
public class CombatSession : MonoBehaviour {
    public ResistancePlayer player;
    public CombatHealth health;
    public RobotShooter enemy;
    public Image healthFill;
    public Text healthLabel, objective;
    public GameObject defeatPanel;
    public Button retryButton, menuButton;
    public bool Dead => !health.Alive;
    bool leaving;
    void Awake() {
        health.Changed += UpdateHealth;
        health.Died += Defeat;
        retryButton.onClick.AddListener(Restart);
        menuButton.onClick.AddListener(BackToMenu);
    }
    void Start() { UpdateHealth(); defeatPanel.SetActive(false); }
    void Update() {
        objective.text = enemy.health.Alive ? "OBJETIVO: NEUTRALIZA AL ROBOT" : "ROBOT ELIMINADO · AVANZA HASTA LA BANDERA";
        if(!Dead) return;
        bool restart = false;
#if ENABLE_INPUT_SYSTEM
        restart = Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame;
#elif ENABLE_LEGACY_INPUT_MANAGER
        restart = Input.GetKeyDown(KeyCode.R);
#endif
        if(restart) Restart();
    }
    void UpdateHealth() {
        float fraction = health.Current / (float)health.maximum;
        healthFill.fillAmount = fraction;
        healthFill.color = fraction > .5f ? new Color(.35f,.86f,.68f) : fraction > .25f ? new Color(1,.72f,.28f) : new Color(1,.3f,.27f);
        healthLabel.text = "SALUD  " + health.Current + " / " + health.maximum;
    }
    void Defeat() {
        player.SetPaused(false);
        player.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
        player.GetComponent<Rigidbody2D>().simulated = false;
        player.enabled = false;
        Time.timeScale = 0;
        defeatPanel.SetActive(true);
        if(EventSystem.current) EventSystem.current.SetSelectedGameObject(retryButton.gameObject);
    }
    public void Restart() {
        if(leaving) return;
        player.enabled = true;
        player.GetComponent<Rigidbody2D>().simulated = true;
        player.SetPaused(false);
        player.Respawn();
    }
    // Se llama desde Respawn para cubrir R, la pausa y las caídas fuera del nivel.
    public void ResetCombat() {
        player.enabled = true; player.GetComponent<Rigidbody2D>().simulated = true;
        player.SetPaused(false); health.ResetHealth(); enemy.ResetEnemy();
        CombatBullet.ClearAll(); defeatPanel.SetActive(false); UpdateHealth();
    }
    public void BackToMenu() {
        if(leaving) return; leaving = true; Time.timeScale = 1;
        SceneManager.LoadSceneAsync("MenuPrincipal");
    }
    void OnDestroy() { if(health) { health.Changed -= UpdateHealth; health.Died -= Defeat; } Time.timeScale = 1; }
}
}
