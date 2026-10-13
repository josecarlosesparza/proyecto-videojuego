using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace NexoAvance {
public class ContinuousMissionCheck:MonoBehaviour {
    const string Output=@"C:\Users\jose\Documents\Codex\2026-09-22\ho\outputs\Mision01\validacion-continuo.txt";
    bool failed;
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Boot(){if(Array.IndexOf(Environment.GetCommandLineArgs(),"-continuousCheck")<0)return;var go=new GameObject("Validación recorrido continuo");DontDestroyOnLoad(go);go.AddComponent<ContinuousMissionCheck>();}
    void Check(bool ok,string message){failed|=!ok;File.AppendAllText(Output,(ok?"OK ":"ERROR ")+message+"\n");}
    void Place(ResistancePlayer p,float x){var b=p.GetComponent<Rigidbody2D>();b.position=new Vector2(x,-.15f);p.transform.position=b.position;b.linearVelocity=Vector2.zero;Physics2D.SyncTransforms();}
    IEnumerator Start(){
        File.WriteAllText(Output,"");Application.runInBackground=true;yield return SceneManager.LoadSceneAsync("CallesBajoControl");yield return null;
        var m=FindAnyObjectByType<StreetMission>();var p=m.session.player;var gun=p.GetComponent<PlayerCombat>();p.TestMode=true;p.SetPaused(false);m.boss.AutoFire=false;
        foreach(var e in FindObjectsByType<StreetEnemy>(FindObjectsInactive.Include,FindObjectsSortMode.None))e.AutoFire=false;
        bool gates=!m.bossEntrance.activeSelf;foreach(var s in m.sectors)gates&=!s.gate.activeSelf;Check(gates,"Todos los muros de control están desactivados");
        Place(p,11);yield return new WaitForSeconds(.2f);var enemies=m.sectors[0].waves[0].GetComponentsInChildren<StreetEnemy>();
        enemies[0].health.Hit(1000,CombatTeam.Player);Check(m.Eliminations==1 && !gun.HasMachineGun,"La primera baja conserva el arma inicial");
        enemies[1].health.Hit(1000,CombatTeam.Player);Check(m.Eliminations==2 && gun.HasMachineGun,"La segunda baja entrega la metralleta");
        Place(p,31.5f);p.visual.flipX=false;yield return new WaitForSeconds(.15f);int hp=enemies[2].health.Current,shots=CombatBullet.ShotsCreated;gun.TryShoot();yield return new WaitForSeconds(.4f);
        Check(CombatBullet.ShotsCreated-shots==3,"Una pulsación genera una ráfaga de tres balas");Check(enemies[2].health.Current==hp-54,"La ráfaga inflige 54 puntos de daño");
        yield return new WaitForSeconds(.2f);shots=CombatBullet.ShotsCreated;gun.TryShoot();p.SetPaused(true);yield return new WaitForSecondsRealtime(.25f);Check(CombatBullet.ShotsCreated==shots+1,"La pausa detiene las balas pendientes de la ráfaga");p.SetPaused(false);yield return new WaitForSeconds(.25f);
        Check(CombatBullet.ShotsCreated==shots+3,"La ráfaga continúa al quitar la pausa");CombatBullet.ClearAll();
        Place(p,79);yield return new WaitForSeconds(.2f);Check(m.Sector==2,"Se avanza y se guardan puntos de control sin eliminar a todos");
        m.session.Restart();yield return new WaitForSeconds(.2f);Check(!enemies[0].health.Alive && !enemies[1].health.Alive && gun.HasMachineGun,"Reintentar conserva las bajas y la metralleta");
        yield return new WaitForSeconds(2);bool extras=false;foreach(var s in m.sectors)for(int i=1;i<s.waves.Length;i++)extras|=s.waves[i].activeSelf;Check(!extras,"No se activan segundas oleadas");
        Place(p,174);yield return new WaitForSeconds(.2f);Check(m.BossStarted && !m.bossEntrance.activeSelf,"Se llega al jefe sin muros ni obligación de eliminar patrullas");
        m.boss.health.Hit(10000,CombatTeam.Player);yield return new WaitForSecondsRealtime(1.3f);Check(m.Complete,"Derrotar al jefe conserva la victoria");
        m.Replay();yield return new WaitForSeconds(.2f);Check(m.Eliminations==0 && !gun.HasMachineGun && m.Sector==0,"Repetir toda la misión reinicia la mejora y el contador");
        File.AppendAllText(Output,failed?"RESULTADO: ERROR":"RESULTADO: CORRECTO");Application.Quit(failed?1:0);
    }
}
}
