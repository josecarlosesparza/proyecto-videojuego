using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
namespace NexoAvance {
public class MissionCheck : MonoBehaviour {
    string output;bool failed;
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Boot(){if(Array.IndexOf(Environment.GetCommandLineArgs(),"-missionCheck")<0)return;var g=new GameObject("Validación misión");DontDestroyOnLoad(g);g.AddComponent<MissionCheck>();}
    void Check(bool ok,string message){failed|=!ok;File.AppendAllText(Path.Combine(output,"validacion.txt"),(ok?"OK ":"ERROR ")+message+"\n");}
    void Place(ResistancePlayer p,float x){var b=p.GetComponent<Rigidbody2D>();b.position=new Vector2(x,-.15f);p.transform.position=b.position;b.linearVelocity=Vector2.zero;Physics2D.SyncTransforms();}
    void Kill(GameObject wave){foreach(var e in wave.GetComponentsInChildren<StreetEnemy>(true)){bool hit=e.health.Hit(10000,CombatTeam.Player);File.AppendAllText(Path.Combine(output,"diagnostico.txt"),wave.name+" / "+e.name+" hit="+hit+" hp="+e.health.Current+" time="+Time.time+" scale="+Time.timeScale+"\n");}}
    IEnumerator Start(){
        output=@"C:\Users\jose\Documents\Codex\2026-09-22\ho\outputs\Mision01";File.WriteAllText(Path.Combine(output,"validacion.txt"),"");Application.runInBackground=true;
        yield return null;var menu=FindAnyObjectByType<MainMenuController>();Check(menu && menu.gameScene=="CallesBajoControl","Jugar abre la misión nueva");menu.playButton.onClick.Invoke();
        while(SceneManager.GetActiveScene().name!="CallesBajoControl")yield return null;yield return null;
        var m=FindAnyObjectByType<StreetMission>();var s=m.session;var p=s.player;var gun=p.GetComponent<PlayerCombat>();p.TestMode=true;p.SetPaused(false);
        var all=FindObjectsByType<StreetEnemy>(FindObjectsInactive.Include,FindObjectsSortMode.None);int[] kinds=new int[3];foreach(var e in all){kinds[(int)e.kind]++;e.AutoFire=false;}m.boss.AutoFire=false;
        Check(all.Length==24 && kinds[0]==8 && kinds[1]==8 && kinds[2]==8,"24 enemigos: ocho de cada clase");
        Check(m.sectors.Length==4 && m.sectors[0].waves.Length==2,"Cuatro sectores con dos oleadas");
        Check(FindAnyObjectByType<AvanceView>().cameraEnd>180,"La cámara recorre toda la avenida");
        yield return new WaitForSeconds(.4f);Place(p,4.9f);p.TestAxis=1;p.TestJump=true;yield return new WaitForSeconds(.78f);p.TestAxis=0;yield return new WaitForSeconds(.2f);
        Check(p.transform.position.x>8.4f && s.health.Current==100,"El auto se salta y su fuego no daña");
        Place(p,11);yield return new WaitForSeconds(.2f);Check(m.sectors[0].waves[0].activeSelf && m.sectors[0].gate.activeSelf,"La patrulla se activa y mantiene cerrada la salida");
        Place(p,17);p.visual.flipX=false;var first=m.sectors[0].waves[0].GetComponentsInChildren<StreetEnemy>()[0];int before=first.health.Current;gun.TryShoot();yield return new WaitForSeconds(.3f);Check(first.health.Current==before-10,"Las balas del jugador hieren al fusilero");
        first.health.Hit(10000,CombatTeam.Player);Check(FindObjectsByType<EnemyExplosion>(FindObjectsSortMode.None).Length>0,"Los enemigos explotan al ser eliminados");
        Capture("avenida.png");Kill(m.sectors[0].waves[0]);yield return new WaitForSeconds(1.8f);Check(m.Wave==1 && m.sectors[0].gate.activeSelf,"Llegan refuerzos antes de abrir el sector (wave="+m.Wave+", sector="+m.Sector+", cleared="+m.Cleared(m.sectors[0].waves[0])+")");Kill(m.sectors[0].waves[1]);yield return new WaitForSeconds(1.8f);
        Check(m.Sector==1 && !m.sectors[0].gate.activeSelf && m.Checkpoint==40,"Liberar un sector abre la barrera y guarda el punto de control");
        s.health.Hit(1000,CombatTeam.Robot);Check(s.Dead && s.defeatPanel.activeSelf,"La derrota abre la pantalla de reintento");s.Restart();yield return new WaitForSeconds(.15f);
        Check(s.health.Current==100 && Mathf.Abs(p.transform.position.x-40)<.2f && !m.sectors[0].gate.activeSelf,"Reintentar conserva los sectores liberados");
        for(int sector=1;sector<4;sector++){Place(p,m.sectors[sector].trigger+1);yield return new WaitForSeconds(.2f);Kill(m.sectors[sector].waves[0]);yield return new WaitForSeconds(1.8f);Kill(m.sectors[sector].waves[1]);yield return new WaitForSeconds(1.8f);}
        Check(m.Sector==4,"Las ocho oleadas permiten llegar al jefe");Place(p,174);yield return new WaitForSeconds(.3f);
        Check(m.BossStarted && m.boss.arms.Length==8 && m.boss.health.Current==1600 && m.bossHud.activeSelf,"KRAKEN tiene ocho tentáculos, 1600 de salud y barra propia");
        Capture("kraken-01.png");m.boss.AutoFire=true;yield return new WaitForSeconds(5.5f);m.boss.AutoFire=false;Check(s.health.Current<100,"Los ataques del jefe pueden herir al jugador");CombatBullet.ClearAll();s.health.ResetHealth();p.visual.flipX=false;int bossHealth=m.boss.health.Current;gun.TryShoot();yield return new WaitForSeconds(.6f);Check(m.boss.health.Current<bossHealth,"El núcleo del jefe recibe los disparos horizontales");
        m.boss.health.Hit(10000,CombatTeam.Player);yield return new WaitForSecondsRealtime(1.3f);Check(m.Complete && m.victoryPanel.activeSelf && Time.timeScale==0,"Derrotar al jefe completa la misión");Capture("victoria.png");
        m.Replay();yield return new WaitForSeconds(.2f);Check(m.Sector==0 && !m.Complete && s.health.Current==100 && m.sectors[0].gate.activeSelf,"Repetir misión reinicia enemigos, salud y barreras");
        s.BackToMenu();while(SceneManager.GetActiveScene().name!="MenuPrincipal")yield return null;Check(Time.timeScale==1,"Volver al menú restaura el tiempo");
        File.AppendAllText(Path.Combine(output,"validacion.txt"),failed?"RESULTADO: ERROR":"RESULTADO: CORRECTO");Application.Quit(failed?1:0);
    }
    void Capture(string name){Canvas.ForceUpdateCanvases();var rt=new RenderTexture(1280,720,24);rt.Create();RenderPipeline.SubmitRenderRequest(Camera.main,new RenderPipeline.StandardRequest{destination=rt});var old=RenderTexture.active;RenderTexture.active=rt;var t=new Texture2D(1280,720,TextureFormat.RGB24,false);t.ReadPixels(new Rect(0,0,1280,720),0,0);t.Apply();File.WriteAllBytes(Path.Combine(output,name),t.EncodeToPNG());RenderTexture.active=old;rt.Release();Destroy(rt);Destroy(t);}
}
}
