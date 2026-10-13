using System;
using System.IO;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;
namespace NexoAvance {
public class DaylightLevelCheck:MonoBehaviour {
    const string Out="C:/Users/jose/Documents/Codex/2026-09-22/ho/outputs/EscenarioDia/";
    bool failed;
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Boot(){if(Array.IndexOf(Environment.GetCommandLineArgs(),"-daylightCheck")>=0)new GameObject("Verificación diurna").AddComponent<DaylightLevelCheck>();}
    void Check(bool condition,string label){failed|=!condition;File.AppendAllText(Out+"validacion.txt",(condition?"OK ":"ERROR ")+label+"\n");}
    IEnumerator Start(){
        File.WriteAllText(Out+"validacion.txt","");Application.runInBackground=true;
        var mission=FindAnyObjectByType<StreetMission>();var player=mission.session.player;player.TestMode=true;player.SetPaused(false);mission.boss.AutoFire=false;
        var enemies=FindObjectsByType<StreetEnemy>(FindObjectsInactive.Include,FindObjectsSortMode.None);
        foreach(var enemy in enemies){enemy.AutoFire=false;enemy.GetComponent<Collider2D>().enabled=false;}
        Check(SceneManager.GetActiveScene().name=="CallesBajoControl_Dia","Se ejecuta la copia diurna");
        Check(enemies.Length==24,"Se conservan los enemigos originales, incluidas las oleadas inactivas");
        var layers=FindObjectsByType<DaylightParallax>(FindObjectsSortMode.None);Check(layers.Length==4,"Cuatro capas con parallax");
        bool decorative=true;foreach(var l in layers)decorative &=l.GetComponentsInChildren<Collider2D>(true).Length==0;
        Check(decorative,"La decoración del fondo no tiene colisiones");
        bool floor=true;for(float x=-1;x<200;x+=.5f){bool support=false;foreach(var hit in Physics2D.RaycastAll(new Vector2(x,8),Vector2.down,12))if(hit.collider.name=="Avenida")support=true;floor &= support;}
        Check(floor,"Suelo continuo en todo el recorrido");
        int platforms=0;foreach(var t in FindObjectsByType<Transform>(FindObjectsSortMode.None))if(t.name.StartsWith("PLATAFORMA ·"))platforms++;
        Check(platforms==5,"Cinco plataformas bajas con colisión");
        bool safeCheckpoints=true;
        foreach(var sector in mission.sectors)foreach(var c in Physics2D.OverlapBoxAll(new Vector2(sector.checkpoint,-.15f),player.GetComponent<BoxCollider2D>().size*.9f,0))if(c.gameObject!=player.gameObject && !c.GetComponentInParent<CombatHealth>())safeCheckpoints=false;
        Check(safeCheckpoints,"Los puntos de reaparición están libres de obstáculos");
        float oldX=Camera.main.transform.position.x;bool smooth=true,parallax=true;float start=Time.realtimeSinceStartup;int zone=0;float[] shots={12,62,115,154};
        player.TestAxis=1;
        while(player.transform.position.x<176 && Time.realtimeSinceStartup-start<90){
            if(player.Grounded){bool blocked=false;foreach(float dy in new[]{-.66f,-.1f,.55f})foreach(var hit in Physics2D.RaycastAll((Vector2)player.transform.position+new Vector2(.35f,dy),Vector2.right,1.35f))if(hit.collider && hit.collider.gameObject!=player.gameObject && !hit.collider.GetComponentInParent<CombatHealth>() && hit.collider.name!="Avenida")blocked=true;if(blocked)player.TestJump=true;}
            yield return new WaitForEndOfFrame();
            float cameraX=Camera.main.transform.position.x;smooth &=cameraX>=oldX-.02f && Mathf.Abs(cameraX-oldX)<Mathf.Max(1.2f,Time.deltaTime*8);oldX=cameraX;
            foreach(var l in layers)parallax &=Mathf.Abs(l.transform.position.x-(cameraX-8)*l.followFraction)<.025f;
            if(zone<shots.Length && player.transform.position.x>=shots[zone]){Capture("zona-"+(zone+1)+".png");zone++;}
            if(player.transform.position.y< -3){Check(false,"El jugador cayó fuera del recorrido");break;}
        }
        player.TestAxis=0;yield return new WaitForSeconds(.3f);
        Check(player.transform.position.x>=176,"Recorrido físico de 175 unidades superado caminando y saltando");
        File.AppendAllText(Out+"validacion.txt","Posición final: "+player.transform.position+"; tiempo de recorrido: "+(Time.realtimeSinceStartup-start).ToString("F1")+" s\n");
        Check(zone==4,"Se atravesaron las cuatro zonas conectadas");Check(smooth,"Cámara continua sin saltos entre zonas");Check(parallax,"Parallax sincronizado después de la cámara");
        Check(player.GetComponent<GrenadeInventory>().Collected,"Se conserva el suministro de granadas del camión");
        // Bring the camera to both limits and then restart, checking parallax rebases without accumulation.
        var body=player.GetComponent<Rigidbody2D>();body.position=new Vector2(199,-.15f);player.transform.position=body.position;yield return new WaitForSeconds(1.5f);
        Check(Mathf.Abs(Camera.main.transform.position.x-191)<.1f,"Límite derecho de cámara conservado");
        body.position=new Vector2(1,-.15f);player.transform.position=body.position;yield return new WaitForSeconds(1.5f);Check(Mathf.Abs(Camera.main.transform.position.x-8)<.1f,"Regreso de cámara al inicio sin deriva");
        // Verify the new scenery did not break a shot against the original first enemy.
        var target=enemies[0];foreach(var e in enemies)if(e.kind==StreetEnemyKind.Fusilero && Mathf.Abs(e.transform.position.x-19)<.5f){target=e;break;}
        target.GetComponent<Collider2D>().enabled=true;target.gameObject.SetActive(true);body.position=new Vector2(17,-.15f);player.transform.position=body.position;body.linearVelocity=Vector2.zero;player.visual.flipX=false;Physics2D.SyncTransforms();yield return new WaitForSeconds(.2f);
        int hp=target.health.Current;player.GetComponent<PlayerCombat>().TryShoot();yield return new WaitForSeconds(.5f);Check(target.health.Current<hp,"Los disparos siguen dañando al fusilero");
        Check(target.visual.sprite && target.visual.enabled,"El robot naranja mantiene su sprite visible");
        File.AppendAllText(Out+"validacion.txt",failed?"RESULTADO: ERROR":"RESULTADO: CORRECTO");Application.Quit(failed?1:0);
    }
    void Capture(string name){Canvas.ForceUpdateCanvases();var rt=new RenderTexture(1280,720,24);rt.Create();RenderPipeline.SubmitRenderRequest(Camera.main,new RenderPipeline.StandardRequest{destination=rt});var old=RenderTexture.active;RenderTexture.active=rt;var t=new Texture2D(1280,720,TextureFormat.RGB24,false);t.ReadPixels(new Rect(0,0,1280,720),0,0);t.Apply();File.WriteAllBytes(Out+name,t.EncodeToPNG());RenderTexture.active=old;rt.Release();Destroy(rt);Destroy(t);}
}
}
