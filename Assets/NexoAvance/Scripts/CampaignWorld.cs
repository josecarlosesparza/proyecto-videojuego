using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
namespace NexoAvance {
public static class CampaignCarry {
    public static bool Ready,MachineGun; public static int Grenades;
    public static void Save(CombatSession session){MachineGun=session.player.GetComponent<PlayerCombat>().HasMachineGun;Grenades=session.player.GetComponent<GrenadeInventory>()?.Count??0;Ready=true;}
    public static void Apply(CombatSession session){if(!Ready)return;session.player.GetComponent<PlayerCombat>().SetMachineGun(MachineGun);session.player.GetComponent<GrenadeInventory>().Give(Grenades);Ready=false;}
}
}