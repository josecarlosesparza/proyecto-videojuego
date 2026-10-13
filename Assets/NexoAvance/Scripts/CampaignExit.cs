using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
namespace NexoAvance {
public class CampaignExit:MonoBehaviour {
    public StreetMission mission;public Button next;bool loading;
    void Start(){next.onClick.AddListener(Continue);}
    public void Continue(){if(loading || !mission.Complete)return;loading=true;CampaignCarry.Save(mission.session);Time.timeScale=1;SceneManager.LoadSceneAsync("FabricaAutomatizada");}
}

}