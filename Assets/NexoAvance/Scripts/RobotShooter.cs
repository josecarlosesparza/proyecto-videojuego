using System.Collections.Generic;
using UnityEngine;
namespace NexoAvance {
public class RobotShooter : MonoBehaviour {
    public CombatHealth health, target;
    public SpriteRenderer visual, warning;
    public Sprite pixel;
    public Material material;
    public int damage = 15;
    public float detectionRange = 7, shotInterval = 1.5f, warningTime = .5f, bulletSpeed = 8;
    public bool AutoFire = true;
    public bool IsCharging { get; private set; }
    float nextShot, chargeEnds, direction;
    Vector3 initialPosition;
    Collider2D body;
    EnemyExplosion deathEffect;
    readonly List<RaycastHit2D> hits = new List<RaycastHit2D>(16);
    void Awake() { initialPosition = transform.position; body = GetComponent<Collider2D>(); health.Died += Die; nextShot = Time.time + .6f; }
    void Update() {
        if(!health.Alive || !target || !target.Alive || Time.timeScale == 0) return;
        direction = target.transform.position.x >= transform.position.x ? 1 : -1;
        visual.flipX = direction < 0;
        bool visible = AutoFire && HasLineOfSight();
        if(!visible) { IsCharging = false; warning.enabled = false; return; }
        if(!IsCharging && Time.time >= nextShot) { IsCharging = true; chargeEnds = Time.time + warningTime; }
        warning.enabled = IsCharging;
        if(IsCharging && Time.time >= chargeEnds) {
            Fire(); IsCharging = false; warning.enabled = false; nextShot = Time.time + shotInterval;
        }
    }
    public bool HasLineOfSight() {
        Vector2 origin = (Vector2)transform.position + new Vector2(0,-.12f);
        Vector2 delta = (Vector2)target.transform.position + new Vector2(0,-.12f) - origin;
        if(Mathf.Abs(delta.y) > .7f || Mathf.Abs(delta.x) > detectionRange) return false;
        var filter = new ContactFilter2D(); filter.SetLayerMask(~0); filter.useTriggers = false;
        Physics2D.Raycast(origin, Vector2.right * Mathf.Sign(delta.x), filter, hits, Mathf.Abs(delta.x));
        foreach(var h in hits) { var entity = h.collider.GetComponentInParent<CombatHealth>(); if(entity == health) continue; return entity == target; }
        return false;
    }
    public bool Fire() {
        if(!health.Alive || !target.Alive || Time.timeScale == 0) return false;
        float side = target.transform.position.x >= transform.position.x ? 1 : -1;
        CombatBullet.Fire((Vector2)transform.position + new Vector2(0,-.12f),side,CombatTeam.Robot,damage,bulletSpeed,pixel,material);
        return true;
    }
    void Die() { deathEffect = EnemyExplosion.Spawn(transform.position, pixel, material); body.enabled = false; visual.enabled = false; warning.enabled = false; IsCharging = false; }
    public void ResetEnemy() {
        if(deathEffect) { Destroy(deathEffect.gameObject); deathEffect = null; }
        transform.position = initialPosition; body.enabled = true; visual.enabled = true;
        health.ResetHealth(); warning.enabled = false; IsCharging = false; nextShot = Time.time + .6f;
    }
    void OnDestroy() { if(health) health.Died -= Die; }
}
}
