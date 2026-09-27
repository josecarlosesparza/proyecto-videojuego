using UnityEngine;
using UnityEngine.SceneManagement;
namespace NexoAvance {
public class AvanceView : MonoBehaviour {
    public ResistancePlayer player;
    public Camera worldCamera;
    float cameraVelocity;
    bool confirmMenu, leaving;
    public bool ConfirmingMenu => confirmMenu;
    GUIStyle title, small, button;
    void LateUpdate() {
        float x = Mathf.Clamp(player.transform.position.x + 3, 8, 29);
        var p = worldCamera.transform.position;
        p.x = Mathf.SmoothDamp(p.x, x, ref cameraVelocity, .18f);
        worldCamera.transform.position = p;
        if (!player.Paused) confirmMenu = false;
    }
    public void RequestMenu() { if (player.Paused) confirmMenu = true; }
    public void CancelReturn() { confirmMenu = false; }
    public void ConfirmReturn() {
        if (!confirmMenu || leaving) return;
        leaving = true;
        player.SetPaused(false);
        SceneManager.LoadSceneAsync("MenuPrincipal", LoadSceneMode.Single);
    }
    void OnGUI() {
        if (title == null) {
            title = Style(28, new Color(.9f,.95f,1), FontStyle.Bold);
            small = Style(13, new Color(.8f,.85f,.9f));
            button = new GUIStyle(GUI.skin.button) { fontSize = 17 };
        }
        GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(Screen.width / 960f, Screen.height / 540f, 1));
        if (!player.Paused || leaving) return;
        Panel(new Rect(0,0,960,540), new Color(.01f,.025f,.05f,.9f));
        if (confirmMenu) {
            GUI.Label(new Rect(280,171,600,42), "¿VOLVER AL MENÚ?", title);
            GUI.Label(new Rect(280,218,600,28), "Se perderá el avance de este recorrido.", small);
            if (GUI.Button(new Rect(300,275,360,44), "Cancelar", button)) CancelReturn();
            if (GUI.Button(new Rect(300,332,360,44), "Confirmar y volver al menú", button)) ConfirmReturn();
            return;
        }
        GUI.Label(new Rect(365,164,300,40), "EN PAUSA", title);
        if (GUI.Button(new Rect(335,223,290,42), "Continuar", button)) player.SetPaused(false);
        if (GUI.Button(new Rect(335,277,290,42), "Reiniciar recorrido", button)) { player.Respawn(); player.SetPaused(false); }
        if (GUI.Button(new Rect(335,331,290,42), "Volver al menú principal", button)) RequestMenu();
    }
    static GUIStyle Style(int size, Color color, FontStyle weight = FontStyle.Normal) {
        var s = new GUIStyle(GUI.skin.label) { fontSize = size, fontStyle = weight }; s.normal.textColor = color; return s;
    }
    static void Panel(Rect r, Color color) { GUI.color = color; GUI.DrawTexture(r, Texture2D.whiteTexture); GUI.color = Color.white; }
}
}


