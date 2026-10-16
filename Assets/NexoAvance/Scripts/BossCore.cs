using UnityEngine;
namespace NexoAvance {
public class BossCore:MonoBehaviour {
 public OctopusBoss boss;public float multiplier=2,radius=.9f;
 LineRenderer halo;
 public bool Vulnerable=>boss&&boss.Phase==3&&boss.health.Alive;
 public bool InTrajectory(Vector2 from,Vector2 direction){var delta=(Vector2)transform.position-from;float t=Vector2.Dot(delta,direction);return Vulnerable&&t>=0&&(delta-direction*t).magnitude<radius;}
 public bool Hit(int damage,CombatTeam source){return Vulnerable&&boss.health.Hit(Mathf.RoundToInt(damage*multiplier),source);}
 void Update(){GetComponent<CircleCollider2D>().radius=radius;if(!halo){halo=gameObject.AddComponent<LineRenderer>();halo.sharedMaterial=boss.material;halo.useWorldSpace=false;halo.loop=true;halo.positionCount=32;halo.startWidth=halo.endWidth=.055f;halo.sortingOrder=24;halo.startColor=halo.endColor=Color.cyan;for(int i=0;i<32;i++){float a=i*Mathf.PI*2/32;halo.SetPosition(i,new Vector3(Mathf.Cos(a),Mathf.Sin(a),0)*radius);}}halo.enabled=Vulnerable;}
}
}
