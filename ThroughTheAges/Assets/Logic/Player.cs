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
            _state = value;
            gravityScale = originalGravity;
            fastFalling = false;
            slowJump = false;
            ballEnabled = false;
            inputBuffer = KeyCode.None;
            switch(value) {
                case PlayerState.Baby:
                    break;
                case PlayerState.Teen:
                    break;
                case PlayerState.Old:
                    break;
            }
        }
    }
    PlayerState _state;

    // JUMP
    float originalGravity = 1f;
    bool fastFalling = false;
    bool slowJump = false;

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

    protected override void Start() {
        base.Start();
        ballCollider = GetComponent<SphereCollider>();
        ballEnabled = false;
        normalMass = rigidBody.mass;

        attackCollider = GetComponent<BoxCollider>();
        attackCollider.enabled = false;

        originalGravity = gravityScale;
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
            MoveTowards(transform.position + Vector3.right * dir);
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
            } else if(slowJump && (Input.GetKeyUp(KeyCode.Space) || Vector3.Dot(accumulatedVel, Vector3.up) < 0)) {
                gravityScale = originalGravity;
                slowJump = false;
            }
            // Slam down
            if(!fastFalling && Input.GetAxisRaw("Vertical") < 0 && Vector3.Dot(accumulatedVel, Vector3.up) < 0) {
                gravityScale *= fastFallFactor;
                fastFalling = true;
            } else if(fastFalling && Input.GetAxisRaw("Vertical") >= 0) {
                gravityScale = originalGravity;
                fastFalling = false;
            }
        } else if(state == PlayerState.Baby) {
            // Roll
            if(Input.GetKeyDown(KeyCode.Space) || bufferedInput == KeyCode.Space) {
                ballEnabled = true;
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
            rigidBody.mass = normalMass;
            if(IsGrounded(out _)) {
                rigidBody.MoveRotation(Quaternion.LookRotation(Vector3.up));
                accumulatedVel = Vector3.up * jumpSpeed;
            }
        }
        else {
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
        Debug.Log(lastVelocity);
        // additional acceleration
        rigidBody.velocity += lastVelocity;
        // clamp speed
        rigidBody.velocity = Vector3.ClampMagnitude(rigidBody.velocity, maxBallSpeed);

    }

    protected override void Jump() {
        if(Time.time - lastGroundedTime < coyoteTime) {
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
        attack = true;
        // 2. Hitbox Query
        Collider[] cols = Physics.OverlapBox(transform.TransformPoint(attackCollider.center), attackCollider.size * 0.5f, transform.rotation);
        // 3. Add Force and Damage to hit objects
        foreach(Collider col in cols) {
            if(col.gameObject == gameObject || col.transform.parent == transform)
                continue;
            Rigidbody rb = col.GetComponent<Rigidbody>();
            IDamagable health = col.GetComponent<IDamagable>();
            if(health != null) {
                health.Health -= attackDamage;
            }
            IEnumerator Knockback() {
                yield return new WaitForSeconds(attackTime);
                if(rb != null && rb.isKinematic == false) {
                    rb.AddForce(transform.TransformDirection(localAttackForce), ForceMode.Impulse);
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

    protected override bool CanMove(Vector3 normal)
    {
        return !attack && Input.GetAxisRaw("Horizontal") != 0;
    }
    protected override void EnableRagdoll(bool enable)
    {
        if(ballCollider)
            ballCollider.enabled = !enable;
        base.EnableRagdoll(enable);
    }
}
