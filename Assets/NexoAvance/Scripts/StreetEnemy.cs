using UnityEngine;
namespace NexoAvance {
public enum StreetEnemyKind { Fusilero, Dron, Blindado, Torreta, Cazador, Bombardero, Incinerador, Electrico }
public class StreetEnemy:MonoBehaviour {
    public StreetEnemyKind kind;public CombatHealth health,target;public SpriteRenderer visual,warning;public Sprite pixel;public Material material;
    public bool AutoFire=true;public Vector3 origin;
    [Header("Ataque")][Min(.2f)]public float attackInterval=2.3f;
    [Min(.2f)]public float warningTime=.65f;
    [Min(1)]public float attackRange=12;
    [Min(1)]public int damage=10;
    float nextAttack,readyAt,burstAt;int burst;bool charging;Collider2D hitbox;EnemyPursuit pursuit;
    void Awake(){origin=transform.position;hitbox=GetComponent<Collider2D>();pursuit=GetComponent<EnemyPursuit>();health.Died+=Die;}
    void OnEnable(){nextAttack=Time.time+1.2f;charging=false;burst=0;}
    void Update(){
        var arsenal=GetComponent<RobotArsenal>();if(arsenal&&arsenal.profile){arsenal.Tick();return;}
        if(!health.Alive||!target||!target.Alive||Time.timeScale==0||(pursuit&&pursuit.director&&pursuit.director.Stopped)){warning.enabled=false;charging=false;burst=0;return;}
        float dx=target.transform.position.x-transform.position.x;if(Mathf.Abs(dx)>.3f)visual.flipX=dx<0;
        float range=pursuit&&pursuit.melee?1.45f:attackRange;
        bool visible=AutoFire&&Mathf.Abs(dx)<range&&HasSight();
        if(!visible){charging=false;warning.enabled=false;burst=0;return;}
        if(burst>0&&Time.time>=burstAt){Shoot();burst--;burstAt=Time.time+.24f;}
        if(!charging&&burst==0&&Time.time>=nextAttack){charging=true;readyAt=Time.time+warningTime;}
        warning.enabled=charging;
        if(charging&&Time.time>=readyAt){charging=false;warning.enabled=false;Shoot();if((kind==StreetEnemyKind.Blindado||kind==StreetEnemyKind.Torreta)&&!(pursuit&&pursuit.melee)){burst=2;burstAt=Time.time+.24f;}nextAttack=Time.time+attackInterval;}
    }
    public bool HasSight(){
        if(!target)return false;var arsenal=GetComponent<RobotArsenal>();if(arsenal&&arsenal.profile)return arsenal.Sight(arsenal.Muzzle());Vector2 a=(Vector2)transform.position+Vector2.down*.12f,b=(Vector2)target.transform.position+Vector2.down*.12f;
        foreach(var h in Physics2D.RaycastAll(a,(b-a).normalized,(b-a).magnitude+.05f,~0)){
            if(h.collider.isTrigger||h.collider.GetComponentInParent<StreetEnemy>()||h.collider.GetComponent<ThrownGrenade>())continue;
            return h.collider.GetComponentInParent<CombatHealth>()==target;
        }return false;
    }
    void Shoot(){if(!HasSight())return;if(pursuit&&pursuit.melee){if(Vector2.Distance(transform.position,target.transform.position)<1.5f)target.Hit(damage,CombatTeam.Robot);return;}
        Vector2 from=(Vector2)transform.position+Vector2.down*.12f;Vector2 aim=((Vector2)target.transform.position+Vector2.down*.12f-from).normalized;
        CombatBullet.Fire(from,aim,CombatTeam.Robot,damage,kind==StreetEnemyKind.Dron?5:7,pixel,material);
    }
    void Die(){EnemyExplosion.Spawn(transform.position,pixel,material);visual.enabled=false;warning.enabled=false;hitbox.enabled=false;charging=false;burst=0;}
    public void ResetEnemy(){if(!hitbox)hitbox=GetComponent<Collider2D>();transform.position=origin;health.ResetHealth();visual.enabled=true;hitbox.enabled=true;warning.enabled=false;nextAttack=Time.time+1.2f;charging=false;burst=0;}
    void OnDestroy(){if(health)health.Died-=Die;}
}
}
