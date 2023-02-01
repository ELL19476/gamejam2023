using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Player : Mover
{
    float originalGravity = 1f;
    bool fastFalling = false;
    bool slowJump = false;

    float lastGroundedTime = 0f;

    // JUMP BUFFERING
    KeyCode inputBuffer; 
    float lastInputBufferTime = 0.2f;

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
    // [HideInInspector]
    [Header("TMP")]
    public PlayerState state;

    bool _ball = false;
    bool ballEnabled {
        get {
            return _ball;
        }
        set {
            _ball = value;
            EnableBall(value);
        }
    }
    int dir = 1;

    protected SphereCollider ballCollider;
    float normalMass = 1f;

    protected override void Start() {
        base.Start();
        ballCollider = GetComponent<SphereCollider>();
        ballEnabled = false;
        normalMass = rigidBody.mass;

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
        float h = Input.GetAxisRaw("Horizontal");
        if(!ballEnabled && h != 0) {
            dir = h > 0 ? 1 : -1;
        }
        if(!ballEnabled) {
            MoveTowards(transform.position + Vector3.right * dir);
        }

        SpecialAction(bufferedInput);

        if(!fastFalling && Input.GetAxisRaw("Vertical") < 0 && Vector3.Dot(accumulatedVel, Vector3.up) < 0) {
            gravityScale *= fastFallFactor;
            fastFalling = true;
        } else if(fastFalling && Input.GetAxisRaw("Vertical") >= 0) {
            gravityScale = originalGravity;
            fastFalling = false;
        }
    }
    protected void SpecialAction(KeyCode bufferedInput) {
        if(state == PlayerState.Teen) {
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
        } else if(state == PlayerState.Baby) {
            if(Input.GetKeyDown(KeyCode.Space) || bufferedInput == KeyCode.Space) {
                ballEnabled = true;
            } 
            if(ballEnabled && Input.GetKeyUp(KeyCode.Space)) {
                ballEnabled = false;
            }
        } else if(state == PlayerState.Old) {
            // Attack
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
                // TODO
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

    protected override bool CanMove(Vector3 normal)
    {
        return Input.GetAxisRaw("Horizontal") != 0;
    }
}
