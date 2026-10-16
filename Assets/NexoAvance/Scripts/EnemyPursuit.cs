using System.Collections.Generic;
using UnityEngine;
namespace NexoAvance {
[DisallowMultipleComponent]
public class EnemyPursuit:MonoBehaviour {
    [Min(.5f)] public float moveSpeed=2.8f;
    [Min(1)] public float attackDistance=4.5f;
    [Min(.2f)] public float resumeMargin=1.2f;
    [Range(.6f,1.5f)] public float separation=.95f;
    public bool melee;
    public EncounterDirector director;
    public int serial;
    StreetEnemy enemy;Rigidbody2D body;List<int> path=new List<int>();int cursor;
    Vector2 segmentStart,segmentEnd;float lift,progress,duration,repathAt;bool moving,holding;
    public bool Moving=>moving;
    float Hover{get{var a=GetComponent<RobotArsenal>();return a&&a.profile&&a.profile.flying?a.profile.hoverHeight:0;}}
    void Awake(){enemy=GetComponent<StreetEnemy>();body=GetComponent<Rigidbody2D>();}
    void OnEnable(){moving=false;holding=false;repathAt=0;}
    public void Invalidate(){moving=false;path.Clear();repathAt=0;}
    void FixedUpdate(){
        if(!director||director.Stopped||!enemy.health.Alive||!enemy.target.Alive||Time.timeScale==0)return;
        float dx=enemy.target.transform.position.x-transform.position.x;
        float range=melee?1.25f:attackDistance+(serial%4)*.55f;
        var arsenal=GetComponent<RobotArsenal>();if(arsenal&&arsenal.profile)range=Mathf.Min(range,arsenal.profile.range-.7f);
        bool sight=enemy.HasSight();
        if(holding){float resume=range+resumeMargin;if(arsenal&&arsenal.profile)resume=Mathf.Min(resume,arsenal.profile.range-.2f);if(Mathf.Abs(dx)>resume||!sight)holding=false;}
        else if(Mathf.Abs(dx)<range&&sight&&(!moving||lift==0)){holding=true;moving=false;path.Clear();}
        if(holding)return;
        if(!moving && Time.time>=repathAt){
            path=director.Routes.Path((Vector2)transform.position-Vector2.up*Hover,enemy.target.transform.position,point=>Occupied(point+Vector2.up*Hover));cursor=0;repathAt=Time.time+.7f;
            if(path.Count>1){cursor=1;Begin(path[0],path[1]);}
        }
        if(!moving)return;
        // Yield to a nearer ally, with a stable serial tie-break. No enemy-body pushing/piling.
        if(progress<.02f||lift==0){foreach(var other in director.Live){if(!other||other==enemy||!other.health.Alive)continue;var d=other.transform.position-transform.position;var otherMove=other.GetComponent<EnemyPursuit>();if(Mathf.Abs(d.y)<.9f&&Mathf.Abs(d.x)<separation&&Mathf.Sign(d.x)==Mathf.Sign(segmentEnd.x-segmentStart.x)&&(Mathf.Abs(other.transform.position.x-enemy.target.transform.position.x)<Mathf.Abs(dx)||otherMove.serial<serial))return;}}
        float next=Mathf.Min(1,progress+Time.fixedDeltaTime/duration);var dest=EncounterRoutes.Arc(segmentStart,segmentEnd,lift,next);
        if(!director.Routes.SegmentClear(body.position,dest)){moving=false;repathAt=Time.time+.25f;return;}
        body.MovePosition(dest);progress=next;
        if(progress>=1){moving=false;cursor++;if(cursor<path.Count && Time.time<repathAt && Mathf.Abs(dx)>range && path[cursor]<director.Routes.nodes.Count)Begin(path[cursor-1],path[cursor]);else repathAt=0;}
    }
    bool Occupied(Vector2 point){foreach(var other in director.Live){if(!other||other==enemy||!other.health.Alive)continue;var agent=other.GetComponent<EnemyPursuit>();Vector2 pos=other.transform.position;if(Mathf.Abs(pos.y-point.y)<.8f&&Mathf.Abs(pos.x-point.x)<separation)return true;if(agent.moving&&Mathf.Abs(agent.segmentEnd.y-point.y)<.8f&&Mathf.Abs(agent.segmentEnd.x-point.x)<separation)return true;}return false;}
    void Begin(int from,int to){var end=director.Routes.nodes[to].point+Vector2.up*Hover;if(Occupied(end)){moving=false;repathAt=Time.time+.35f;return;}segmentStart=body.position;segmentEnd=end;lift=director.Routes.Lift(from,to);progress=0;duration=Mathf.Max(.17f,Mathf.Abs(segmentEnd.x-segmentStart.x)/moveSpeed);if(lift>0)duration=Mathf.Max(.65f,duration);moving=true;}
}
}
