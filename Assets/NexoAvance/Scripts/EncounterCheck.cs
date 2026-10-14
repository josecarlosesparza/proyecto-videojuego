using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;
namespace NexoAvance {
public class EncounterCheck:MonoBehaviour {
 const string Out="C:/Users/jose/Documents/Codex/2026-09-22/ho/outputs/Encuentros/";
 bool failed;float began;int checks;EncounterDirector d;ResistancePlayer p;
 [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Boot(){if(Array.IndexOf(Environment.GetCommandLineArgs(),"-encounterCheck")<0)return;var go=new GameObject("Validación encuentros");DontDestroyOnLoad(go);go.AddComponent<EncounterCheck>();}
 void Check(bool ok,string s){checks++;failed|=!ok;File.AppendAllText(Out+"validacion.txt",(ok?"OK ":"ERROR ")+s+"\n");}
 void Update(){if(Time.realtimeSinceStartup-began>480){File.AppendAllText(Out+"validacion.txt","ERROR tiempo agotado\n");Application.Quit(2);}}
 void Place(Vector2 pos){p.transform.position=pos;p.GetComponent<Rigidbody2D>().position=pos;p.GetComponent<Rigidbody2D>().linearVelocity=Vector2.zero;Physics2D.SyncTransforms();}
 void Kill(){foreach(var e in d.Live.ToArray())if(e&&e.health.Alive)e.health.Hit(10000,CombatTeam.Player);}
 void Capture(string name){Canvas.ForceUpdateCanvases();var rt=new RenderTexture(1280,720,24);rt.Create();RenderPipeline.SubmitRenderRequest(Camera.main,new RenderPipeline.StandardRequest{destination=rt});var prev=RenderTexture.active;RenderTexture.active=rt;var tex=new Texture2D(1280,720,TextureFormat.RGB24,false);tex.ReadPixels(new Rect(0,0,1280,720),0,0);tex.Apply();File.WriteAllBytes(Out+name,tex.EncodeToPNG());RenderTexture.active=prev;rt.Release();Destroy(rt);Destroy(tex);}
 IEnumerator Start(){
  began=Time.realtimeSinceStartup;Application.runInBackground=true;Application.targetFrameRate=60;QualitySettings.vSyncCount=0;File.WriteAllText(Out+"validacion.txt","");
  string[] names={"CallesBajoControl","FabricaAutomatizada","NucleoNexo"};
  for(int m=0;m<3;m++){
   yield return SceneManager.LoadSceneAsync(names[m]);yield return null;
   d=FindAnyObjectByType<EncounterDirector>();p=d.session.player;p.TestMode=true;d.SuppressFire=true;d.session.health.Shielded=true;
   foreach(var b in FindObjectsByType<OctopusBoss>(FindObjectsInactive.Include))b.AutoFire=false;foreach(var b in FindObjectsByType<MachineBoss>(FindObjectsInactive.Include))b.AutoFire=false;
   string tag="M"+(m+1)+" ";Check(d.Routes.nodes.Count>100,tag+"navegación calculada sobre terreno");
   Check(FindObjectsByType<StreetEnemy>().Length==0,tag+"generadores antiguos inactivos");Check(FindObjectsByType<ResistancePlayer>().Length==1&&p.GetComponent<HeroAppearance>()!=null,tag+"protagonista único conservado");
   float firstX=0;int invalid=0,spawns=0;float last=-100;bool stagger=true;
   d.Spawned+=(enemy,zone)=>{spawns++;if(spawns==1)firstX=enemy.transform.position.x;var c=Camera.main;float edge=c.orthographicSize*c.aspect;if(Mathf.Abs(enemy.transform.position.x-c.transform.position.x)<edge+d.cameraMargin-.1f||Mathf.Abs(enemy.transform.position.x-p.transform.position.x)<d.safePlayerDistance-.1f||!d.Routes.Clear(enemy.transform.position)||d.Routes.Path(enemy.transform.position,p.transform.position).Count==0)invalid++;if(zone==d.encounters[0]&&Time.time-last<zone.interval-.05f)stagger=false;last=Time.time;};
   Place(new Vector2(4.2f,-.12f));yield return new WaitForSeconds(6);
   Check(d.encounters[0].spawned==d.encounters[0].count,tag+"primer grupo completo escalonado ("+d.encounters[0].spawned+")");Check(stagger,tag+"intervalo de refuerzos");
   var first=d.Live.FirstOrDefault(e=>e&&e.health.Alive);float before=Mathf.Abs(firstX-p.transform.position.x);
   yield return new WaitForSeconds(4);Check(first&&Mathf.Abs(first.transform.position.x-p.transform.position.x)<before-.4f,tag+"persecución se acerca al jugador");
   bool separated=true;foreach(var a in d.Live)foreach(var b in d.Live)if(a&&b&&a!=b&&a.health.Alive&&b.health.Alive&&Vector2.Distance(a.transform.position,b.transform.position)<.6f)separated=false;Check(separated,tag+"grupo mantiene separación al acercarse");
   yield return new WaitForEndOfFrame();Capture("mision-"+(m+1)+"-grupo.png");
   if(first){
    var mover=first.GetComponent<EnemyPursuit>();mover.enabled=false;Place(new Vector2(1,-.12f));first.transform.position=new Vector2(3,-.12f);first.GetComponent<Rigidbody2D>().position=first.transform.position;Physics2D.SyncTransforms();
    first.AutoFire=true;d.session.health.Shielded=false;int shots=CombatBullet.ShotsCreated;bool warning=false;float until=Time.time+3.2f;int hp=d.session.health.Current;
    Check(first.HasSight(),tag+"visión incluye la capa del jugador");
    while(Time.time<until){warning|=first.warning.enabled;yield return null;}
    Check(warning&&CombatBullet.ShotsCreated>shots&&d.session.health.Current<hp,tag+"aviso visible y disparo causa daño");
    d.session.health.Shielded=true;d.session.health.ResetHealth();CombatBullet.ClearAll();
    var wall=new GameObject("Pared temporal de prueba");wall.transform.position=new Vector2(2,0);wall.AddComponent<BoxCollider2D>().size=new Vector2(.2f,4);Physics2D.SyncTransforms();shots=CombatBullet.ShotsCreated;
    Check(!first.HasSight(),tag+"pared bloquea visión");yield return new WaitForSeconds(3);Check(CombatBullet.ShotsCreated==shots,tag+"no dispara a través de pared");Destroy(wall);first.AutoFire=false;mover.enabled=true;mover.Invalidate();
   }
   int previous=d.SpawnedTotal;Place(new Vector2(1,-.12f));yield return new WaitForSeconds(.5f);Place(new Vector2(4.2f,-.12f));yield return new WaitForSeconds(1);Check(d.SpawnedTotal==previous,tag+"retroceder no reactiva grupo");
   // Cap and queue: keep first squad alive and trigger the next encounter.
   int cap=d.maxActive;d.maxActive=3;Place(new Vector2(d.encounters[1].triggerX+.1f,2));yield return new WaitForSeconds(3);
   Check(d.encounters[1].remaining==d.encounters[1].count,tag+"límite conserva refuerzos pendientes");Kill();yield return new WaitForSeconds(3);Check(d.encounters[1].spawned>0&&d.ActiveCount<=3,tag+"cola reanuda al liberar espacio");d.maxActive=cap;
   // Open objective doors as normal destruction does, then sample all remaining zones.
   var campaign=d.session.GetComponent<CampaignMission>();if(campaign)foreach(var o in campaign.objectives)o.health.Hit(10000,CombatTeam.Player);
   for(int z=1;z<d.encounters.Length;z++){
    Kill();Place(new Vector2(d.encounters[z].triggerX+.1f,2));yield return new WaitForSeconds(10);
    Check(d.encounters[z].spawned==d.encounters[z].count,tag+"zona "+(z+1)+" entrega grupo entero ("+d.encounters[z].spawned+"/"+d.encounters[z].count+")");
    bool clear=true;foreach(var e in d.Live)if(e&&e.health.Alive&&!d.Routes.Clear(e.transform.position))clear=false;Check(clear,tag+"zona "+(z+1)+" cuerpos fuera de muros");
   }
   Check(invalid==0&&spawns>20,tag+"todas las apariciones seguras y fuera de cámara ("+spawns+", inválidas "+invalid+")");
   Check(d.Routes.Path(new Vector2(1,-.12f),new Vector2(m==0?170:94,-.12f)).Count>1,tag+"ruta principal conectada después de abrir objetivos");
   var probe=d.Live.FirstOrDefault(e=>e&&e.health.Alive);if(probe){
    d.enabled=false;foreach(var e in d.Live.ToArray())if(e&&e!=probe)e.health.Hit(10000,CombatTeam.Player);
    var start=d.Routes.nodes[d.Routes.Nearest(new Vector2(10,-.12f))].point;probe.transform.position=start;probe.GetComponent<Rigidbody2D>().position=start;probe.GetComponent<EnemyPursuit>().Invalidate();Place(new Vector2(30,2));
    yield return new WaitForSeconds(15);Check(Mathf.Abs(probe.transform.position.x-p.transform.position.x)<9,tag+"persecución larga supera obstáculos reales (x="+probe.transform.position.x.ToString("F1")+")");Check(d.Routes.Clear(probe.transform.position),tag+"persecución mantiene cuerpo fuera de sólidos");d.enabled=true;
   }
   int active=d.ActiveCount,pending=d.Pending,total=d.SpawnedTotal;foreach(var e in d.Live)if(e)e.AutoFire=true;int deathShots=CombatBullet.ShotsCreated;p.GetComponent<CombatHealth>().Shielded=false;p.GetComponent<CombatHealth>().invulnerability=0;p.GetComponent<CombatHealth>().Hit(10000,CombatTeam.Robot);yield return new WaitForSecondsRealtime(.7f);
   Check(d.Stopped&&d.SpawnedTotal==total&&d.Pending==pending&&CombatBullet.ShotsCreated==deathShots,tag+"derrota congela refuerzos y ataques");d.session.Restart();yield return null;Check(d.session.health.Alive&&d.SpawnedTotal==0&&d.ActiveCount==0,tag+"reintento limpia enemigos y restablece encuentros");
   if(m==0){d.session.GetComponent<StreetMission>().Replay();yield return null;Check(d.TriggeredCount==0&&!d.encounters[0].triggered,tag+"reinicio completo restaura zona inicial");}
  }
  File.AppendAllText(Out+"validacion.txt","RESULTADO: "+(failed?"ERROR":"CORRECTO")+" · "+checks+" comprobaciones\n");Application.Quit(failed?1:0);
 }
}
}
