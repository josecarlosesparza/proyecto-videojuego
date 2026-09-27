using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
namespace NexoAvance {
public class CombatSmokeTest : MonoBehaviour {
    static bool running;
    bool failed;
    string output;
    static bool Testing => Array.IndexOf(Environment.GetCommandLineArgs(),"-nexoCombatTest")>=0;
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Boot() { if(Testing) {var go=new GameObject("Pruebas de combate");DontDestroyOnLoad(go);go.AddComponent<CombatSmokeTest>();} }
    IEnumerator Start() {
        if(!Testing || running){Destroy(this);yield break;}running=true;
        Application.runInBackground=true;
        var a=Environment.GetCommandLineArgs();int i=Array.IndexOf(a,"-nexoOutput");output=a[i+1];
        File.WriteAllText(Path.Combine(output,"pruebas-combate.txt"),"");
        yield return SceneManager.LoadSceneAsync("PrimerAvance");
        yield return null;
        var session=FindAnyObjectByType<CombatSession>();var player=session.player;var robot=session.enemy;
        var gun=player.GetComponent<PlayerCombat>();var body=player.GetComponent<Rigidbody2D>();
        player.TestMode=true;player.SetPaused(false);robot.AutoFire=false;
        yield return new WaitForSeconds(.4f);
        Check(session.health.Current==100 && session.healthFill.fillAmount==1,"Inicio con 100 de salud y barra completa");
        SetPosition(body,new Vector2(6,-.15f));player.visual.flipX=false;
        Check(gun.TryShoot() && !gun.TryShoot(),"Disparo y limite de cuatro balas por segundo");
        yield return new WaitForSeconds(.4f);
        Check(robot.health.Current==50 && session.health.Current==100,"La bala del jugador dana al robot sin herir a su emisor (robot="+robot.health.Current+", jugador="+session.health.Current+")");
        player.visual.flipX=true;gun.TryShoot();
        yield return new WaitForSeconds(.35f);
        Check(robot.health.Current==50,"Al mirar a la izquierda las balas viajan en esa direccion");
        CombatBullet.ClearAll();player.visual.flipX=false;
        SetPosition(body,new Vector2(3,-.15f));robot.transform.position=new Vector3(6,-.15f,0);Physics2D.SyncTransforms();
        gun.TryShoot();yield return new WaitForSeconds(.4f);
        Check(robot.health.Current==50 && FindObjectsByType<CombatBullet>(FindObjectsSortMode.None).Length==0,"La caja intercepta las balas del jugador");
        Check(!robot.HasLineOfSight(),"El enemigo no detecta al jugador a traves de la caja");
        robot.Fire();yield return new WaitForSeconds(.4f);
        Check(session.health.Current==100,"La caja tambien bloquea las balas enemigas");
        session.Restart();robot.AutoFire=true;SetPosition(body,new Vector2(6,-.15f));
        yield return new WaitForSeconds(.8f);
        Check(robot.IsCharging && robot.warning.enabled,"El robot avisa antes de disparar");
        player.SetPaused(true);int shots=CombatBullet.ShotsCreated;
        Check(!gun.TryShoot() && !robot.Fire(),"Ambos disparos quedan bloqueados durante la pausa");
        yield return new WaitForSecondsRealtime(.25f);
        Check(CombatBullet.ShotsCreated==shots && session.health.Current==100,"El aviso y el combate se detienen en pausa");
        player.SetPaused(false);
        yield return new WaitForSeconds(1.1f);
        robot.AutoFire=false;
        Check(session.health.Current==85 && Mathf.Abs(session.healthFill.fillAmount-.85f)<.001f,"Una bala enemiga quita 15 puntos y actualiza la barra");
        CombatBullet.ClearAll();
        Capture("combate-salud-85.png");
        body.gravityScale=0;SetPosition(body,new Vector2(6,2.5f));robot.Fire();
        yield return new WaitForSeconds(.8f);
        Check(session.health.Current==85,"Las balas horizontales se pueden esquivar por arriba");
        body.gravityScale=3;SetPosition(body,new Vector2(6,-.15f));CombatBullet.ClearAll();
        session.health.Hit(1000,CombatTeam.Robot);
        Check(session.health.Current==0 && session.defeatPanel.activeSelf && !player.enabled && Time.timeScale==0,"Cero salud causa derrota y detiene al jugador");
        Check(!gun.TryShoot(),"El jugador derrotado no puede disparar");
        Capture("combate-derrota.png");
        session.retryButton.onClick.Invoke();
        Check(session.health.Current==100 && player.enabled && body.simulated && Time.timeScale==1 && !session.defeatPanel.activeSelf,"Reintentar restaura salud, fisicas y controles");
        Check(robot.health.Current==60 && robot.visual.enabled && FindObjectsByType<CombatBullet>(FindObjectsSortMode.None).Length==0,"Reintentar restaura al enemigo y elimina las balas");
        robot.AutoFire=false;SetPosition(body,new Vector2(6,-.15f));player.visual.flipX=false;
        for(int n=0;n<6;n++){gun.TryShoot();yield return new WaitForSeconds(.7f);}
        Check(robot.health.Current==0 && !robot.visual.enabled && !robot.GetComponent<Collider2D>().enabled,"Seis impactos destruyen al robot y desactivan su colision (salud="+robot.health.Current+")");
        Check(!robot.Fire(),"El enemigo derrotado deja de disparar");
        Capture("combate-robot-eliminado.png");
        session.Restart();robot.AutoFire=false;
        Check(robot.health.Current==60 && robot.visual.enabled,"R o reiniciar permiten volver a enfrentar al enemigo");
        var moving=CombatBullet.Fire(new Vector2(20,4),1,CombatTeam.Player,10,10,gun.pixel,gun.material);
        player.SetPaused(true);Vector3 pos=moving.transform.position;
        yield return new WaitForSecondsRealtime(.2f);
        Check(moving && moving.transform.position==pos,"Las balas en vuelo quedan inmoviles al pausar");
        player.SetPaused(false);CombatBullet.ClearAll();
        SetPosition(body,new Vector2(6,-.15f));session.health.Hit(30,CombatTeam.Robot);
        yield return new WaitForSeconds(.2f);Capture("combate.png");
        File.AppendAllText(Path.Combine(output,"pruebas-combate.txt"),failed?"RESULTADO: ERROR\n":"RESULTADO: CORRECTO\n");
        Application.Quit(failed?1:0);
    }
    void Check(bool ok,string message){if(!ok)failed=true;File.AppendAllText(Path.Combine(output,"pruebas-combate.txt"),(ok?"OK  ":"ERROR  ")+message+"\n");}
    static void SetPosition(Rigidbody2D body,Vector2 p){body.transform.position=p;body.position=p;body.linearVelocity=Vector2.zero;Physics2D.SyncTransforms();}
    void Capture(string filename) {
        Canvas.ForceUpdateCanvases();var camera=Camera.main;var rt=new RenderTexture(1280,720,24,RenderTextureFormat.ARGB32);rt.Create();
        var old=RenderTexture.active;
        try{
            RenderPipeline.SubmitRenderRequest(camera,new RenderPipeline.StandardRequest{destination=rt});
            RenderTexture.active=rt;var t=new Texture2D(1280,720,TextureFormat.RGB24,false);
            t.ReadPixels(new Rect(0,0,1280,720),0,0);t.Apply();File.WriteAllBytes(Path.Combine(output,filename),t.EncodeToPNG());Destroy(t);
        }finally{RenderTexture.active=old;rt.Release();Destroy(rt);}
    }
}
}

