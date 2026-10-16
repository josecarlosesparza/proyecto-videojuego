using UnityEngine;
namespace NexoAvance {
public class OctopusBoss:MonoBehaviour {
 public CombatHealth health,target;public Sprite pixel;public Material material;public Transform[] arms;public SpriteRenderer warning;
 public bool AutoFire=true;public EncounterDirector encounters;public BossCore core;public SpriteRenderer art;
 [Header("Ataques y fases")]
 public int cannonDamage=10,sweepDamage=20,missileDamage=25;
 public float cannonSpeed=7,attackInterval=3.6f,warningDuration=1.1f,missileWarning=1.5f,sweepWidth=6,missileRadius=2.2f;
 public Vector2 cannonA=new Vector2(-2,3),cannonB=new Vector2(4.4f,2.8f);
 public float supportInterval=14;public int supportCount=2;
 public int Phase {get;private set;}=1;public int PhaseTransitions {get;private set;}
 public int PatternsExecuted{get;private set;}
 public int MissilesLaunched{get;private set;}
 SpriteRenderer secondWarning;
 void LateUpdate(){if(!secondWarning){secondWarning=new GameObject("Aviso cañón derecho").AddComponent<SpriteRenderer>();secondWarning.transform.SetParent(transform);secondWarning.sprite=warning.sprite;secondWarning.sharedMaterial=warning.sharedMaterial;secondWarning.sortingOrder=26;secondWarning.transform.localScale=Vector3.one*.35f;}secondWarning.enabled=health.Alive&&warning.enabled&&pattern==0;secondWarning.color=warning.color;secondWarning.transform.position=(Vector2)transform.position+cannonB;}
 float next,ready,burstAt,nextSupport;int shots,pattern,sequence;bool charging;Collider2D hitbox;Vector2 marked;
 public int CurrentPattern=>pattern;float impactAt,attackEndAt;public bool Preparing=>charging||Time.time<impactAt;public bool Firing=>Time.time>=impactAt&&Time.time<attackEndAt;
 void Awake(){hitbox=GetComponent<Collider2D>();health.Died+=Die;}
 void OnEnable(){next=Time.time+2;nextSupport=Time.time+supportInterval;}
 void Update(){
  if(!health.Alive||!target||!target.Alive||Time.timeScale==0){warning.enabled=false;return;}
  int wanted=health.Current>=health.maximum*.65f?1:health.Current>=health.maximum*.3f?2:3;
  if(wanted>Phase){PhaseTransitions+=wanted-Phase;Phase=wanted;nextSupport=Time.time+2;var mission=encounters?encounters.session.GetComponent<StreetMission>():null;if(mission)mission.ShowNotice(Phase==2?"KRAKEN · MISILES Y REFUERZOS":"KRAKEN · NÚCLEO VULNERABLE",2);}
  if(core)core.GetComponent<Collider2D>().enabled=Phase==3;
  if(!AutoFire){warning.enabled=false;return;}
  if(Phase>=2&&Time.time>=nextSupport){encounters?.QueueSupport(supportCount);nextSupport=Time.time+supportInterval;}
  if(shots>0&&Time.time>=burstAt){FireCannon();shots--;burstAt=Time.time+.3f;if(shots==0)next=Time.time+Pause;}
  if(!charging&&shots==0&&Time.time>=next){pattern=sequence++%(Phase==1?2:4);charging=true;ready=Time.time+warningDuration;marked=target.transform.position;warning.enabled=true;warning.color=pattern==0?Color.yellow:pattern==1?new Color(1,.45f,.1f):Color.red;warning.transform.position=pattern==0?(Vector2)transform.position+cannonA:marked+Vector2.up*1.4f;}
  if(charging&&Time.time>=ready){charging=false;warning.enabled=false;PatternsExecuted++;impactAt=Time.time+((pattern==1||pattern==3)?warningDuration:pattern==2?missileWarning+.5f:0);attackEndAt=impactAt+.65f;if(pattern==0){shots=3;burstAt=Time.time;}else if(pattern==1){HostileHazard.Sweep(new Vector2(Mathf.Clamp(marked.x,transform.position.x-13,transform.position.x+9),marked.y),sweepDamage,sweepWidth,warningDuration,this);next=Time.time+warningDuration+Pause;}else if(pattern==3){HostileHazard.GroundStrike(marked,sweepDamage,1.8f,warningDuration,this);next=Time.time+warningDuration+Pause;}else{MissilesLaunched++;HostileHazard.Missile(marked,missileDamage,missileRadius,missileWarning,this);next=Time.time+missileWarning+.5f+Pause;}}
 }
 float Pause=>attackInterval*(Phase==3?.8f:1);
 void FireCannon(){var from=(Vector2)transform.position+(shots%2==0?cannonB:cannonA);var aim=((Vector2)target.transform.position+Vector2.down*.15f-from).normalized;CombatBullet.Fire(from,aim,CombatTeam.Robot,cannonDamage,cannonSpeed,pixel,material);}
 void Die(){charging=false;shots=0;HostileHazard.ClearAll();CombatBullet.ClearAll();encounters?.FinishCombat();for(int i=0;i<7;i++){var e=EnemyExplosion.Spawn(transform.position+new Vector3(i%3-1,i/3,0),pixel,material);e.transform.localScale=Vector3.one*2;}foreach(var sr in GetComponentsInChildren<SpriteRenderer>())sr.enabled=false;foreach(var c in GetComponentsInChildren<Collider2D>())c.enabled=false;}
 public void ResetBoss(){if(!hitbox)hitbox=GetComponent<Collider2D>();health.ResetHealth();hitbox.enabled=true;if(art)art.enabled=true;else foreach(var sr in GetComponentsInChildren<SpriteRenderer>())sr.enabled=true;warning.enabled=false;if(core)core.GetComponent<Collider2D>().enabled=false;next=Time.time+2;nextSupport=Time.time+supportInterval;charging=false;shots=sequence=0;Phase=1;PhaseTransitions=PatternsExecuted=MissilesLaunched=0;impactAt=attackEndAt=0;HostileHazard.ClearAll();}
 void OnDestroy(){if(health)health.Died-=Die;}
}
}
