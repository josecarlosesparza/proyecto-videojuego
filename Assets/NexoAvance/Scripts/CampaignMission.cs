using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
namespace NexoAvance {
public class CampaignMission:MonoBehaviour {
    public int number;public CombatSession session;public CampaignObjective[] objectives;public MachineBoss boss;
    public GameObject[] groups;public float[] triggers;public GameObject exitDoor,bossHud,victoryPanel;
    public float bossTrigger,exitX;public Text toast,progress,bossName,victoryTime;public Image bossFill;public Button replay,menu,next;
    public bool Complete {get;private set;}public bool BossStarted {get;private set;}
    public int DestroyedCount {get{int n=0;foreach(var o in objectives)if(!o.health.Alive)n++;return n;}}
    float elapsed,noticeUntil,deadAt,checkpoint=1;int kills;bool loading;
    void Start(){GrenadeInventory.Install(session,Notice,false);CampaignCarry.Apply(session);replay.onClick.AddListener(Replay);menu.onClick.AddListener(session.BackToMenu);next.onClick.AddListener(Continue);victoryPanel.SetActive(false);bossHud.SetActive(false);foreach(var group in groups)foreach(var enemy in group.GetComponentsInChildren<StreetEnemy>(true))enemy.health.Died+=EnemyDown;Notice(number==2?"FÁBRICA AUTOMATIZADA":"EL NÚCLEO DE NEXO",3);}
    void EnemyDown(){kills++;if(kills==2 && !session.player.GetComponent<PlayerCombat>().HasMachineGun){session.player.GetComponent<PlayerCombat>().SetMachineGun(true);Notice("METRALLETA OBTENIDA",2);}}
    public void Notice(string text,float duration){toast.text=text;noticeUntil=Time.time+duration;toast.gameObject.SetActive(true);}
    void Update(){if(Complete || Time.timeScale==0)return;elapsed+=Time.deltaTime;toast.gameObject.SetActive(Time.time<noticeUntil);float x=session.player.transform.position.x;
        for(int i=0;i<groups.Length;i++)if(x>=triggers[i])groups[i].SetActive(true);
        if(x>=40 && checkpoint<40){checkpoint=40;session.player.SetSpawn(new Vector3(40,-.15f,0));}if(x>=80 && checkpoint<80){checkpoint=80;session.player.SetSpawn(new Vector3(number==2?79:80,-.15f,0));}
        progress.text=(number==2?"GENERADORES ":"TERMINALES ")+DestroyedCount+" / "+objectives.Length;
        if(!BossStarted && x>=bossTrigger){BossStarted=true;boss.gameObject.SetActive(true);bossHud.SetActive(true);Notice(number==2?"SUPERVISOR DE ENSAMBLAJE":"NEXO · NÚCLEO CENTRAL",2);}
        if(BossStarted){bossFill.fillAmount=boss.health.Current/(float)boss.health.maximum;bossName.text=number==2?"SUPERVISOR":"NEXO · FASE "+boss.Phase+(boss.health.Shielded?" · ESCUDO":"");}
        bool disabled=DestroyedCount==objectives.Length;
        if(disabled && BossStarted && !boss.health.Alive){exitDoor.SetActive(false);if(deadAt==0){deadAt=Time.time+.9f;CombatBullet.ClearAll();}if(Time.time>=deadAt && (number==3 || x>=exitX))Win();}
        else if(x>=exitX-2 && number==2)Notice("DESTRUYE LOS DOS GENERADORES Y AL SUPERVISOR",1);
    }
    public void ResetCheckpoint(){session.player.GetComponent<GrenadeInventory>()?.ClearThrows();Complete=false;victoryPanel.SetActive(false);boss.gameObject.SetActive(false);boss.ResetBoss();BossStarted=false;bossHud.SetActive(false);deadAt=0;exitDoor.SetActive(true);Notice("PUNTO DE CONTROL",1.5f);}
    void Win(){Complete=true;CombatBullet.ClearAll();session.player.GetComponent<GrenadeInventory>().ClearThrows();session.player.SetPaused(false);session.player.GetComponent<Rigidbody2D>().linearVelocity=Vector2.zero;session.player.enabled=false;Time.timeScale=0;bossHud.SetActive(false);victoryPanel.SetActive(true);victoryTime.text=(number==2?"Producción detenida · ":"La resistencia ha desconectado NEXO · ")+TimeSpan.FromSeconds(elapsed).ToString(@"mm\:ss");EventSystem.current?.SetSelectedGameObject(number==2?next.gameObject:menu.gameObject);}
    public void Continue(){if(number!=2 || !Complete || loading)return;loading=true;CampaignCarry.Save(session);Time.timeScale=1;SceneManager.LoadSceneAsync("NucleoNexo");}
    public void Replay(){if(loading)return;loading=true;CampaignCarry.Ready=false;Time.timeScale=1;SceneManager.LoadSceneAsync(number==2?"FabricaAutomatizada":"NucleoNexo");}
}
}
