using UnityEngine;
namespace NexoAvance {
public enum HeroMotion { Idle, Run, Jump, Shoot, Hurt, Death }
[System.Serializable] public class HeroFrames {
    public Sprite[] idle, run, jump, shoot, hurt, death;
    public Sprite[] For(HeroMotion motion) {
        switch(motion){case HeroMotion.Run:return run;case HeroMotion.Jump:return jump;case HeroMotion.Shoot:return shoot;case HeroMotion.Hurt:return hurt;case HeroMotion.Death:return death;default:return idle;}
    }
}
[System.Serializable] public class HeroDesign {
    public string name; public Sprite pose; public Vector2 muzzle; public HeroFrames animations=new HeroFrames();
}
public class HeroRoster:ScriptableObject {
    public HeroDesign[] heroes;
    public static HeroRoster Load()=>Resources.Load<HeroRoster>("HeroRoster");
}
public static class HeroSelection {
    public static int Index {get;private set;}
    public static void Choose(int index){Index=Mathf.Clamp(index,0,3);}
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void Reset(){Index=0;CampaignCarry.Ready=false;}
}
}
