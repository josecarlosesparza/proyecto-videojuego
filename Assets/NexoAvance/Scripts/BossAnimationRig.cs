using UnityEngine;
namespace NexoAvance {
[System.Serializable]public class TentaclePart {public Transform pivot;public float angle=7,frequency=1,phase;[System.NonSerialized]public Quaternion rest;}
// The optional rig is gated by a reconstructed body: it never overlays cutouts on painted tentacles.
public class BossAnimationRig:MonoBehaviour {
 public Sprite bodyWithoutTentacles;public TentaclePart[] tentacles=new TentaclePart[0];
 public Sprite[] idle,prepareCannon,fireCannon,prepareSweep,sweep,prepareMissile,fireMissile,prepareStrike,strike,hurt,exposeCore,defeat;
 public float framesPerSecond=10;public bool enableSeparatedParts;
 OctopusBoss boss;float clock;string state;
 public string State=>state;
 void Awake(){boss=GetComponent<OctopusBoss>();foreach(var t in tentacles)if(t.pivot)t.rest=t.pivot.localRotation;}
 void LateUpdate(){if(!boss.art)return;string next=!boss.health.Alive?"Defeat":boss.Preparing?"Prepare"+boss.CurrentPattern:boss.Firing?"Attack"+boss.CurrentPattern:boss.Phase==3?"Core":"Idle";if(next!=state){state=next;clock=0;}clock+=!boss.health.Alive?Time.unscaledDeltaTime:Time.deltaTime;
  bool parts=enableSeparatedParts&&bodyWithoutTentacles&&tentacles.Length>0;foreach(var t in tentacles)if(t.pivot){t.pivot.gameObject.SetActive(parts&&boss.health.Alive);if(parts)t.pivot.localRotation=t.rest*Quaternion.Euler(0,0,Mathf.Sin(Time.time*t.frequency+t.phase)*t.angle);}
  if(parts){boss.art.sprite=bodyWithoutTentacles;return;}
  var frames=state=="Defeat"?defeat:state=="Prepare0"?prepareCannon:state=="Attack0"?fireCannon:state=="Prepare1"?prepareSweep:state=="Attack1"?sweep:state=="Prepare2"?prepareMissile:state=="Attack2"?fireMissile:state=="Prepare3"?prepareStrike:state=="Attack3"?strike:state=="Core"?exposeCore:idle;
  bool fallback=frames==null||frames.Length==0;if(fallback){if(state=="Defeat")return;frames=idle;}if(frames==null||frames.Length==0)return;int i=(int)((fallback?Time.time:clock)*framesPerSecond);boss.art.sprite=frames[state=="Idle"||fallback?i%frames.Length:Mathf.Min(i,frames.Length-1)];if(state=="Defeat")boss.art.enabled=true;
 }
}
}
