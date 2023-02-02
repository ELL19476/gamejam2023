using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Player : Mover
{
    float originalGravity = 1f;
    bool fastFalling = false;
    bool slowJump = false;

    [Header("Juice")]
    [Range(0, 1)]
    public float slowJumpFactor = 0.5f;
    [Range(0, 7)]
    public float fastFallFactor = 4f;

    protected override void Start() {
        base.Start();
        originalGravity = gravityScale;
    }

    private void Update() {
        MoveTowards(transform.position + Vector3.right * Input.GetAxisRaw("Horizontal"));
        if(Input.GetKeyDown(KeyCode.Space)) {
            Jump();
            gravityScale *= slowJumpFactor;
            slowJump = true;
        } else if(slowJump && (Input.GetKeyUp(KeyCode.Space) || Vector3.Dot(accumulatedVel, Vector3.up) < 0)) {
            gravityScale = originalGravity;
            slowJump = false;
        }

        if(!fastFalling && Input.GetAxisRaw("Vertical") < 0 && Vector3.Dot(accumulatedVel, Vector3.up) < 0) {
            gravityScale *= fastFallFactor;
            fastFalling = true;
        } else if(fastFalling && Input.GetAxisRaw("Vertical") >= 0) {
            gravityScale = originalGravity;
            fastFalling = false;
        }

    }
}
