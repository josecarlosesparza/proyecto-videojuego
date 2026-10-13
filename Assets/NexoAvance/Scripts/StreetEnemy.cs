using UnityEngine;
namespace NexoAvance {
public enum StreetEnemyKind { Fusilero, Dron, Blindado, Torreta }
public class StreetEnemy : MonoBehaviour {
    public StreetEnemyKind kind;
    public CombatHealth health, target;
    public SpriteRenderer visual, warning;
    public Sprite pixel;
    public Material material;
    public bool AutoFire=true;
    public Vector3 origin;
    float nextAttack, readyAt, burstAt;
    int burst;
    bool charging;
    Collider2D hitbox;
    void Awake(){origin=transform.position;hitbox=GetComponent<Collider2D>();health.Died+=Die;}
    void OnEnable(){nextAttack=Time.time+1.2f;charging=false;burst=0;}
    void Update(){
        if(!health.Alive || !target.Alive || Time.timeScale==0)return;
        float dx=target.transform.position.x-transform.position.x;
        visual.flipX=dx<0;
        if(kind==StreetEnemyKind.Dron)transform.position=origin+new Vector3(Mathf.Sin(Time.time*1.1f)*.65f,Mathf.Sin(Time.time*2)*.2f,0);
        bool visible=AutoFire && Mathf.Abs(dx)<12 && Visible();
        if(!visible){charging=false;warning.enabled=false;burst=0;return;}
        if(burst>0 && Time.time>=burstAt){Shoot();burst--;burstAt=Time.time+.24f;}
        if(!charging && burst==0 && Time.time>=nextAttack){charging=true;readyAt=Time.time+((kind==StreetEnemyKind.Blindado || kind==StreetEnemyKind.Torreta)?.9f:.65f);}
        warning.enabled=charging;
        if(charging && Time.time>=readyAt){charging=false;warning.enabled=false;Shoot();if((kind==StreetEnemyKind.Blindado || kind==StreetEnemyKind.Torreta)){burst=2;burstAt=Time.time+.24f;}nextAttack=Time.time+(kind==StreetEnemyKind.Dron?2.8f:2.3f);}
    }
    bool Visible(){
        Vector2 a=(Vector2)transform.position+Vector2.down*.12f;
        Vector2 b=(Vector2)target.transform.position+Vector2.down*.12f;
        foreach(var h in Physics2D.RaycastAll(a,(b-a).normalized,(b-a).magnitude,~0)){
            var entity=h.collider.GetComponentInParent<CombatHealth>();if(entity==health || (entity && entity.team==CombatTeam.Robot))continue;
            return entity==target;
        }return false;
    }
    void Shoot(){
        Vector2 from=(Vector2)transform.position+Vector2.down*.12f;
        Vector2 aim=kind==StreetEnemyKind.Dron ? ((Vector2)target.transform.position+Vector2.down*.12f-from).normalized : Vector2.right*Mathf.Sign(target.transform.position.x-from.x);
        CombatBullet.Fire(from,aim,CombatTeam.Robot,kind==StreetEnemyKind.Fusilero?10:8,kind==StreetEnemyKind.Dron?5:7,pixel,material);
    }
    void Die(){EnemyExplosion.Spawn(transform.position,pixel,material);visual.enabled=false;warning.enabled=false;hitbox.enabled=false;charging=false;burst=0;}
    public void ResetEnemy(){if(!hitbox)hitbox=GetComponent<Collider2D>();transform.position=origin;health.ResetHealth();visual.enabled=true;hitbox.enabled=true;warning.enabled=false;nextAttack=Time.time+1.2f;charging=false;burst=0;}
    void OnDestroy(){if(health)health.Died-=Die;}
}
}
