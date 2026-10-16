using UnityEngine;
namespace NexoAvance {
public enum HeroMotion { Idle, Run, Jump, Shoot, Hurt, Death, JumpStart, Rise, Fall, DoubleJump, Land, ShootRun, ShootAir, Grenade, MeleePrepare, MeleeImpact, MeleeRecover }
[System.Serializable] public class HeroFrames {
 public Sprite[] idle,run,jump,shoot,hurt,death,jumpStart,rise,fall,doubleJump,land,shootRun,shootAir,grenade,meleePrepare,meleeImpact,meleeRecover;
 public Vector2[] runMuzzles;public float framesPerSecond=10;
 public Sprite[] For(HeroMotion s){switch(s){case HeroMotion.Run:return run;case HeroMotion.Jump:return jump;case HeroMotion.Shoot:return shoot;case HeroMotion.Hurt:return hurt;case HeroMotion.Death:return death;case HeroMotion.JumpStart:return jumpStart;case HeroMotion.Rise:return rise;case HeroMotion.Fall:return fall;case HeroMotion.DoubleJump:return doubleJump;case HeroMotion.Land:return land;case HeroMotion.ShootRun:return shootRun!=null&&shootRun.Length>0?shootRun:run;case HeroMotion.ShootAir:return shootAir;case HeroMotion.Grenade:return grenade;case HeroMotion.MeleePrepare:return meleePrepare;case HeroMotion.MeleeImpact:return meleeImpact;case HeroMotion.MeleeRecover:return meleeRecover;default:return idle;}}
}
[System.Serializable] public class SpriteMuzzle {public Sprite sprite;public Vector2 offset;}
[System.Serializable] public class HeroDesign {public string name;public Sprite pose;public Vector2 muzzle;public SpriteMuzzle[] spriteMuzzles;public HeroFrames animations=new HeroFrames();}
public class HeroRoster:ScriptableObject {public HeroDesign[] heroes;public static HeroRoster Load()=>Resources.Load<HeroRoster>("HeroRoster");}
public static class HeroSelection {public static int Index{get;private set;}public static void Choose(int index){Index=Mathf.Clamp(index,0,3);}[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]static void Reset(){Index=0;CampaignCarry.Ready=false;}}
}
