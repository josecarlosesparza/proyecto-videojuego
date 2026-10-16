using UnityEngine;
namespace NexoAvance {
public enum RobotAttack { Single, DualBurst, Minigun, Grenade, Flame, Electric }
[CreateAssetMenu(menuName="NEXO/Tipo de robot")]
public class RobotProfile:ScriptableObject {
 public string displayName;public StreetEnemyKind kind;public Sprite pose;
 public Sprite[] idle,run,jump,shoot,hurt,death,prepare,recover;public float animationFps=10;
 public SpriteMuzzle[] spriteMuzzles;
 public RobotAttack attack;public int health=90,damage=8;
 public float speed=2.8f,stopDistance=4.5f,range=12,warning=.65f,cooldown=2.5f,projectileSpeed=7;
 public int burstCount=1;public float burstInterval=.2f;
 public Vector2 muzzleA,muzzleB;
 public float blastRadius=2.2f,fuse=.7f,flightTime=1.3f,fireDuration=1.5f,fireTick=.5f,slowFactor=.8f,slowDuration=1;
 public bool flying,heavy;public float hoverHeight=.75f;
}
}
