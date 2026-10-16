using UnityEngine;
namespace NexoAvance {
public class PlayerSlow:MonoBehaviour {
 float until;float factor=1;
 public float Factor=>Time.time<until?factor:1;
 public void Apply(float multiplier,float seconds){factor=Mathf.Clamp(multiplier,.5f,1);until=Time.time+Mathf.Clamp(seconds,0,3);}
 public void Clear(){until=0;factor=1;}
}
}
