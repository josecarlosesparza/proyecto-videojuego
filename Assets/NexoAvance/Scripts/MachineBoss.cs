using UnityEngine;
namespace NexoAvance {
public class MachineBoss:MonoBehaviour {
    public CombatHealth health,target;public bool core,AutoFire=true;
    public Sprite pixel;public Material material;public SpriteRenderer warning;public Transform[] parts;
    float next,fireAt,burstAt;bool charging;int burst,attack;
    public int Phase => core ? (health.Current>health.maximum*2/3?1:health.Current>health.maximum/3?2:3):1;
    void Awake(){health.Died+=Die;}
    void OnEnable(){next=Time.time+1.5f;}
    void Update(){
        if(!health.Alive || !target.Alive || Time.timeScale==0)return;
        for(int i=0;i<parts.Length;i++)parts[i].localRotation=Quaternion.Euler(0,0,core?Time.time*(i%2==0?20:-20):Mathf.Sin(Time.time*2+i)*12);
        if(!AutoFire){health.Shielded=false;warning.enabled=false;return;}
        if(burst>0 && Time.time>=burstAt){Shoot();burst--;burstAt=Time.time+.26f;}
        if(!charging && burst==0 && Time.time>=next){charging=true;fireAt=Time.time+.85f;warning.enabled=true;health.Shielded=core;}
        if(charging && Time.time>=fireAt){charging=false;warning.enabled=false;health.Shielded=false;burst=core?Phase+1:3;burstAt=Time.time;attack++;next=Time.time+(core?3.7f-Phase*.35f:3.5f);}
    }
    void Shoot(){float side=Mathf.Sign(target.transform.position.x-transform.position.x);Vector2 from=(Vector2)transform.position+new Vector2(side*1.1f,-.4f);Vector2 aim=((Vector2)target.transform.position+Vector2.down*.12f-from).normalized;
        if(attack%2==0){from.y=-.55f;aim=Vector2.right*side;}
        CombatBullet.Fire(from,aim,CombatTeam.Robot,core?14:12,6,pixel,material);
        if(core && Phase>=2 && attack%2!=0){CombatBullet.Fire(from,Quaternion.Euler(0,0,18)*aim,CombatTeam.Robot,12,5,pixel,material);CombatBullet.Fire(from,Quaternion.Euler(0,0,-18)*aim,CombatTeam.Robot,12,5,pixel,material);}
    }
    void Die(){health.Shielded=false;for(int i=0;i<5;i++){var fx=EnemyExplosion.Spawn(transform.position+new Vector3(i%3-1,i/3-.5f,0),pixel,material);fx.transform.localScale=Vector3.one*1.6f;}foreach(var sr in GetComponentsInChildren<SpriteRenderer>())sr.enabled=false;GetComponent<Collider2D>().enabled=false;}
    public void ResetBoss(){health.Shielded=false;health.ResetHealth();foreach(var sr in GetComponentsInChildren<SpriteRenderer>())sr.enabled=true;GetComponent<Collider2D>().enabled=true;warning.enabled=false;charging=false;burst=0;next=Time.time+1.5f;}
    void OnDestroy(){if(health)health.Died-=Die;}
}
}
