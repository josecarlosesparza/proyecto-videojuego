using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;
namespace NexoAvance {
public class GrenadeCheck:MonoBehaviour {
    const string Output=@"C:\Users\jose\Documents\Codex\2026-09-22\ho\outputs\Mision01";bool failed;
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Boot(){if(Array.IndexOf(Environment.GetCommandLineArgs(),"-grenadeCheck")<0)return;var go=new GameObject("Prueba granadas");DontDestroyOnLoad(go);go.AddComponent<GrenadeCheck>();}
    void Check(bool ok,string text){failed|=!ok;File.AppendAllText(Path.Combine(Output,"validacion-granadas.txt"),(ok?"OK ":"ERROR ")+text+"\n");}
    void Place(ResistancePlayer p,float x){var b=p.GetComponent<Rigidbody2D>();b.position=new Vector2(x,-.15f);p.transform.position=b.position;b.linearVelocity=Vector2.zero;Physics2D.SyncTransforms();}
    IEnumerator Start(){
        File.WriteAllText(Path.Combine(Output,"validacion-granadas.txt"),"");Application.runInBackground=true;yield return SceneManager.LoadSceneAsync("CallesBajoControl");yield return null;
        var m=FindAnyObjectByType<StreetMission>();var p=m.session.player;p.TestMode=true;p.SetPaused(false);var inv=p.GetComponent<GrenadeInventory>();var gun=p.GetComponent<PlayerCombat>();
        foreach(var e in FindObjectsByType<StreetEnemy>(FindObjectsInactive.Include,FindObjectsSortMode.None))e.AutoFire=false;m.boss.AutoFire=false;
        Check(inv.Count==0 && !inv.TryThrow(),"El jugador empieza sin granadas y no puede lanzar");
        Check(inv.SupplyPosition.x>33 && inv.SupplyPosition.x<48 && GameObject.Find("Camión militar · suministro de granadas"),"Camión militar después de los tres primeros enemigos");
        Place(p,41.5f);yield return new WaitForSeconds(.25f);Check(inv.Count==5 && inv.Collected,"La caja entrega cinco granadas");
        p.visual.flipX=false;Check(inv.TryThrow() && inv.Count==4 && !inv.TryThrow(),"Lanzar consume una granada y respeta el intervalo");
        var thrown=FindAnyObjectByType<ThrownGrenade>();Check(thrown.GetComponent<Rigidbody2D>().linearVelocity.x>0 && thrown.GetComponent<Rigidbody2D>().linearVelocity.y>0,"La granada sale hacia adelante y hacia arriba");
        p.SetPaused(true);yield return new WaitForSecondsRealtime(1.6f);Check(thrown,"La pausa detiene la mecha");p.SetPaused(false);yield return new WaitForSeconds(1.7f);Check(!thrown,"La granada explota y desaparece al terminar la mecha");
        Place(p,39);yield return new WaitForSeconds(.1f);Place(p,41.5f);yield return new WaitForSeconds(.1f);Check(inv.Count==4,"La caja no repone granadas al volver a pasar");
        var enemies=m.sectors[0].waves[0].GetComponentsInChildren<StreetEnemy>();
        enemies[0].transform.position=new Vector3(58,-.15f,0);enemies[1].transform.position=new Vector3(59,-.15f,0);enemies[2].transform.position=new Vector3(65,-.15f,0);Physics2D.SyncTransforms();
        Place(p,58.5f);int hp=m.session.health.Current;var blast=ThrownGrenade.Launch(new Vector2(58.5f,-.15f),1,gun);blast.Explode();
        Check(!enemies[0].health.Alive && !enemies[1].health.Alive && enemies[2].health.Alive,"La explosión elimina varios enemigos y respeta el radio");Check(m.session.health.Current==hp,"La explosión no daña al jugador");Check(gun.HasMachineGun,"Las bajas por granada también cuentan para obtener la metralleta");
        Place(p,174);yield return new WaitForSeconds(.2f);int bossHP=m.boss.health.Current;blast=ThrownGrenade.Launch(m.boss.transform.position,1,gun);blast.Explode();Check(m.boss.health.Current==bossHP-180,"La explosión inflige 180 puntos al jefe");
        m.session.Restart();yield return new WaitForSeconds(.1f);Check(inv.Count==4 && inv.Collected && FindObjectsByType<ThrownGrenade>(FindObjectsSortMode.None).Length==0,"Reintentar conserva el inventario y limpia las granadas activas");
        m.Replay();yield return new WaitForSeconds(.1f);Check(inv.Count==0 && !inv.Collected,"Una misión nueva restablece cero granadas y la caja");
        Place(p,41.5f);yield return new WaitForSeconds(1.1f);var cam=Camera.main;cam.transform.position=new Vector3(44,2,-10);Capture(cam);
        File.AppendAllText(Path.Combine(Output,"validacion-granadas.txt"),failed?"RESULTADO: ERROR":"RESULTADO: CORRECTO");Application.Quit(failed?1:0);
    }
    void Capture(Camera camera){Canvas.ForceUpdateCanvases();var rt=new RenderTexture(1280,720,24);rt.Create();RenderPipeline.SubmitRenderRequest(camera,new RenderPipeline.StandardRequest{destination=rt});var old=RenderTexture.active;RenderTexture.active=rt;var t=new Texture2D(1280,720,TextureFormat.RGB24,false);t.ReadPixels(new Rect(0,0,1280,720),0,0);t.Apply();File.WriteAllBytes(Path.Combine(Output,"camion-granadas.png"),t.EncodeToPNG());RenderTexture.active=old;rt.Release();Destroy(rt);Destroy(t);}
}
}
