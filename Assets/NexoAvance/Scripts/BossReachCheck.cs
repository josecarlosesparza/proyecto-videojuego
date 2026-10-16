using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace NexoAvance {
public class BossReachCheck:MonoBehaviour {
 const string Out="C:/Users/jose/Documents/Codex/2026-09-22/ho/outputs/Robots/alcance-jefe.txt";
 bool failed;float start;int count;ResistancePlayer player;
 [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Boot(){if(Array.IndexOf(Environment.GetCommandLineArgs(),"-bossReachCheck")<0)return;var go=new GameObject("Prueba de alcance real del núcleo");DontDestroyOnLoad(go);go.AddComponent<BossReachCheck>();}
 void Check(bool ok,string text){count++;failed|=!ok;File.AppendAllText(Out,(ok?"OK ":"ERROR ")+text+"\n");}
 void Update(){if(Time.realtimeSinceStartup-start>90){File.AppendAllText(Out,"ERROR tiempo agotado\n");Application.Quit(2);}}
 void Place(float x){var p=new Vector2(x,-.12f);player.transform.position=p;player.GetComponent<Rigidbody2D>().position=p;player.GetComponent<Rigidbody2D>().linearVelocity=Vector2.zero;Physics2D.SyncTransforms();}
 IEnumerator Start(){start=Time.realtimeSinceStartup;Application.runInBackground=true;Application.targetFrameRate=60;QualitySettings.vSyncCount=0;File.WriteAllText(Out,"");yield return SceneManager.LoadSceneAsync("CallesBajoControl");yield return null;
  var d=FindAnyObjectByType<EncounterDirector>();d.enabled=false;player=d.session.player;player.TestMode=true;player.SetPaused(false);var hp=d.session.health;hp.Shielded=true;var boss=d.session.GetComponent<StreetMission>().boss;boss.AutoFire=false;Place(178);yield return new WaitForSeconds(.5f);boss.health.Hit(900,CombatTeam.Player);yield return null;var combat=player.GetComponent<PlayerCombat>();combat.SetMachineGun(false);
  for(int hero=0;hero<4;hero++){
   HeroSelection.Choose(hero);player.GetComponent<HeroAppearance>().Initialize(player);Place(178);yield return new WaitForSeconds(.4f);int before=boss.health.Current;player.TestJump=true;yield return new WaitForSeconds(.25f);while(player.Velocity.y>1)yield return new WaitForFixedUpdate();player.visual.flipX=false;bool fired=combat.TryShoot();float height=combat.LastShotOrigin.y;yield return new WaitForSeconds(.65f);
   Check(fired&&boss.health.Current==before-combat.damage*2,"Personaje "+hero+" alcanza núcleo saltando con bala normal, altura "+height.ToString("F2"));
  }
  HostileHazard.ClearAll();CombatBullet.ClearAll();hp.Shielded=false;hp.invulnerability=0;hp.ResetHealth();Place(174);yield return new WaitForSeconds(.3f);
  HostileHazard.Sweep(player.transform.position,boss.sweepDamage,boss.sweepWidth,.4f,boss);yield return new WaitForSeconds(.6f);Check(hp.Current==80,"Barrido bajo aplica 20 una sola vez al jugador en suelo");
  hp.ResetHealth();HostileHazard.Sweep(player.transform.position,boss.sweepDamage,boss.sweepWidth,.5f,boss);yield return new WaitForSeconds(.2f);player.TestJump=true;yield return new WaitForSeconds(.5f);Check(hp.Current==100,"Salto real esquiva el barrido");yield return new WaitForSeconds(.5f);
  hp.ResetHealth();Place(174);HostileHazard.Missile(player.transform.position,boss.missileDamage,boss.missileRadius,.5f,boss);yield return new WaitForSeconds(1.2f);Check(hp.Current==75,"Misil anunciado aplica 25 una sola vez");
  Check(boss.core.GetComponent<CircleCollider2D>().radius==boss.core.radius,"Zona de colisión coincide con el área vulnerable marcada");
  File.AppendAllText(Out,"RESULTADO: "+(failed?"ERROR":"CORRECTO")+" · "+count+" comprobaciones\n");Application.Quit(failed?1:0);
 }
}
}
