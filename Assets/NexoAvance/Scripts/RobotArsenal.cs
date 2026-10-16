using UnityEngine;
namespace NexoAvance {
public class RobotArsenal:MonoBehaviour {
 public RobotProfile profile;public bool bossSupport;
 StreetEnemy enemy;float next,ready,tick,stop;int shots,shotIndex;bool charging,burning;
 LineRenderer flame;
 public Vector2 LastMuzzle{get;private set;}
 public int Emitted{get;private set;}
 public bool Charging=>charging;float attackVisibleUntil;public bool Attacking=>burning||Time.time<attackVisibleUntil;public bool Recovering=>attackVisibleUntil>0&&Time.time<next;
 void Awake(){enemy=GetComponent<StreetEnemy>();if(!GetComponent<RobotAnimation>())gameObject.AddComponent<RobotAnimation>();}
 void OnEnable(){next=Time.time+1.2f;charging=burning=false;shots=0;if(profile&&enemy&&enemy.health){enemy.health.maximum=Mathf.Max(1,profile.health);enemy.health.ResetHealth();}}
 public Vector2 Muzzle(int index=0){var m=index%2==0?profile.muzzleA:profile.muzzleB;if(m==Vector2.zero)m=profile.muzzleA;if(profile.spriteMuzzles!=null)foreach(var anchor in profile.spriteMuzzles)if(anchor.sprite==enemy.visual.sprite){m=anchor.offset+(index%2==1?profile.muzzleB-profile.muzzleA:Vector2.zero);break;}m.x*=enemy.visual.flipX?-1:1;Vector2 end=(Vector2)transform.position+m;Vector2 start=new Vector2(transform.position.x,end.y);foreach(var hit in Physics2D.RaycastAll(start,(end-start).normalized,(end-start).magnitude,~0))if(EncounterRoutes.Solid(hit.collider)){end=hit.point-(end-start).normalized*.03f;break;}return end;}
 public bool Sight(Vector2 from){var to=(Vector2)enemy.target.transform.position;foreach(var h in Physics2D.RaycastAll(from,(to-from).normalized,(to-from).magnitude+.1f,~0)){if(h.collider.isTrigger||h.collider.GetComponentInParent<StreetEnemy>()||h.collider.GetComponent<ThrownGrenade>())continue;return h.collider.GetComponentInParent<CombatHealth>()==enemy.target;}return false;}
 public void Tick(){
  if(!profile||!enemy.health.Alive||!enemy.target||!enemy.target.Alive||Time.timeScale==0||!enemy.AutoFire){Cancel();return;}
  var pursuit=GetComponent<EnemyPursuit>();if(pursuit&&pursuit.director&&pursuit.director.Stopped){Cancel();return;}
  float dx=enemy.target.transform.position.x-transform.position.x;if(Mathf.Abs(dx)>.3f)enemy.visual.flipX=dx<0;
  bool visible=Mathf.Abs(dx)<=profile.range&&Sight(Muzzle());
  if(burning){if(Time.time>=stop){burning=false;if(flame)flame.enabled=false;next=Time.time+profile.cooldown;}else Flame();return;}
  if(shots>0&&Time.time>=tick){if(visible)Emit();shots--;tick=Time.time+profile.burstInterval;if(shots==0)next=Time.time+profile.cooldown;}
  if(!visible){charging=false;enemy.warning.enabled=false;return;}
  if(shots==0&&!charging&&Time.time>=next){charging=true;ready=Time.time+profile.warning;}
  enemy.warning.enabled=charging;enemy.warning.transform.position=Muzzle();
  if(charging&&Time.time>=ready){charging=false;enemy.warning.enabled=false;shotIndex=0;if(profile.attack==RobotAttack.Flame){burning=true;stop=Time.time+profile.fireDuration;tick=Time.time;Flame();}else{shots=profile.burstCount;tick=Time.time;}}
 }
 void Emit(){
  attackVisibleUntil=Time.time+.12f;GetComponent<RobotAnimation>()?.RefreshPose();var from=Muzzle(profile.attack==RobotAttack.DualBurst?shotIndex:0);LastMuzzle=from;shotIndex++;Emitted++;attackVisibleUntil=Time.time+.12f;
  if(profile.attack==RobotAttack.Grenade){HostileHazard.Lob(from,enemy.target.transform.position,profile.damage,profile.blastRadius,profile.flightTime,profile.fuse,enemy);return;}
  var aim=((Vector2)enemy.target.transform.position+Vector2.down*.12f-from).normalized;
  var b=CombatBullet.Fire(from,aim,CombatTeam.Robot,profile.damage,profile.projectileSpeed,enemy.pixel,enemy.material);
  if(profile.attack==RobotAttack.Electric){b.slowFactor=profile.slowFactor;b.slowDuration=profile.slowDuration;var sr=b.GetComponent<SpriteRenderer>();sr.color=new Color(.8f,.3f,1);b.transform.localScale=Vector3.one*.3f;}
 }
 void Flame(){
  var a=Muzzle();float side=enemy.visual.flipX?-1:1;Vector2 b=a+Vector2.right*side*profile.range;
  foreach(var hit in Physics2D.RaycastAll(a,Vector2.right*side,profile.range,~0)){if(!EncounterRoutes.Solid(hit.collider))continue;b=hit.point;break;}
  if(!flame){flame=new GameObject("Llama Â· alcance visible").AddComponent<LineRenderer>();flame.transform.SetParent(transform);flame.material=enemy.material;flame.sortingOrder=24;flame.positionCount=2;flame.startWidth=.15f;flame.endWidth=.65f;flame.startColor=Color.yellow;flame.endColor=new Color(1,.25f,0,.65f);}
  flame.enabled=true;flame.SetPosition(0,a);flame.SetPosition(1,b);
  if(Time.time>=tick){tick=Time.time+Mathf.Max(.1f,profile.fireTick);var target=enemy.target.GetComponent<Collider2D>();var center=(a+b)*.5f;var size=new Vector2(Mathf.Abs(b.x-a.x),.65f);if(target&&target.bounds.Intersects(new Bounds(center,size))&&Sight(a))enemy.target.Hit(profile.damage,CombatTeam.Robot);}
 }
 public void Cancel(){charging=burning=false;shots=0;if(enemy&&enemy.warning)enemy.warning.enabled=false;if(flame)flame.enabled=false;}
 void OnDisable(){Cancel();}
}
}
