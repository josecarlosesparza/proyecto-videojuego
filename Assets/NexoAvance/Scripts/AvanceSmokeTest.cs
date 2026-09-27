using System.Collections;
using System.IO;
using UnityEngine;
namespace NexoAvance {
public class AvanceSmokeTest : MonoBehaviour {
    public ResistancePlayer player;
    bool failed;
    string report;
    IEnumerator Start() {
        if (System.Array.IndexOf(System.Environment.GetCommandLineArgs(), "-nexoSmokeTest") < 0) yield break;
        player.TestMode = true;
        player.SetPaused(false);
        Application.runInBackground = true;
        yield return new WaitForSeconds(.7f);
        Check(player.Grounded, "El personaje aterriza sobre el suelo");
        float startX=player.transform.position.x;
        player.TestAxis=1;
        yield return new WaitForSeconds(1f);
        Check(player.transform.position.x > startX + 2, "Movimiento a la derecha");
        Check(player.transform.position.x < 4.1f, "La caja bloquea el movimiento");
        player.TestJump=true;
        yield return new WaitForSeconds(.22f);
        Check(player.transform.position.y > .9f && !player.Grounded, "Salto desde el suelo");
        float before=player.Velocity.y;
        player.TestJump=true;
        yield return new WaitForSeconds(.06f);
        Check(player.Velocity.y < before, "No se permite un segundo salto en el aire");
        yield return new WaitForSeconds(.95f);
        Check(player.transform.position.x > 6, "El salto supera la caja");
        player.TestAxis=0;
        var rb=player.GetComponent<Rigidbody2D>();
        rb.position=new Vector2(8,4); rb.linearVelocity=Vector2.zero;
        yield return new WaitForSeconds(.85f);
        Check(player.Grounded && player.transform.position.y > .9f && player.transform.position.y < 1.2f, "Aterrizaje sobre plataforma");
        player.TestAxis=-1;
        yield return new WaitForSeconds(.25f);
        Check(player.visual.flipX && player.Velocity.x < -1, "Orientación y movimiento a la izquierda");
        player.TestAxis=0;
        player.SetPaused(true);
        Vector3 pausePosition=player.transform.position;
        yield return new WaitForSecondsRealtime(.2f);
        Check(Vector3.Distance(pausePosition,player.transform.position)<.01f,"La pausa detiene la física");
        player.SetPaused(false); player.Respawn();
        yield return new WaitForSeconds(.5f);
        Check(Mathf.Abs(player.transform.position.x-1)<.1f && player.Grounded, "Reinicio de posición y velocidad");
        rb.position=new Vector2(1,-8);
        yield return new WaitForSeconds(.6f);
        Check(player.transform.position.y > -1, "Recuperación al caer fuera del escenario");
        player.TestAxis=1; player.TestJump=true;
        yield return new WaitForSeconds(.22f);
        ScreenCapture.CaptureScreenshot(Path.Combine(Application.dataPath,"../../NEXO-captura.png"));
        yield return new WaitForSeconds(.3f);
        File.WriteAllText(Path.Combine(Application.dataPath,"../../NEXO-pruebas.txt"),report);
        Debug.Log(failed ? "NEXO_TESTS_FAILED" : "NEXO_TESTS_OK");
        Application.Quit(failed ? 1 : 0);
    }
    void Check(bool ok,string message) {
        report+=(ok ? "OK  " : "ERROR  ")+message+"\n";
        if (!ok) failed=true;
        Debug.Log((ok ? "PASS: " : "FAIL: ")+message);
    }
}
}
