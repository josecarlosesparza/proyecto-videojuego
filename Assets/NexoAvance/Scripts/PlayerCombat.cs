using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
namespace NexoAvance {
public class PlayerCombat : MonoBehaviour {
    public CombatSession session;
    public ResistancePlayer movement;
    public CombatHealth health;
    public Sprite pixel;
    public Material material;
    [Min(1)] public int damage=10;
    [Min(.05f)] public float shotInterval=.25f;
    public float bulletSpeed=18;
    public bool TestFire {get;set;}
    public bool HasMachineGun {get;private set;}
    float nextShot,nextBurst;
    int remaining;
    Transform gunVisual;
    bool CanShoot => health.Alive && movement.enabled && !(GetComponent<PlayerActions>()?.Busy ?? false);
    void Update(){
        if(gunVisual)gunVisual.localPosition=new Vector3(movement.visual.flipX?-.47f:.47f,-.12f,0);
        if(!CanShoot){remaining=0;return;}
        if(movement.Paused || Time.timeScale==0)return;
        if(remaining>0 && Time.time>=nextBurst){Emit();remaining--;nextBurst=Time.time+.08f;}
        bool fire=TestFire;
        if(!movement.TestMode){
#if ENABLE_INPUT_SYSTEM
            fire=Keyboard.current!=null && Keyboard.current.jKey.isPressed;
#elif ENABLE_LEGACY_INPUT_MANAGER
            fire=Input.GetKey(KeyCode.J);
#endif
        }
        if(fire)TryShoot();
    }
    public bool TryShoot(){
        if(!CanShoot || movement.Paused || Time.timeScale==0 || remaining>0 || Time.time<nextShot)return false;
        nextShot=Time.time+(HasMachineGun?.5f:shotInterval);Emit();
        if(HasMachineGun){remaining=2;nextBurst=Time.time+.08f;}
        return true;
    }
    public Vector2 LastShotOrigin {get;private set;}
    void Emit(){
        float direction=movement.visual.flipX?-1:1;
        var appearance=GetComponent<HeroAppearance>();
        if(appearance)appearance.Fired();
        Vector2 origin=appearance?appearance.Muzzle:GetComponent<Rigidbody2D>().position+new Vector2(0,-.12f);
        // Do not spawn beyond a wall when the drawn barrel extends outside the shared hitbox.
        Vector2 from=new Vector2(transform.position.x,origin.y);
        foreach(var hit in Physics2D.RaycastAll(from,Vector2.right*direction,Mathf.Abs(origin.x-from.x))){
            if(hit.collider.isTrigger || hit.collider.GetComponentInParent<ResistancePlayer>() || hit.collider.GetComponent<ThrownGrenade>())continue;
            origin=hit.point-Vector2.right*direction*.01f;break;
        }
        LastShotOrigin=origin;
        CombatBullet.Fire(origin,direction,CombatTeam.Player,HasMachineGun?18:damage,HasMachineGun?23:bulletSpeed,pixel,material);
        }
    public void SetMachineGun(bool value){
        HasMachineGun=value;remaining=0;nextShot=0;
        if(value && !gunVisual && !GetComponent<HeroAppearance>()){var sr=new GameObject("Metralleta del jugador").AddComponent<SpriteRenderer>();gunVisual=sr.transform;gunVisual.SetParent(transform,false);gunVisual.localScale=new Vector3(.6f,.14f,1);sr.sprite=pixel;sr.sharedMaterial=material;sr.sortingOrder=12;sr.color=new Color(.9f,.7f,.3f);}
        if(gunVisual)gunVisual.gameObject.SetActive(value && !GetComponent<HeroAppearance>());
    }
    public void ResetEncounter(){nextShot=0;remaining=0;TestFire=false;if(session)session.ResetCombat();}
}
}
