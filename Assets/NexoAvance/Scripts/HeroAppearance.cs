using UnityEngine;
namespace NexoAvance {
[DisallowMultipleComponent]
public class HeroAppearance:MonoBehaviour {
    public HeroRoster roster;
    public int AppliedIndex {get;private set;}
    public HeroMotion State {get;private set;}
    ResistancePlayer player; CombatHealth health; HeroDesign design; float shootingUntil,hurtUntil,frameTime; int previousHealth;
    public void Initialize(ResistancePlayer owner){
        player=owner;if(!roster)roster=HeroRoster.Load();
        if(!roster || roster.heroes.Length!=4){Debug.LogError("Falta HeroRoster con los cuatro diseños originales",this);return;}
        AppliedIndex=HeroSelection.Index;design=roster.heroes[AppliedIndex];
        player.idle=new[]{design.pose};player.run=new[]{design.pose};player.jump=design.pose;
        var visual=player.visual;visual.sprite=design.pose;visual.enabled=true;visual.color=Color.white;
        visual.transform.localPosition=new Vector3(0,-.85f,0);visual.transform.localScale=Vector3.one;visual.transform.localRotation=Quaternion.identity;
        foreach(var animator in GetComponentsInChildren<Animator>())animator.enabled=false;
        health=GetComponent<CombatHealth>();
    }
    void Start(){if(!player)Initialize(GetComponent<ResistancePlayer>());previousHealth=health.Current;health.Changed+=Changed;}
    void Changed(){if(health.Current<previousHealth)hurtUntil=Time.time+.15f;previousHealth=health.Current;}
    void OnDestroy(){if(health)health.Changed-=Changed;}
    public void Fired(){shootingUntil=Time.time+.12f;}
    public Vector2 Muzzle {
        get {var offset=design==null?new Vector2(.5f,.1f):design.muzzle;offset.x*=player.visual.flipX?-1:1;return (Vector2)transform.position+offset;}
    }
    void LateUpdate(){
        if(design==null)return;
        var state=!health.Alive?HeroMotion.Death:Time.time<hurtUntil?HeroMotion.Hurt:Time.time<shootingUntil?HeroMotion.Shoot:!player.Grounded?HeroMotion.Jump:Mathf.Abs(player.Velocity.x)>.25f?HeroMotion.Run:HeroMotion.Idle;
        if(state!=State){State=state;frameTime=0;}frameTime+=Time.deltaTime;
        var frames=design.animations.For(state);player.visual.sprite=frames!=null&&frames.Length>0?frames[(int)(frameTime*10)%frames.Length]:design.pose;
    }
}
}
