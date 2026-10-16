using System;
using System.Collections.Generic;
using UnityEngine;
namespace NexoAvance {
[Serializable]public class AdvanceEncounter {
    public string label;public float triggerX;
    [Range(1,20)]public int count=4;
    [Min(.15f)]public float interval=.8f;
    [Min(0)]public float initialDelay=.5f;
    public bool allowRear;[NonSerialized]public bool support;
    public StreetEnemyKind[] composition;
    [NonSerialized]public bool triggered;[NonSerialized]public int remaining,spawned;[NonSerialized]public float next;
}
[DefaultExecutionOrder(-60)]
public class EncounterDirector:MonoBehaviour {
    public CombatSession session;public StreetEnemy[] templates;public AdvanceEncounter[] encounters;
    [Header("Presión y aparición")][Range(3,24)]public int maxActive=10;
    [Min(1)]public float cameraMargin=1.8f;
    [Min(3)]public float safePlayerDistance=8;
    public bool safeRearFallback=true;
    public float routeLeft=-1,routeRight=199;
    [Min(.5f)]public float normalSpeed=2.8f,heavySpeed=2.2f,droneSpeed=3.2f;
    [Min(1)]public float firingDistance=4.5f;
    public EncounterRoutes Routes{get;private set;}
    public readonly List<StreetEnemy> Live=new List<StreetEnemy>();
    public bool SuppressFire {get;set;}
    public bool Finished{get;private set;}
    public int maxHeavyActive=2,maxBossSupports=3;
    readonly List<AdvanceEncounter> supports=new List<AdvanceEncounter>();
    IEnumerable<AdvanceEncounter> AllZones(){foreach(var z in encounters)yield return z;foreach(var z in supports)yield return z;}
    public void QueueSupport(int count){if(Finished||supports.Exists(z=>z.remaining>0))return;supports.Add(new AdvanceEncounter{label="Apoyo del calamar",triggered=true,support=true,remaining=Mathf.Clamp(count,1,3),count=count,interval=1.2f,next=Time.time+1,allowRear=true,composition=new[]{StreetEnemyKind.Fusilero,StreetEnemyKind.Cazador}});}
    public void FinishCombat(){Finished=true;foreach(var z in AllZones())z.remaining=0;foreach(var e in Live)if(e){e.gameObject.SetActive(false);Destroy(e.gameObject);}Live.Clear();}

    public int SpawnedTotal{get;private set;}
    public int TriggeredCount{get;private set;}
    public int Pending {get{int n=0;foreach(var z in AllZones())if(z.triggered)n+=z.remaining;return n;}}
    public int ActiveCount {get{int n=0;foreach(var e in Live)if(e&&e.health.Alive)n++;return n;}}
    public bool HasOutstanding=>Pending+ActiveCount>0;
    public bool Stopped=>Finished||!session||!session.health.Alive||!session.player.enabled||Time.timeScale==0;
    public event Action<StreetEnemy,AdvanceEncounter> Spawned;
    float routeCheck;int serial;StreetMission street;CampaignMission campaign;
    void Awake(){street=session.GetComponent<StreetMission>();campaign=session.GetComponent<CampaignMission>();}
    void Start(){Routes=new EncounterRoutes();Routes.Build(routeLeft,routeRight);ResetAttempt(session.player.transform.position.x);}
    void Update(){
        if(Stopped)return;
        if(Time.time>=routeCheck){routeCheck=Time.time+1;if(Routes.Changed()){Routes.Build(routeLeft,routeRight);foreach(var e in Live)if(e)e.GetComponent<EnemyPursuit>().Invalidate();}}
        foreach(var zone in AllZones()){
            if(!zone.triggered&&session.player.transform.position.x>=zone.triggerX){zone.triggered=true;zone.remaining=zone.count;zone.next=Time.time+zone.initialDelay;TriggeredCount++;}
            if(!zone.triggered||zone.remaining<=0||Time.time<zone.next||ActiveCount>=maxActive)continue;
            if(TrySpawn(zone)){zone.remaining--;zone.spawned++;zone.next=Time.time+zone.interval;}else zone.next=Time.time+.4f;
        }
        Live.RemoveAll(e=>!e);
    }
    public void ResetAttempt(float checkpoint){
        Finished=false;supports.Clear();foreach(var e in Live)if(e){e.gameObject.SetActive(false);Destroy(e.gameObject);}Live.Clear();serial=0;SpawnedTotal=TriggeredCount=0;
        foreach(var z in encounters){z.triggered=z.triggerX<checkpoint-5;z.remaining=z.spawned=0;z.next=0;}
    }
    public bool SpawnPosition(AdvanceEncounter zone,out Vector2 position){
        position=default;var cam=Camera.main;if(!cam||Routes==null)return false;
        float left=cam.transform.position.x-cam.orthographicSize*cam.aspect-cameraMargin;
        float right=cam.transform.position.x+cam.orthographicSize*cam.aspect+cameraMargin;
        bool preferRear=zone.allowRear&&zone.spawned%3==2;
        // If the front is blocked by a mission door or the end of the avenue,
        // retain a reachable rear entry rather than stranding the queue forever.
        for(int side=0;side<(zone.allowRear||safeRearFallback?2:1);side++){
            bool rear=side==0?preferRear:!preferRear;float desired=rear?left:right;
            int best=-1;float distance=float.MaxValue;
            for(int i=0;i<Routes.nodes.Count;i++){
                Vector2 p=Routes.nodes[i].point;
                if(rear?p.x>left:p.x<right)continue;
                float delta=Mathf.Abs(p.x-desired);if(delta>12||delta>=distance||Mathf.Abs(p.x-session.player.transform.position.x)<safePlayerDistance)continue;
                bool occupied=false;foreach(var e in Live)if(e&&e.health.Alive&&Vector2.Distance(e.transform.position,p)<1.3f){occupied=true;break;}
                if(occupied||!Routes.Clear(p)||Routes.Path(p,session.player.transform.position).Count==0)continue;
                best=i;distance=delta;
            }
            if(best>=0){position=Routes.nodes[best].point;return true;}
        }return false;
    }
    bool TrySpawn(AdvanceEncounter zone){
        if(zone.composition==null||zone.composition.Length==0||templates==null)return false;
        if(!SpawnPosition(zone,out var point))return false;
        var kind=zone.composition[zone.spawned%zone.composition.Length];StreetEnemy template=Array.Find(templates,t=>t&&t.kind==kind);if(!template)return false;
        var profile=template.GetComponent<RobotArsenal>()?.profile;int heavies=0,helpers=0;foreach(var live in Live)if(live&&live.health.Alive){var a=live.GetComponent<RobotArsenal>();if(a&&a.profile.heavy)heavies++;if(a&&a.bossSupport)helpers++;}if(profile&&profile.heavy&&heavies>=maxHeavyActive||zone.support&&helpers>=maxBossSupports)return false;
        if(profile&&profile.flying){var hover=point+Vector2.up*profile.hoverHeight;if(Routes.Clear(hover))point=hover;else return false;}
        var e=Instantiate(template,point,Quaternion.identity,transform);e.name=kind+" · refuerzo "+(++serial);e.target=session.health;e.origin=point;e.AutoFire=!SuppressFire;
        var mover=e.GetComponent<EnemyPursuit>();mover.director=this;mover.serial=serial;mover.moveSpeed=kind==StreetEnemyKind.Dron?droneSpeed:kind==StreetEnemyKind.Fusilero?normalSpeed:heavySpeed;mover.attackDistance=firingDistance;var arsenal=e.GetComponent<RobotArsenal>();if(arsenal&&arsenal.profile){mover.moveSpeed=arsenal.profile.speed;mover.attackDistance=arsenal.profile.stopDistance;arsenal.bossSupport=zone.support;}
        Live.Add(e);e.health.Died+=()=>{if(street)street.RecordReinforcementKill();if(campaign)campaign.RecordReinforcementKill();Destroy(e.gameObject,1.2f);};
        e.gameObject.SetActive(true);
        var collider=e.GetComponent<Collider2D>();Physics2D.IgnoreCollision(collider,session.player.GetComponent<Collider2D>());
        foreach(var other in Live)if(other&&other!=e)Physics2D.IgnoreCollision(collider,other.GetComponent<Collider2D>());
        SpawnedTotal++;Spawned?.Invoke(e,zone);return true;
    }
}
}
