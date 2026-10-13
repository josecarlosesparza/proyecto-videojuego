using System.Collections.Generic;
using UnityEngine;
namespace NexoAvance {
public class ThrownGrenade:MonoBehaviour {
    public const float Radius=3.2f;
    public const int Damage=180;
    float fuse=1.35f;
    Sprite pixel;Material material;bool exploded;
    public static ThrownGrenade Launch(Vector2 position,float side,PlayerCombat weapon){
        var go=new GameObject("Granada");go.transform.position=position;
        var sr=go.AddComponent<SpriteRenderer>();sr.sprite=weapon.pixel;sr.sharedMaterial=weapon.material;sr.color=new Color(.58f,.67f,.26f);sr.sortingOrder=24;go.transform.localScale=new Vector3(.22f,.3f,1);
        var collider=go.AddComponent<CircleCollider2D>();collider.radius=.5f;
        var body=go.AddComponent<Rigidbody2D>();body.gravityScale=2;body.collisionDetectionMode=CollisionDetectionMode2D.Continuous;body.linearVelocity=new Vector2(side*6.5f,6);body.angularVelocity=-side*400;
        foreach(var c in weapon.GetComponentsInChildren<Collider2D>())Physics2D.IgnoreCollision(collider,c);
        var g=go.AddComponent<ThrownGrenade>();g.pixel=weapon.pixel;g.material=weapon.material;return g;
    }
    void Update(){fuse-=Time.deltaTime;if(fuse<=0)Explode();}
    public void Explode(){
        if(exploded)return;exploded=true;
        var effect=EnemyExplosion.Spawn(transform.position,pixel,material);effect.transform.localScale=Vector3.one*2.8f;
        var victims=new HashSet<CombatHealth>();
        foreach(var c in Physics2D.OverlapCircleAll(transform.position,Radius,~0)){var h=c.GetComponentInParent<CombatHealth>();if(h && h.team==CombatTeam.Robot && h.Alive)victims.Add(h);}
        foreach(var h in victims)h.Hit(Damage,CombatTeam.Player);
        gameObject.SetActive(false);Destroy(gameObject);
    }
    public static void ClearAll(){foreach(var g in FindObjectsByType<ThrownGrenade>(FindObjectsSortMode.None)){g.gameObject.SetActive(false);Destroy(g.gameObject);}}
}
}
