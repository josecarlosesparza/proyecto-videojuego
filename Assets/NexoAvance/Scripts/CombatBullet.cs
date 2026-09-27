using System.Collections.Generic;
using UnityEngine;
namespace NexoAvance {
// Barrido del segmento completo: una bala rápida no atraviesa una pared delgada.
public class CombatBullet : MonoBehaviour {
    public static int ShotsCreated { get; private set; }
    CombatTeam team;
    float speed, direction, expires;
    int damage;
    readonly List<RaycastHit2D> hits = new List<RaycastHit2D>(16);
    public static CombatBullet Fire(Vector2 origin, float direction, CombatTeam team, int damage, float speed, Sprite pixel, Material material) {
        var go = new GameObject(team == CombatTeam.Player ? "Bala del jugador" : "Bala del robot");
        go.transform.position = origin;
        var sr = go.AddComponent<SpriteRenderer>(); sr.sprite = pixel; sr.sharedMaterial = material; sr.sortingOrder = 25;
        sr.color = team == CombatTeam.Player ? new Color(.5f,1,.87f) : new Color(1,.35f,.22f);
        go.transform.localScale = new Vector3(.30f,.10f,1);
        var b = go.AddComponent<CombatBullet>(); b.team = team; b.direction = Mathf.Sign(direction);
        b.damage = damage; b.speed = speed; b.expires = Time.time + 3;
        ShotsCreated++;
        return b;
    }
    void FixedUpdate() {
        if(Time.time >= expires) { Discard(); return; }
        var origin = (Vector2)transform.position;
        float distance = speed * Time.fixedDeltaTime;
        var filter = new ContactFilter2D(); filter.SetLayerMask(~0); filter.useTriggers = false;
        Physics2D.Raycast(origin, Vector2.right * direction, filter, hits, distance);
        foreach(var hit in hits) {
            var target = hit.collider.GetComponentInParent<CombatHealth>();
            if(target && (target.team == team || !target.Alive)) continue;
            if(target) target.Hit(damage,team);
            Discard(); return;
        }
        transform.position = origin + Vector2.right * direction * distance;
    }
    void Discard() { gameObject.SetActive(false); Destroy(gameObject); }
    public static void ClearAll() { foreach(var b in FindObjectsByType<CombatBullet>(FindObjectsSortMode.None)) b.Discard(); }
}
}
