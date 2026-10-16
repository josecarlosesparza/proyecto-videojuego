using System.Collections.Generic;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
namespace NexoAvance {
public enum ActionPhase { None, MeleePrepare, MeleeImpact, MeleeRecover, Grenade }
[DefaultExecutionOrder(60)]
public class PlayerActions:MonoBehaviour {
 public KeyCode meleeKey=KeyCode.K;
 public float preparation=.1f,impactDuration=.08f,recovery=.18f,cooldown=.45f;
 public Vector2 meleeSize=new Vector2(1.2f,1.1f);public float meleeOffset=.9f;
 [Tooltip("0 usa el daño de dos balas normales.")]public int meleeDamage;
 public ActionPhase Phase{get;private set;}public bool Busy=>Phase!=ActionPhase.None;
 public int Swings{get;private set;}public int Hits{get;private set;}
 ResistancePlayer player;PlayerCombat gun;CombatHealth hp;float began,next;int side;
 readonly HashSet<CombatHealth> struck=new HashSet<CombatHealth>();
 void Awake(){player=GetComponent<ResistancePlayer>();gun=GetComponent<PlayerCombat>();hp=GetComponent<CombatHealth>();}
 public bool TryMelee(){if(Busy||!hp.Alive||!player.enabled||player.Paused||Time.timeScale==0||!player.Grounded||Time.time<next)return false;Phase=ActionPhase.MeleePrepare;began=Time.time;next=Time.time+Mathf.Max(cooldown,preparation+impactDuration+recovery);side=player.visual.flipX?-1:1;struck.Clear();Swings++;return true;}
 public void GrenadeReleased(){Phase=ActionPhase.Grenade;began=Time.time;}
 public void ResetActions(){Phase=ActionPhase.None;next=began=0;struck.Clear();}
 void Update(){
  if(!hp.Alive||!player.enabled){Phase=ActionPhase.None;return;}if(player.Paused||Time.timeScale==0)return;
  if(!player.TestMode){bool pressed=false;
#if ENABLE_INPUT_SYSTEM
   var keyboard=Keyboard.current;if(keyboard!=null&&System.Enum.TryParse<UnityEngine.InputSystem.Key>(meleeKey.ToString(),true,out var key))pressed=keyboard[key].wasPressedThisFrame;
#elif ENABLE_LEGACY_INPUT_MANAGER
   pressed=Input.GetKeyDown(meleeKey);
#endif
   if(pressed)TryMelee();
  }
  if(Phase==ActionPhase.None)return;float t=Time.time-began;
  if(Phase==ActionPhase.Grenade){if(t>=.22f)Phase=ActionPhase.None;return;}
  if(!player.Grounded){Phase=ActionPhase.None;return;}player.visual.flipX=side<0;
  if(t<preparation)return;
  if(Phase==ActionPhase.MeleePrepare){Phase=ActionPhase.MeleeImpact;began=Time.time-preparation;t=preparation;next=Mathf.Max(next,Time.time+impactDuration+recovery);ActionPulse.Spawn((Vector2)transform.position+new Vector2(side*meleeOffset,0),gun.material,new Color(1,.85f,.35f),.55f,.15f,side);Impact();}
  if(t<preparation+impactDuration)Impact();
  else if(t<preparation+impactDuration+recovery)Phase=ActionPhase.MeleeRecover;else Phase=ActionPhase.None;
 }
 void Impact(){var origin=(Vector2)transform.position;var center=origin+new Vector2(side*meleeOffset,0);foreach(var c in Physics2D.OverlapBoxAll(center,meleeSize,0,~0)){var victim=c.GetComponentInParent<CombatHealth>();if(!victim||victim.team==hp.team||!victim.Alive||struck.Contains(victim))continue;var end=c.ClosestPoint(origin);bool blocked=false;foreach(var hit in Physics2D.RaycastAll(origin,(end-origin).normalized,Vector2.Distance(origin,end),~0))if(EncounterRoutes.Solid(hit.collider)&&hit.collider!=c){blocked=true;break;}if(blocked)continue;struck.Add(victim);if(victim.Hit(meleeDamage>0?meleeDamage:gun.damage*2,hp.team))Hits++;}}
}
public class ActionPulse:MonoBehaviour {
 LineRenderer line;float elapsed,duration,radius;Color color;int side;
 public static void Spawn(Vector2 position,Material material,Color color,float radius,float duration,int side=0){var go=new GameObject(side==0?"Impulso de doble salto":"Impacto cuerpo a cuerpo");go.transform.position=position;var p=go.AddComponent<ActionPulse>();p.radius=radius;p.duration=duration;p.color=color;p.side=side;p.line=go.AddComponent<LineRenderer>();p.line.sharedMaterial=material;p.line.useWorldSpace=false;p.line.positionCount=20;p.line.loop=side==0;p.line.startWidth=p.line.endWidth=.055f;p.line.sortingOrder=24;p.Draw();}
 void Draw(){float t=elapsed/duration;line.startColor=line.endColor=new Color(color.r,color.g,color.b,1-t);for(int i=0;i<20;i++){float a=side==0?i*Mathf.PI*2/20:(i/19f-.5f)*Mathf.PI;line.SetPosition(i,new Vector3(Mathf.Cos(a)*(side==0?1:side),Mathf.Sin(a)*(side==0?.3f:1),0)*radius*(.8f+t*.4f));}}
 void Update(){elapsed+=Time.unscaledDeltaTime;if(elapsed>=duration){Destroy(gameObject);return;}Draw();}
}
}
