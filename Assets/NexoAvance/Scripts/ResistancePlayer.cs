using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
namespace NexoAvance {
[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
public class ResistancePlayer : MonoBehaviour {
    [Header("Movimiento")]
    public float speed = 6, jumpSpeed = 11, acceleration = 55;
    [Header("Doble salto")] public float doubleJumpSpeed=10.5f;public bool allowDoubleJump=true;
    public bool AirJumpUsed{get;private set;} public float DoubleJumpUntil{get;private set;} public float JumpStartUntil{get;private set;} public float LandUntil{get;private set;}
    public LayerMask groundMask = 1;
    [Header("Animaciones")]
    public Sprite[] idle, run;
    public Sprite jump;
    public SpriteRenderer visual;
    public bool Grounded { get; private set; }
    public bool Paused { get; private set; }
    public bool TestMode { get; set; }
    public float TestAxis { get; set; }
    public bool TestJump { get; set; }
    public Vector2 Velocity => body.linearVelocity;
    Rigidbody2D body;
    BoxCollider2D hitbox;
    Vector3 spawn;
    float axis, jumpBuffer, coyoteTime, animationTime;
    readonly RaycastHit2D[] hits = new RaycastHit2D[8];
    void Awake() {
        body = GetComponent<Rigidbody2D>();
        hitbox = GetComponent<BoxCollider2D>();
        spawn = transform.position;
        var appearance=GetComponent<HeroAppearance>();if(!appearance)appearance=gameObject.AddComponent<HeroAppearance>();appearance.Initialize(this);if(!GetComponent<PlayerActions>())gameObject.AddComponent<PlayerActions>();
    }
    void Update() {
        bool pressJump = false, pause = false, reset = false;
        axis = 0;
        if (TestMode) { axis = TestAxis; pressJump = TestJump; TestJump = false; }
        else {
#if ENABLE_INPUT_SYSTEM
            var k = Keyboard.current;
            if (k != null) {
                axis = ((k.dKey.isPressed || k.rightArrowKey.isPressed) ? 1 : 0)
                    - ((k.aKey.isPressed || k.leftArrowKey.isPressed) ? 1 : 0);
                pressJump = k.spaceKey.wasPressedThisFrame;
                pause = k.escapeKey.wasPressedThisFrame;
                reset = k.rKey.wasPressedThisFrame;
            }
#elif ENABLE_LEGACY_INPUT_MANAGER
            axis = ((Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) ? 1 : 0)
                - ((Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) ? 1 : 0);
            pressJump = Input.GetKeyDown(KeyCode.Space);
            pause = Input.GetKeyDown(KeyCode.Escape);
            reset = Input.GetKeyDown(KeyCode.R);
#endif
        }
        if (pause) SetPaused(!Paused);
        if (Paused) return;
        if (reset || transform.position.y < -7) Respawn();
        if (pressJump) jumpBuffer = .12f;
        jumpBuffer -= Time.deltaTime;
        animationTime += Time.deltaTime;
        if (Mathf.Abs(axis) > .01f) visual.flipX = axis < 0;
        if(GetComponent<HeroAppearance>())return;
        if (!Grounded) visual.sprite = jump;
        else if (Mathf.Abs(body.linearVelocity.x) > .25f && run.Length > 0)
            visual.sprite = run[(int)(animationTime * 12) % run.Length];
        else if (idle.Length > 0) visual.sprite = idle[(int)(animationTime * 3) % idle.Length];
    }
    void FixedUpdate() {
        var filter = new ContactFilter2D();
        filter.SetLayerMask(groundMask);
        filter.useTriggers = false;
        int count = hitbox.Cast(Vector2.down, filter, hits, .07f);
        bool wasGrounded=Grounded;Grounded = false;
        for (int i = 0; i < count; i++) if (hits[i].normal.y > .65f && body.linearVelocity.y<=.1f) Grounded = true;
        if(Grounded){AirJumpUsed=false;if(!wasGrounded)LandUntil=Time.time+.1f;}
        coyoteTime = Grounded ? .10f : coyoteTime - Time.fixedDeltaTime;
        Vector2 v = body.linearVelocity;
        v.x = Mathf.MoveTowards(v.x, axis * speed * (GetComponent<PlayerSlow>()?.Factor ?? 1), acceleration * Time.fixedDeltaTime);
        if (jumpBuffer > 0 && coyoteTime > 0 && v.y <= .1f) {
            if(!Grounded)AirJumpUsed=true;v.y = jumpSpeed; jumpBuffer = 0; coyoteTime = 0; Grounded = false;JumpStartUntil=Time.time+.1f;
        }
        if(jumpBuffer>0&&!Grounded&&coyoteTime<=0&&allowDoubleJump&&!AirJumpUsed){v.y=doubleJumpSpeed;AirJumpUsed=true;jumpBuffer=0;DoubleJumpUntil=Time.time+.2f;ActionPulse.Spawn((Vector2)transform.position+Vector2.down*.8f,GetComponent<PlayerCombat>().material,Color.cyan,.7f,.25f);}
        v.y = Mathf.Max(v.y, -18f);
        body.linearVelocity = v;
    }
    public void SetSpawn(Vector3 point) { spawn=point; }
    public void Respawn() { transform.position = spawn; body.position = spawn; body.linearVelocity = Vector2.zero; jumpBuffer = coyoteTime = 0;AirJumpUsed=false;DoubleJumpUntil=JumpStartUntil=LandUntil=0;GetComponent<PlayerActions>()?.ResetActions();GetComponent<HeroAppearance>()?.ResetVisual(); GetComponent<PlayerCombat>()?.ResetEncounter(); }
    public void SetPaused(bool value) { Paused = value; Time.timeScale = value ? 0 : 1; jumpBuffer = 0; }
    void OnApplicationFocus(bool focus) { if (!focus && !TestMode) SetPaused(true); }
    void OnDisable() { Time.timeScale = 1; }
}
}


