using UnityEngine;
namespace NexoAvance {
public enum RobotMotion { Idle, Move, Prepare, Attack, Recover, Hurt, Destroy }
// Optional real frame clips. No translation/stretch of a complete static sprite.
public class RobotAnimation:MonoBehaviour {
 public RobotMotion State{get;private set;}StreetEnemy enemy;RobotArsenal arsenal;EnemyPursuit movement;float clock,hurtUntil;int previous;
 void Awake(){enemy=GetComponent<StreetEnemy>();arsenal=GetComponent<RobotArsenal>();movement=GetComponent<EnemyPursuit>();}
 void Start(){previous=enemy.health.Current;enemy.health.Changed+=Changed;}
 void Changed(){if(enemy.health.Current<previous)hurtUntil=Time.time+.15f;previous=enemy.health.Current;}
 public void RefreshPose(){RenderPose(0);}
 void LateUpdate(){RenderPose(Time.deltaTime);}
 void RenderPose(float dt){if(!arsenal.profile)return;var p=arsenal.profile;var state=!enemy.health.Alive?RobotMotion.Destroy:Time.time<hurtUntil?RobotMotion.Hurt:arsenal.Charging?RobotMotion.Prepare:arsenal.Attacking?RobotMotion.Attack:arsenal.Recovering?RobotMotion.Recover:movement&&movement.Moving?RobotMotion.Move:RobotMotion.Idle;if(state!=State){State=state;clock=0;}clock+=dt;var frames=state==RobotMotion.Destroy?p.death:state==RobotMotion.Hurt?p.hurt:state==RobotMotion.Prepare?p.prepare:state==RobotMotion.Attack?p.shoot:state==RobotMotion.Recover?p.recover:state==RobotMotion.Move?p.run:p.idle;if(frames==null||frames.Length==0){enemy.visual.sprite=p.pose;return;}int i=(int)(clock*Mathf.Max(1,p.animationFps));bool loop=state==RobotMotion.Move||state==RobotMotion.Idle||state==RobotMotion.Attack;enemy.visual.sprite=frames[loop?i%frames.Length:Mathf.Min(i,frames.Length-1)];if(state==RobotMotion.Destroy)enemy.visual.enabled=true;}
 void OnDestroy(){if(enemy&&enemy.health)enemy.health.Changed-=Changed;}
}
}
