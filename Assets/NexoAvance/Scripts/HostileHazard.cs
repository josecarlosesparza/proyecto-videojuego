using System.Collections.Generic;
using UnityEngine;
namespace NexoAvance {
// One damage window per explosion/sweep; pooled, with a visible warning before activation.
public class HostileHazard:MonoBehaviour {
 static readonly Queue<HostileHazard> pool=new Queue<HostileHazard>();
 public static int Detonations,DamageApplications;
 public Vector2 Destination{get;private set;}
 Vector2 start;float started,flight,fuse,radius;int damage;bool sweep,missile,landed,strike;
 CombatHealth target;Sprite pixel;Material material;SpriteRenderer projectile;LineRenderer marker;
 public static HostileHazard Lob(Vector2 from,Vector2 to,int damage,float radius,float flight,float fuse,StreetEnemy owner){return Create(from,Ground(to),damage,radius,flight,fuse,owner.target,owner.pixel,owner.material,false,false);}
 public static HostileHazard Missile(Vector2 to,int damage,float radius,float warning,OctopusBoss owner){to=Ground(to);return Create(to+Vector2.up*8,to,damage,radius,.5f,warning,owner.target,owner.pixel,owner.material,false,true);}
 public static HostileHazard Sweep(Vector2 to,int damage,float width,float warning,OctopusBoss owner){to=Ground(to)+Vector2.up*.3f;return Create(to,to,damage,width,0,warning,owner.target,owner.pixel,owner.material,true,false);}
 public static HostileHazard GroundStrike(Vector2 to,int damage,float width,float warning,OctopusBoss owner){var h=Sweep(to,damage,width,warning,owner);h.strike=true;return h;}
 static Vector2 Ground(Vector2 p){foreach(var hit in Physics2D.RaycastAll(new Vector2(p.x,6),Vector2.down,12,~0))if(EncounterRoutes.Solid(hit.collider)&&hit.normal.y>.5f)return hit.point+Vector2.up*.12f;return new Vector2(p.x,-.88f);}
 static HostileHazard Create(Vector2 from,Vector2 to,int dmg,float size,float travel,float delay,CombatHealth victim,Sprite sprite,Material mat,bool box,bool rocket){
  HostileHazard h=null;while(pool.Count>0&&!h)h=pool.Dequeue();if(!h){var go=new GameObject("Ataque anunciado");h=go.AddComponent<HostileHazard>();h.projectile=go.AddComponent<SpriteRenderer>();h.marker=new GameObject("Zona de aviso").AddComponent<LineRenderer>();h.marker.transform.SetParent(go.transform);h.marker.loop=true;h.marker.useWorldSpace=true;h.marker.startWidth=h.marker.endWidth=.06f;h.marker.sortingOrder=23;}
  h.start=from;h.Destination=to;h.damage=dmg;h.radius=size;h.flight=travel;h.fuse=delay;h.target=victim;h.pixel=sprite;h.material=mat;h.sweep=box;h.strike=false;h.missile=rocket;h.landed=false;h.started=Time.time;
  h.projectile.sprite=sprite;h.projectile.sharedMaterial=mat;h.projectile.sortingOrder=25;h.projectile.color=rocket?new Color(1,.3f,.15f):Color.yellow;h.projectile.enabled=!box;h.transform.localScale=rocket?new Vector3(.2f,.5f,1):Vector3.one*.23f;h.transform.position=from;h.marker.sharedMaterial=mat;h.marker.enabled=true;h.gameObject.SetActive(true);h.DrawWarning();return h;
 }
 void Update(){
  if(Time.timeScale==0||!target||!target.Alive)return;float t=Time.time-started;
  if(sweep){DrawWarning();if(t>=fuse)Explode();return;}
  float travel=missile?t-fuse:t;
  if(travel>=0&&!landed){float u=Mathf.Clamp01(travel/flight);Vector2 p=Vector2.Lerp(start,Destination,u)+(missile?Vector2.zero:Vector2.up*(4*u*(1-u)*3));var delta=p-(Vector2)transform.position;
   foreach(var hit in Physics2D.RaycastAll(transform.position,delta.normalized,delta.magnitude,~0)){if(!EncounterRoutes.Solid(hit.collider))continue;p=hit.point+hit.normal*.13f;Destination=p;landed=true;break;}
   transform.position=p;if(u>=1)landed=true;
  }
  DrawWarning();if(t>=flight+fuse)Explode();
 }
 void DrawWarning(){
  float pulse=.5f+.5f*Mathf.Sin(Time.time*14);marker.startColor=marker.endColor=new Color(1,.25f+pulse*.5f,0,.9f);
  if(sweep){marker.positionCount=4;var p=Destination;marker.SetPositions(new[]{new Vector3(p.x-radius/2,p.y-.2f),new Vector3(p.x+radius/2,p.y-.2f),new Vector3(p.x+radius/2,p.y+.2f),new Vector3(p.x-radius/2,p.y+.2f)});}
  else{marker.positionCount=32;for(int i=0;i<32;i++){float a=i*Mathf.PI*2/32;marker.SetPosition(i,Destination+new Vector2(Mathf.Cos(a)*radius,Mathf.Sin(a)*.18f));}}
 }
 public void Explode(){if(!gameObject.activeSelf)return;Detonations++;Vector2 at=sweep?Destination:(Vector2)transform.position;var victims=new HashSet<CombatHealth>();var hits=sweep?Physics2D.OverlapBoxAll(Destination+(strike?Vector2.up*.6f:Vector2.zero),new Vector2(radius,strike?1.6f:.4f),0,~0):Physics2D.OverlapCircleAll(at,radius,~0);foreach(var c in hits){var hp=c.GetComponentInParent<CombatHealth>();if(hp&&hp.team==CombatTeam.Player)victims.Add(hp);}foreach(var hp in victims){bool blocked=false;Vector2 delta=(Vector2)hp.transform.position-at;foreach(var hit in Physics2D.RaycastAll(at,delta.normalized,delta.magnitude,~0))if(EncounterRoutes.Solid(hit.collider)){blocked=true;break;}if(!blocked&&hp.Hit(damage,CombatTeam.Robot))DamageApplications++;}
  var effect=EnemyExplosion.Spawn(at,pixel,material);effect.transform.localScale=Vector3.one*(sweep?1:2);Release();}
 void Release(){gameObject.SetActive(false);if(pool.Count<32)pool.Enqueue(this);else Destroy(gameObject);}
 public static void ClearAll(){foreach(var h in FindObjectsByType<HostileHazard>())h.Release();}
}
}
