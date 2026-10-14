using System.Collections.Generic;
using UnityEngine;
namespace NexoAvance {
// Navigation samples the real solid surfaces; every link is clearance-tested with a body-sized box.
public class EncounterRoutes {
    public class Node {public Vector2 point;public List<Link> links=new List<Link>();}
    public struct Link {public int to;public float lift;public Link(int t,float l){to=t;lift=l;}}
    public readonly List<Node> nodes=new List<Node>();
    readonly List<Collider2D> solids=new List<Collider2D>();
    public readonly Vector2 bodySize=new Vector2(1f,1.7f);
    int signature;
    public static bool Solid(Collider2D c)=>c&&c.enabled&&c.gameObject.activeInHierarchy&&!c.isTrigger&&!c.GetComponentInParent<StreetEnemy>()&&!c.GetComponentInParent<ResistancePlayer>()&&!c.GetComponentInParent<ThrownGrenade>()&&!c.GetComponentInParent<OctopusBoss>()&&!c.GetComponentInParent<MachineBoss>();
    public bool Changed(){int s=0;foreach(var c in solids)if(Solid(c))s^=c.GetHashCode();return s!=signature;}
    public void Build(float left,float right){
        solids.Clear();signature=0;foreach(var c in Object.FindObjectsByType<Collider2D>(FindObjectsInactive.Include)){if(c.isTrigger||c.GetComponentInParent<StreetEnemy>()||c.GetComponentInParent<ResistancePlayer>()||c.GetComponentInParent<ThrownGrenade>()||c.GetComponentInParent<OctopusBoss>()||c.GetComponentInParent<MachineBoss>())continue;solids.Add(c);if(Solid(c))signature^=c.GetHashCode();}
        nodes.Clear();Physics2D.SyncTransforms();
        for(float x=left;x<=right;x+=.5f){
            foreach(var hit in Physics2D.RaycastAll(new Vector2(x,7),Vector2.down,12)){
                if(!Solid(hit.collider)||hit.normal.y<.8f||hit.point.y>3||hit.point.y< -3)continue;
                Vector2 p=hit.point+Vector2.up*.88f;
                if(!Clear(p))continue;
                bool duplicate=false;foreach(var n in nodes)if(Vector2.Distance(n.point,p)<.15f){duplicate=true;break;}if(!duplicate)nodes.Add(new Node{point=p});
            }
        }
        for(int i=0;i<nodes.Count;i++)for(int j=i+1;j<nodes.Count;j++){
            var a=nodes[i].point;var b=nodes[j].point;float dx=Mathf.Abs(b.x-a.x),dy=b.y-a.y;
            if(dx>3.5f||dx<.1f||Mathf.Abs(dy)>1.8f)continue;
            float lift=0;
            bool walk=Mathf.Abs(dy)<.15f&&SegmentClear(a,b);
            if(walk)for(int sample=1;sample<8;sample++){var p=Vector2.Lerp(a,b,sample/8f);bool support=false;foreach(var hit in Physics2D.RaycastAll(p,Vector2.down,.95f))if(Solid(hit.collider)&&hit.normal.y>.8f)support=true;if(!support){walk=false;break;}}
            if(!walk){if(dx<.8f)continue;lift=1.3f;}
            if(!ArcClear(a,b,lift))continue;
            nodes[i].links.Add(new Link(j,lift));nodes[j].links.Add(new Link(i,lift));
        }
    }
    public bool Clear(Vector2 p){foreach(var c in Physics2D.OverlapBoxAll(p,bodySize-new Vector2(.04f,.04f),0))if(Solid(c))return false;return true;}
    public bool SegmentClear(Vector2 a,Vector2 b){var d=b-a;foreach(var hit in Physics2D.BoxCastAll(a,bodySize-new Vector2(.04f,.04f),0,d.normalized,d.magnitude))if(Solid(hit.collider))return false;return true;}
    public static Vector2 Arc(Vector2 a,Vector2 b,float lift,float t)=>Vector2.Lerp(a,b,t)+Vector2.up*(Mathf.Sin(t*Mathf.PI)*lift);
    bool ArcClear(Vector2 a,Vector2 b,float lift){var prev=a;for(int k=1;k<=16;k++){var p=Arc(a,b,lift,k/16f);if(!SegmentClear(prev,p))return false;prev=p;}return true;}
    public int Nearest(Vector2 p){int best=-1;float dist=float.MaxValue;for(int i=0;i<nodes.Count;i++){float d=(nodes[i].point-p).sqrMagnitude;if(d<dist){best=i;dist=d;}}return best;}
    public List<int> Path(Vector2 start,Vector2 end,System.Predicate<Vector2> occupied=null){
        int from=Nearest(start),to=Nearest(end);var result=new List<int>();if(from<0||to<0)return result;
        int[] parent=new int[nodes.Count];for(int i=0;i<parent.Length;i++)parent[i]=-2;parent[from]=-1;
        var queue=new Queue<int>();queue.Enqueue(from);
        while(queue.Count>0){int at=queue.Dequeue();if(at==to)break;foreach(var edge in nodes[at].links)if(parent[edge.to]==-2&&(occupied==null||!occupied(nodes[edge.to].point))){parent[edge.to]=at;queue.Enqueue(edge.to);}}
        if(parent[to]==-2)return result;for(int n=to;n>=0;n=parent[n])result.Add(n);result.Reverse();return result;
    }
    public float Lift(int from,int to){foreach(var link in nodes[from].links)if(link.to==to)return link.lift;return 0;}
}
}
