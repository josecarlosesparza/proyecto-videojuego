using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
namespace NexoAvance {
public class CampaignPickup:MonoBehaviour {
    public CombatSession session;public bool grenades;bool taken;
    void Update(){if(taken || !session.health.Alive || Time.timeScale==0)return;if(Vector2.Distance(transform.position,session.player.transform.position)>1.2f)return;
        if(grenades)session.player.GetComponent<GrenadeInventory>().Give(3);else{if(session.health.Current>=session.health.maximum)return;session.health.Heal(35);}
        taken=true;foreach(var sr in GetComponentsInChildren<SpriteRenderer>())sr.enabled=false;session.GetComponent<CampaignMission>().Notice(grenades?"+3 GRANADAS":"+35 SALUD",2);
    }
}

}