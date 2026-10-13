using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
namespace NexoAvance {
[Serializable] public class StreetSector {
    public float trigger,checkpoint;
    public GameObject gate;
    public GameObject[] waves;
}
public class StreetMission : MonoBehaviour {
    public CombatSession session;
    public StreetSector[] sectors;
    public OctopusBoss boss;
    public GameObject bossEntrance,bossHud,victoryPanel;
    public Image bossFill;
    public Text toast,victoryTime;
    public Button replay,menu;
    public float cameraEnd=191,bossTrigger=163;
    public int Sector {get;private set;}
    public int Wave => 0;
    public int Eliminations {get;private set;}
    public bool Complete {get;private set;}
    public bool BossStarted {get;private set;}
    public float Elapsed {get;private set;}
    float toastUntil,victoryAt;
    readonly List<CombatHealth> enemies=new List<CombatHealth>();
    PlayerCombat weapon;
    public float Checkpoint => Sector==0?1:sectors[Sector-1].checkpoint;
    void Awake(){
        weapon=session.player.GetComponent<PlayerCombat>();
        foreach(var sector in sectors){
            if(sector.gate)sector.gate.SetActive(false);
            for(int i=1;i<sector.waves.Length;i++)sector.waves[i].SetActive(false);
            foreach(var health in sector.waves[0].GetComponentsInChildren<CombatHealth>(true)){enemies.Add(health);health.Died+=EnemyDown;}
        }
        bossEntrance.SetActive(false);
    }
    public void ShowNotice(string text,float seconds){Notify(text,seconds);}
    void Start(){GrenadeInventory.Install(this);replay.onClick.AddListener(Replay);menu.onClick.AddListener(session.BackToMenu);victoryPanel.SetActive(false);bossHud.SetActive(false);Notify("CALLES BAJO CONTROL",3);}
    void EnemyDown(){Eliminations++;if(Eliminations==2){weapon.SetMachineGun(true);Notify("METRALLETA OBTENIDA · J PARA DISPARAR RÁFAGAS",3);}}
    void Update(){
        if(Time.timeScale==0 || Complete)return;
        Elapsed+=Time.deltaTime;toast.gameObject.SetActive(Time.time<toastUntil);
        float x=session.player.transform.position.x;
        // Cada patrulla aparece una sola vez. El avance no depende de eliminarla.
        foreach(var sector in sectors)if(x>=sector.trigger && !sector.waves[0].activeSelf)sector.waves[0].SetActive(true);
        while(Sector<sectors.Length && x>=sectors[Sector].checkpoint){Sector++;session.health.ResetHealth();session.player.SetSpawn(new Vector3(Checkpoint,-.15f,0));}
        if(!BossStarted && x>=bossTrigger){BossStarted=true;boss.gameObject.SetActive(true);bossHud.SetActive(true);Notify("KRAKEN-01",2);}
        if(BossStarted){bossFill.fillAmount=boss.health.Current/(float)boss.health.maximum;if(!boss.health.Alive){if(victoryAt==0){CombatBullet.ClearAll();victoryAt=Time.time+.85f;}if(Time.time>=victoryAt)Win();}}
    }
    public bool Cleared(GameObject wave){foreach(var h in wave.GetComponentsInChildren<CombatHealth>(true))if(h.Alive)return false;return true;}
    void Notify(string s,float duration){toast.text=s;toastUntil=Time.time+duration;toast.gameObject.SetActive(true);}
    public void ResetCheckpoint(){
        session.player.GetComponent<GrenadeInventory>()?.ClearThrows();
        victoryAt=0;Complete=false;victoryPanel.SetActive(false);bossHud.SetActive(false);BossStarted=false;bossEntrance.SetActive(false);boss.gameObject.SetActive(false);boss.ResetBoss();
        // Los enemigos eliminados y la mejora se conservan al reintentar.
        foreach(var e in FindObjectsByType<EnemyExplosion>(FindObjectsSortMode.None))Destroy(e.gameObject);
        Notify("PUNTO DE CONTROL",1.5f);
    }
    void Win(){session.player.GetComponent<GrenadeInventory>()?.ClearThrows();Complete=true;CombatBullet.ClearAll();bossHud.SetActive(false);session.player.SetPaused(false);session.player.GetComponent<Rigidbody2D>().linearVelocity=Vector2.zero;session.player.enabled=false;Time.timeScale=0;victoryPanel.SetActive(true);victoryTime.text="Avenida liberada · "+TimeSpan.FromSeconds(Elapsed).ToString(@"mm\:ss");EventSystem.current?.SetSelectedGameObject(replay.gameObject);}
    public void Replay(){
        session.player.GetComponent<GrenadeInventory>()?.ResetRun();
        Sector=0;Elapsed=0;Eliminations=0;weapon.SetMachineGun(false);
        foreach(var sector in sectors){foreach(var enemy in sector.waves[0].GetComponentsInChildren<StreetEnemy>(true))enemy.ResetEnemy();sector.waves[0].SetActive(false);}
        session.player.SetSpawn(new Vector3(1,.2f,0));session.Restart();
    }
    void OnDestroy(){foreach(var health in enemies)if(health)health.Died-=EnemyDown;}
}
}
