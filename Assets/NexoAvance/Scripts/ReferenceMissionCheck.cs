using System;
using System.IO;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;
namespace NexoAvance {
public class ReferenceMissionCheck:MonoBehaviour {
    const string Out="C:/Users/jose/Documents/Codex/2026-09-22/ho/outputs/Mision1Referencia/";
    bool failed;
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Boot(){if(Array.IndexOf(Environment.GetCommandLineArgs(),"-referenceCheck")<0)return;var go=new GameObject("Revisión misión referencia");DontDestroyOnLoad(go);go.AddComponent<ReferenceMissionCheck>();}
    void Check(bool ok,string label){failed|=!ok;File.AppendAllText(Out+"validacion.txt",(ok?"OK ":"ERROR ")+label+"\n");}
    void Place(ResistancePlayer player,float x){var body=player.GetComponent<Rigidbody2D>();body.position=new Vector2(x,-.15f);player.transform.position=body.position;body.linearVelocity=Vector2.zero;Physics2D.SyncTransforms();}
    IEnumerator Start(){
        File.WriteAllText(Out+"validacion.txt","");Application.runInBackground=true;yield return null;
        var menu=FindAnyObjectByType<MainMenuController>();Check(menu && menu.gameScene=="CallesBajoControl","Jugar apunta a la misión 1 modificada");menu.Play();while(SceneManager.GetActiveScene().name!="CallesBajoControl")yield return null;yield return null;
        var mission=FindAnyObjectByType<StreetMission>();var player=mission.session.player;player.TestMode=true;player.SetPaused(false);mission.boss.AutoFire=false;mission.boss.GetComponent<Collider2D>().enabled=false;
        var enemies=FindObjectsByType<StreetEnemy>(FindObjectsInactive.Include,FindObjectsSortMode.None);foreach(var e in enemies){e.AutoFire=false;e.GetComponent<Collider2D>().enabled=false;}
        var backdrop=FindAnyObjectByType<ReferenceBackdrop>();Check(backdrop && backdrop.panels.Length==6,"Seis panoramas cubren toda la misión");
        Check(backdrop.panels.Select(s=>s.sprite.texture).Distinct().Count()==6,"Cada panorama tiene un recurso diferente");
        Check(backdrop.panels.All(s=>s.sprite && s.sharedMaterial.shader.isSupported && s.enabled),"Imágenes y shader de transición válidos");
        bool joined=true,aligned=true;for(int i=0;i<6;i++){var bounds=backdrop.panels[i].bounds;aligned&=Mathf.Abs(bounds.min.y+bounds.size.y*.2f+1)<.015f;if(i>0)joined&=Mathf.Abs(backdrop.panels[i-1].bounds.max.x-bounds.min.x-2.25f)<.015f;}
        Check(joined,"Cinco uniones con solape continuo, sin huecos");Check(aligned,"Camino visible alineado con Y=-1 en los seis tramos");
        Check(FindObjectsByType<DaylightParallax>(FindObjectsSortMode.None).Length==0,"Las imágenes aplanadas permanecen unidas, sin parallax artificial");
        var world=GameObject.Find("Avenida · Calles Bajo Control").transform;string[] old={"Edificio","Ventana","Fachada","Cornisa","Persiana","Lámina","Poste","Farola","Pavimento","Borde de acera"};Check(!world.Cast<Transform>().Any(t=>old.Contains(t.name)),"No quedan edificios ni adornos del escenario anterior");
        Check(!world.Find("Avenida").GetComponent<SpriteRenderer>().enabled,"El suelo geométrico anterior no tapa el recurso original");
        bool floor=true;for(float x=-1;x<201;x+=.5f)floor&=Physics2D.RaycastAll(new Vector2(x,8),Vector2.down,12).Any(h=>h.collider.name=="Avenida");Check(floor,"Colisión de suelo continua hasta el final");
        bool checkpoints=true;foreach(var s in mission.sectors)foreach(var c in Physics2D.OverlapBoxAll(new Vector2(s.checkpoint,-.15f),player.GetComponent<BoxCollider2D>().size*.9f,0))if(c.gameObject!=player.gameObject&&!c.GetComponentInParent<CombatHealth>())checkpoints=false;Check(checkpoints,"Puntos de reaparición libres");
        float[] shots={13,30.075f,47,64.025f,81,97.975f,115,131.925f,149,165.875f,184,190.9f};int next=0;float start=Time.realtimeSinceStartup,previous=8;bool smooth=true;player.TestAxis=1;
        while(player.transform.position.x<199 && Time.realtimeSinceStartup-start<90){
            if(player.Grounded){bool block=false;foreach(float dy in new[]{-.66f,-.1f,.55f})foreach(var hit in Physics2D.RaycastAll((Vector2)player.transform.position+new Vector2(.35f,dy),Vector2.right,1.35f))if(hit.collider.gameObject!=player.gameObject&&!hit.collider.GetComponentInParent<CombatHealth>()&&hit.collider.name!="Avenida")block=true;if(block)player.TestJump=true;}
            yield return new WaitForEndOfFrame();float x=Camera.main.transform.position.x;smooth&=x>=previous-.02f&&Mathf.Abs(x-previous)<Mathf.Max(1.2f,Time.deltaTime*8);previous=x;
            if(next<shots.Length&&x>=shots[next]){Capture("recorrido-"+next.ToString("00")+".png");next++;}
        }
        player.TestAxis=0;yield return new WaitForSeconds(.5f);if(next==11){Capture("recorrido-11.png");next++;}
        Check(player.transform.position.x>=199,"Recorrido físico completo, desde X=1 hasta X=199");Check(next==12,"Capturadas todas las zonas y las cinco uniones");Check(smooth,"Cámara continua en todas las transiciones");Check(player.GetComponent<GrenadeInventory>().Collected,"Suministro de granadas conservado");
        Check(Camera.main.transform.position.x+Camera.main.orthographicSize*Camera.main.aspect<backdrop.right,"Fondo cubre el límite derecho de cámara");
        Place(player,17);yield return new WaitForSeconds(1.5f);var target=enemies.First(e=>e.kind==StreetEnemyKind.Fusilero&&Mathf.Abs(e.transform.position.x-19)<.5f);target.GetComponent<Collider2D>().enabled=true;player.visual.flipX=false;int hp=target.health.Current;player.GetComponent<PlayerCombat>().TryShoot();yield return new WaitForSeconds(.5f);Check(target.health.Current<hp,"Los disparos del jugador dañan al enemigo");
        int playerHP=mission.session.health.Current;target.AutoFire=true;yield return new WaitForSeconds(3);target.AutoFire=false;Check(mission.session.health.Current<playerHP,"El enemigo dispara y causa daño");Check(target.visual.sprite&&target.visual.enabled,"El nuevo robot sigue visible");
        var bulletCount=FindObjectsByType<CombatBullet>(FindObjectsSortMode.None).Length;CombatBullet.ClearAll();mission.session.health.ResetHealth();Place(player,174);yield return new WaitForSeconds(.5f);mission.boss.GetComponent<Collider2D>().enabled=true;mission.boss.health.Hit(10000,CombatTeam.Player);yield return new WaitForSecondsRealtime(1.4f);Check(mission.Complete,"Derrotar al jefe conserva la victoria de misión");
        File.AppendAllText(Out+"validacion.txt",failed?"RESULTADO: ERROR":"RESULTADO: CORRECTO");Application.Quit(failed?1:0);
    }
    void Capture(string name){Canvas.ForceUpdateCanvases();var rt=new RenderTexture(1280,720,24);rt.Create();RenderPipeline.SubmitRenderRequest(Camera.main,new RenderPipeline.StandardRequest{destination=rt});var prev=RenderTexture.active;RenderTexture.active=rt;var tex=new Texture2D(1280,720,TextureFormat.RGB24,false);tex.ReadPixels(new Rect(0,0,1280,720),0,0);tex.Apply();File.WriteAllBytes(Out+name,tex.EncodeToPNG());RenderTexture.active=prev;rt.Release();Destroy(rt);Destroy(tex);}
}
}
