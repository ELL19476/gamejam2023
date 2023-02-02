using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveDown : MonoBehaviour
{
    public Vector3 direction = Vector3.down;
    public float speed = 1f;
    [HideInInspector]
    public bool canMove = false;
    public bool changeDirectionOnHitGround = false;
    public Transform[] groundChecks;

    private void Update() {
        if(!canMove) return;
        if(IsGrounded()) {
            if(changeDirectionOnHitGround) {
                direction = -direction;
            } else {
                return;
            }
        }

        transform.position += direction * speed * Time.deltaTime;
    }

    bool IsGrounded() {
        foreach(var groundCheck in groundChecks)
        {
            if(Physics.Raycast(groundCheck.position, direction, speed * Time.deltaTime, 1 << 6, QueryTriggerInteraction.Ignore))
                return true;
        }
        return false;
    }
}
