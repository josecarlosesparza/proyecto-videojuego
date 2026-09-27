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
    [Min(1)] public int damage = 10;
    [Min(.05f)] public float shotInterval = .25f;
    public float bulletSpeed = 18;
    public bool TestFire { get; set; }
    float nextShot;
    void Update() {
        bool fire = TestFire;
        if(!movement.TestMode) {
#if ENABLE_INPUT_SYSTEM
            fire = Keyboard.current != null && Keyboard.current.jKey.isPressed;
#elif ENABLE_LEGACY_INPUT_MANAGER
            fire = Input.GetKey(KeyCode.J);
#endif
        }
        if(fire) TryShoot();
    }
    public bool TryShoot() {
        if(!health.Alive || !movement.enabled || movement.Paused || Time.timeScale == 0 || Time.time < nextShot) return false;
        nextShot = Time.time + shotInterval;
        float direction = movement.visual.flipX ? -1 : 1;
        CombatBullet.Fire(GetComponent<Rigidbody2D>().position + new Vector2(0,-.12f),direction,CombatTeam.Player,damage,bulletSpeed,pixel,material);
        return true;
    }
    public void ResetEncounter() { nextShot = 0; TestFire = false; if(session) session.ResetCombat(); }
}
}

