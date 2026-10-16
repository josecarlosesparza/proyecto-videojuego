using UnityEngine;
namespace NexoAvance {
[DisallowMultipleComponent]
public class HeroAppearance:MonoBehaviour {
 public HeroRoster roster;public int AppliedIndex{get;private set;}public HeroMotion State{get;private set;}
 ResistancePlayer player;CombatHealth health;HeroDesign design;float shootingUntil,hurtUntil,frameTime,locomotionTime;int previousHealth,frame;bool subscribed;
 public void Initialize(ResistancePlayer owner){player=owner;if(!roster)roster=HeroRoster.Load();if(!roster||roster.heroes.Length!=4){Debug.LogError("Falta HeroRoster");return;}AppliedIndex=HeroSelection.Index;design=roster.heroes[AppliedIndex];player.idle=new[]{design.pose};player.run=new[]{design.pose};player.jump=design.pose;player.visual.sprite=design.pose;player.visual.enabled=true;player.visual.color=Color.white;player.visual.transform.localPosition=Vector3.down*.85f;player.visual.transform.localScale=Vector3.one;player.visual.transform.localRotation=Quaternion.identity;foreach(var a in GetComponentsInChildren<Animator>())a.enabled=false;health=GetComponent<CombatHealth>();ResetVisual();}
 void Start(){if(!player)Initialize(GetComponent<ResistancePlayer>());previousHealth=health.Current;health.Changed+=Changed;subscribed=true;}
 void Changed(){if(health.Current<previousHealth)hurtUntil=Time.time+.15f;previousHealth=health.Current;}
 void OnDestroy(){if(subscribed&&health)health.Changed-=Changed;}
 public void Fired(){shootingUntil=Time.time+.12f;RefreshPose();}
 public void RefreshPose(){RenderPose(0);}
 public void ResetVisual(){shootingUntil=hurtUntil=frameTime=locomotionTime=0;State=HeroMotion.Idle;frame=0;}
 public Vector2 Muzzle{get{var offset=design==null?new Vector2(.5f,.1f):design.muzzle;if(design!=null&&(State==HeroMotion.Run||State==HeroMotion.ShootRun)&&design.animations.runMuzzles!=null&&frame<design.animations.runMuzzles.Length)offset=design.animations.runMuzzles[frame];if(design!=null&&design.spriteMuzzles!=null)foreach(var marker in design.spriteMuzzles)if(marker.sprite==player.visual.sprite){offset=marker.offset;break;}offset.x*=player.visual.flipX?-1:1;return (Vector2)transform.position+offset;}}
 void LateUpdate(){RenderPose(health&&!health.Alive?Time.unscaledDeltaTime:Time.deltaTime);}
 void RenderPose(float dt){
  if(design==null)return;var action=GetComponent<PlayerActions>();bool moving=Mathf.Abs(player.Velocity.x)>.25f;bool firing=Time.time<shootingUntil;
  HeroMotion state;
  if(!health.Alive)state=HeroMotion.Death;else if(Time.time<hurtUntil)state=HeroMotion.Hurt;
  else if(action&&action.Phase!=ActionPhase.None)state=action.Phase==ActionPhase.Grenade?HeroMotion.Grenade:action.Phase==ActionPhase.MeleePrepare?HeroMotion.MeleePrepare:action.Phase==ActionPhase.MeleeImpact?HeroMotion.MeleeImpact:HeroMotion.MeleeRecover;
  else if(!player.Grounded)state=firing?HeroMotion.ShootAir:Time.time<player.DoubleJumpUntil?HeroMotion.DoubleJump:Time.time<player.JumpStartUntil?HeroMotion.JumpStart:player.Velocity.y>0?HeroMotion.Rise:HeroMotion.Fall;
  else state=firing?(moving?HeroMotion.ShootRun:HeroMotion.Shoot):Time.time<player.LandUntil?HeroMotion.Land:moving?HeroMotion.Run:HeroMotion.Idle;
  if(state!=State){State=state;frameTime=0;}frameTime+=dt;if(moving&&player.Grounded)locomotionTime+=dt;
  var frames=design.animations.For(state);if(state==HeroMotion.ShootAir)frames=player.Velocity.y>0?design.animations.rise:design.animations.fall;if(frames==null||frames.Length==0){player.visual.sprite=design.pose;frame=0;return;}
  bool loop=state==HeroMotion.Idle||state==HeroMotion.Run||state==HeroMotion.ShootRun;float clock=state==HeroMotion.Run||state==HeroMotion.ShootRun?locomotionTime:frameTime;int index=(int)(clock*design.animations.framesPerSecond);frame=loop?index%frames.Length:Mathf.Min(index,frames.Length-1);player.visual.sprite=frames[frame];
 }
}
}
