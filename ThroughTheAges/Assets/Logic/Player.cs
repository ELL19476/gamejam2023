using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Player : Mover
{

    [Header("Player Movement")]
    [SerializeField, Range(0, 3)]
    protected float ballAcceleration = 1f;
    [SerializeField, Range(0, 100)]
    protected float ballDrag = 1f;
    [SerializeField, Range(0, 100)]
    protected float maxBallSpeed = 1f;
    [SerializeField]
    protected float ballMass = 10;

    [Header("Juice")]
    [Range(0, 1)]
    public float slowJumpFactor = 0.5f;
    [Range(0, 7)]
    public float fastFallFactor = 4f;


    [Header("Helpers")]
    [SerializeField]
    protected float coyoteTime = 0.1f;
    [SerializeField]
    protected float bufferTime = 0.2f;

    public enum PlayerState {
        Baby,
        Teen,
        Old
    }
    [HideInInspector]
    public PlayerState state {
        get {
            return _state;
        }
        set {
            if(value == _state)
                return;

            _state = value;
            
            ChangeState();
        }
    }
    PlayerState _state;

    // JUMP
    float originalGravity = 1f;
    bool fastFalling = false;
    bool slowJump = false;
    bool afterApex;
    bool isJumping = false;


    float lastGroundedTime = 0f;
    KeyCode inputBuffer; 
    float lastInputBufferTime = 0.2f;

    // ROLL
    bool _ball = false;
    bool ballEnabled {
        get {
            return _ball;
        }
        set {
            if(value == _ball)
                return;
            _ball = value;
            EnableBall(value);
        }
    }
    protected SphereCollider ballCollider;
    float normalMass = 1f;
    [SerializeField]
    protected float rollCooldown;
    float lastRollTime = 0;

    // ATTACK
    [Header("Attack")]
    [SerializeField]
    protected Vector3 localAttackForce;
    [SerializeField]
    protected int attackDamage = 1;
    [SerializeField]
    protected float attackTime = 0.2f;
    bool attack = false;
    BoxCollider attackCollider;

    // Keep track of view direction
    int dir = 1;

    // AgeStats
    public AgeStats ageStats;

    protected override void Start() {
        base.Start();
        ballCollider = GetComponent<SphereCollider>();
        ballEnabled = false;
        normalMass = rigidBody.mass;

        attackCollider = GetComponent<BoxCollider>();
        attackCollider.enabled = false;

        originalGravity = gravityScale;

        ChangeState();
        onLand += () => {
            if(!isJumping)
                Visuals.instance.Land();
        };
        // TMP
        // IEnumerator a() {
        //     yield return new WaitForSeconds(3);
        //     EnableRagdoll(true);
        // }
        // StartCoroutine(a());
    }

    private void Update() {
        if(inputBuffer != KeyCode.None && Time.time - lastInputBufferTime > bufferTime) {
            inputBuffer = KeyCode.None;
        }
        if(IsGrounded(out _)) {
            lastGroundedTime = Time.time;
        }
        DoActions(inputBuffer);
    }

    protected void DoActions(KeyCode bufferedInput) {
        // Change state
        if(Input.GetKeyDown(KeyCode.LeftShift) || bufferedInput == KeyCode.LeftShift) {
            state = (PlayerState)(((int)state + 1) % 3);
            if(bufferedInput == KeyCode.LeftShift) {
                inputBuffer = KeyCode.None;
            }
        }

        float h = Input.GetAxisRaw("Horizontal");
        if(!ballEnabled && h != 0) {
            dir = h > 0 ? 1 : -1;
        }
        if(!ballEnabled) {
            if(MoveTowards(transform.position + Vector3.right * dir)) {
                Visuals.instance.SetSpeed(Mathf.Abs(h));
            } else {
                Visuals.instance.SetSpeed(0);
            }
        }

        if(isJumping && !afterApex && Vector3.Dot(accumulatedVel, Vector3.up) < 0) {
            afterApex = true;
            Visuals.instance.EndSpecial();
        }
        if(isJumping && afterApex && IsGrounded(out _)) {
            afterApex = false;
            isJumping = false;
            Visuals.instance.Land();
        }

        SpecialAction(bufferedInput);
    }
    protected void SpecialAction(KeyCode bufferedInput) {
        if(state == PlayerState.Teen) {
            // Jump
            if(Input.GetKeyDown(KeyCode.Space) || bufferedInput == KeyCode.Space) {
                Jump();
                if(bufferedInput == KeyCode.Space) {
                    inputBuffer = KeyCode.None;
                }
                if(Input.GetKey(KeyCode.Space)) {
                    gravityScale *= slowJumpFactor;
                    slowJump = true;
                }
            } else if(slowJump && (Input.GetKeyUp(KeyCode.Space) || afterApex)) {
                gravityScale = originalGravity;
                slowJump = false;
            }
            // Slam down
            if(!fastFalling && Input.GetAxisRaw("Vertical") < 0 && afterApex) {
                gravityScale *= fastFallFactor;
                fastFalling = true;
            } else if(fastFalling && Input.GetAxisRaw("Vertical") >= 0) {
                gravityScale = originalGravity;
                fastFalling = false;
            }
        } else if(state == PlayerState.Baby) {
            // Roll
            if(Input.GetKeyDown(KeyCode.Space) || bufferedInput == KeyCode.Space) {
                if(Time.time - lastRollTime > rollCooldown)
                    ballEnabled = true;
                else {
                    inputBuffer = KeyCode.Space;
                    lastInputBufferTime = Time.time;
                }
            }
            if(ballEnabled && Input.GetKeyUp(KeyCode.Space)) {
                ballEnabled = false;
            }
        } else if(state == PlayerState.Old) {
            if(Input.GetKeyDown(KeyCode.Space) || bufferedInput == KeyCode.Space)
                Attack();
        }
    }

    protected virtual void EnableBall(bool enable) {
        rigidBody.freezeRotation = !enable;
        capsule.enabled = !enable;
        ballCollider.enabled = enable;
        if(!enable) {
            Visuals.instance.EndSpecial();
            rigidBody.mass = normalMass;
            if(IsGrounded(out _)) {
                rigidBody.MoveRotation(Quaternion.LookRotation(Vector3.up));
                accumulatedVel = Vector3.up * jumpSpeed;
            }

            lastRollTime = Time.time;
        }
        else {
            Visuals.instance.Special();
            rigidBody.mass = ballMass;
            if(IsGrounded(out Vector3 dist)) {
                Vector3 tangent = Vector3.Cross(dist.normalized, Vector3.forward);
                lastVelocity = tangent * dir * speed * ballAcceleration;
            } else {
                lastVelocity = Vector3.right * dir * speed * ballAcceleration * airControl;
            }
        }
    }

    void FixedUpdate ()
    {
        if(!ballEnabled) return;
        Vector3 acceleration = Physics.gravity * gravityScale;
        rigidBody.AddForce(acceleration, ForceMode.Acceleration);

        lastVelocity -= lastVelocity * Mathf.Clamp01(ballDrag * Time.fixedDeltaTime);
        // additional acceleration
        rigidBody.velocity += lastVelocity;
        // clamp speed
        rigidBody.velocity = Vector3.ClampMagnitude(rigidBody.velocity, maxBallSpeed);

    }

    protected override void Jump() {
        if(Time.time - lastGroundedTime < coyoteTime) {
            isJumping = true;
            Visuals.instance.Special();
            base.Jump();
        } else {
            // Buffer jump
            inputBuffer = KeyCode.Space;
            lastInputBufferTime = Time.time;
        }
    }
    protected virtual void Attack() {
        // Attack
        // 1. stop walking
        Visuals.instance.Special();
        attack = true;
        // 2. Hitbox Query
        Collider[] cols = Physics.OverlapBox(transform.TransformPoint(attackCollider.center), attackCollider.size * 0.5f, transform.rotation);
        foreach(Collider col in cols) {
            if(col.gameObject == gameObject || col.transform.root == transform)
                continue;
            Rigidbody rb = col.GetComponent<Rigidbody>();
            IDamagable health = col.GetComponent<IDamagable>();
            BreakableWall wall = col.GetComponent<BreakableWall>();

            IEnumerator Knockback() {
                yield return new WaitForSeconds(attackTime * 0.2f);
                // 3. Damage
                if(health != null) {
                    health.Health -= attackDamage;
                }
                // 4. Wall Knockback
                if(wall != null) {
                    wall.Break(transform.TransformPoint(capsule.center), localAttackForce.magnitude);
                }
                // 4. Knockback
                var ragdollCols = Physics.OverlapBox(transform.TransformPoint(attackCollider.center), attackCollider.size * 0.5f, transform.rotation);
                List<Rigidbody> ragdollRbs = new List<Rigidbody>();
                foreach(Collider rc in ragdollCols) {
                    if(rc.transform.root == col.transform && rc.TryGetComponent(out Rigidbody rbr)) {
                        ragdollRbs.Add(rbr);
                    }
                }
                // Add force to ragdoll
                foreach(Rigidbody rbr in ragdollRbs) {
                    rbr.AddForce(transform.TransformDirection(localAttackForce), ForceMode.Impulse);
                }
            }
            StartCoroutine(Knockback());
        }
        IEnumerator AttackEnd() {
            yield return new WaitForSeconds(attackTime);
            attack = false;
        }
        StartCoroutine(AttackEnd());
    }

    protected void ChangeState() {
        if((isJumping && !afterApex) || attack || ballEnabled)
            Visuals.instance.EndSpecial();

        gravityScale = originalGravity;
        fastFalling = false;
        slowJump = false;
        ballEnabled = false;
        inputBuffer = KeyCode.None; 

        // Change speed
        speed = ageStats.speeds[(int)_state];
        // Change collider
        {
            capsule.height = ageStats.colliders[(int)_state].height;
            capsule.radius = ageStats.colliders[(int)_state].radius;
            capsule.center = ageStats.colliders[(int)_state].center;
            capsule.direction = ageStats.colliders[(int)_state].direction;
        }
        // Change masses
        rigidBody.mass = ageStats.masses[(int)_state];

        Visuals.instance.age = (Visuals.Age)_state;
    }

    protected override bool CanMove(Vector3 normal)
    {
        return !attack && Input.GetAxisRaw("Horizontal") != 0;
    }
    protected override void EnableRagdoll(bool enable)
    {
        if(ballCollider)
            ballCollider.enabled = !enable;
        
        GetComponentsInChildren<Animator>().ToList().ForEach(a => {
            a.enabled = !enable;
            a.gameObject.SetActive(!enable);
        });
        if(enable)
            Visuals.instance.ActiveAnimator().gameObject.SetActive(true);

        base.EnableRagdoll(enable);
    }
}
