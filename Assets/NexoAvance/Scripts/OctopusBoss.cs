using UnityEngine;
namespace NexoAvance {
public class OctopusBoss : MonoBehaviour {
    public CombatHealth health,target;
    public Sprite pixel;
    public Material material;
    public Transform[] arms;
    public SpriteRenderer warning;
    public bool AutoFire=true;
    float nextAttack,readyAt,burstAt;
    int pattern,shots;
    bool charging;
    Collider2D hitbox;
    void Awake(){hitbox=GetComponent<Collider2D>();health.Died+=Die;}
    void OnEnable(){nextAttack=Time.time+2;}
    void Update(){
        if(!health.Alive || !target.Alive || Time.timeScale==0)return;
        for(int i=0;i<arms.Length;i++)arms[i].localRotation=Quaternion.Euler(0,0,Mathf.Sin(Time.time*2+i*.9f)*12);
        if(!AutoFire)return;
        if(shots>0 && Time.time>=burstAt){Attack();shots--;burstAt=Time.time+.28f;}
        if(!charging && shots==0 && Time.time>=nextAttack){charging=true;readyAt=Time.time+.95f;warning.enabled=true;}
        if(charging && Time.time>=readyAt){charging=false;warning.enabled=false;pattern=(pattern+1)%3;shots=health.Current<health.maximum/2?4:3;burstAt=Time.time;nextAttack=Time.time+(health.Current<health.maximum/2?3.2f:4.2f);}
    }
    void Attack(){
        float side=Mathf.Sign(target.transform.position.x-transform.position.x);
        Vector2 origin=(Vector2)transform.position+new Vector2(side*1.2f,-.4f);
        Vector2 aim=((Vector2)target.transform.position+Vector2.down*.12f-origin).normalized;
        if(pattern==0){origin.y=-.62f;aim=Vector2.right*side;}
        CombatBullet.Fire(origin,aim,CombatTeam.Robot,12,pattern==0?6:5,pixel,material);
        if(pattern==2){CombatBullet.Fire(origin,Quaternion.Euler(0,0,15)*aim,CombatTeam.Robot,12,5,pixel,material);CombatBullet.Fire(origin,Quaternion.Euler(0,0,-15)*aim,CombatTeam.Robot,12,5,pixel,material);}
    }
    void Die(){
        for(int i=0;i<7;i++){var e=EnemyExplosion.Spawn(transform.position+new Vector3((i%3-1)*1.1f,(i/3-1)*.8f,0),pixel,material);e.transform.localScale=Vector3.one*1.5f;}
        foreach(var sr in GetComponentsInChildren<SpriteRenderer>())sr.enabled=false;
        hitbox.enabled=false;charging=false;shots=0;
    }
    public void ResetBoss(){if(!hitbox)hitbox=GetComponent<Collider2D>();health.ResetHealth();hitbox.enabled=true;foreach(var sr in GetComponentsInChildren<SpriteRenderer>())sr.enabled=true;warning.enabled=false;nextAttack=Time.time+2;charging=false;shots=0;pattern=0;}
    void OnDestroy(){if(health)health.Died-=Die;}
}
}
