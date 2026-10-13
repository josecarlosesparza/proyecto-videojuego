using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
namespace NexoAvance {
public class CampaignObjective:MonoBehaviour {
    public CombatHealth health;public GameObject door;public Sprite pixel;public Material material;
    void Awake(){health.Died+=Die;}
    void Die(){EnemyExplosion.Spawn(transform.position,pixel,material);foreach(var sr in GetComponentsInChildren<SpriteRenderer>())sr.enabled=false;foreach(var c in GetComponents<Collider2D>())c.enabled=false;if(door)door.SetActive(false);}
    void OnDestroy(){if(health)health.Died-=Die;}
}

}