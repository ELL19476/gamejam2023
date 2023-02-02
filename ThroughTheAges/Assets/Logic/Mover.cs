using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Mover : MonoBehaviour
{
    protected Rigidbody rigidBody;
    protected CapsuleCollider col;
    protected Rigidbody[] childBodies;
    [Header("SetUp")]  
   [SerializeField] 
    protected Transform groundCheck;
   [SerializeField] 
    protected float gravityScale = 1f;
    [SerializeField]
    protected float stoppingDist = 0.1f;
    [Header("Movement")]  
    [SerializeField] 
    protected float speed = 1f;
    [SerializeField]
    protected float jumpSpeed = 1f;
    [SerializeField, Range(0, 100)]
    protected float airDrag = 0.9f;
    [SerializeField]
    protected float maxAirSpeed;
    [SerializeField, Range(0, 1)]
    protected float airControl;

    protected Vector3 accumulatedVel = Vector3.zero;
    private Vector3 lastVelocity = Vector3.zero;


    protected virtual void Start() {
        rigidBody = GetComponent<Rigidbody>();
        // rigidBody.interpolation = RigidbodyInterpolation.None;
        col = GetComponent<CapsuleCollider>();
        rigidBody.freezeRotation = true;

        List<Rigidbody> rbs = GetComponentsInChildren<Rigidbody>().ToList();
        foreach(Rigidbody rb in rbs) {
            if(rb == rigidBody) {
                rbs.Remove(rb);
                break;
            }
        }
        childBodies = rbs.ToArray();

        EnableRagdoll(false);
    }
    protected void EnableRagdoll(bool enable) {
        col.enabled = !enable;
        foreach (Rigidbody rb in childBodies) {
            rb.isKinematic = !enable;
            rb.detectCollisions = enable;
        }
    }
    protected bool IsGrounded(out RaycastHit hit) {
        float downVelocity = Vector3.Dot(accumulatedVel, Vector3.down);
        if(downVelocity < 0) {
            hit = new RaycastHit();
            return false;
        }
        Vector3 o = Vector3.up * (col.height / 2f - col.radius - 0.1f);
        return Physics.CapsuleCast(transform.TransformPoint(col.center) + o, transform.TransformPoint(col.center) - o, col.radius, Vector3.down, out hit, downVelocity * Time.deltaTime + 0.1f, 1 << 6);
    }
    protected bool CanMove(Vector3 normal) {
        return Physics.Raycast(groundCheck.position, -normal, out _, 1f, 1 << 6);
    }
    protected virtual bool MoveTowards(Vector3 target)
    {
        Vector3 velocity = Vector3.zero;
        bool canMove = true;
        if(IsGrounded(out RaycastHit hit)) {
            // Get the direction to the target
            Vector3 direction = Vector3.Cross(hit.normal, Vector3.forward);
            float dot = Vector3.Dot(target - transform.position, direction);
            if(Mathf.Abs(dot) > stoppingDist) {   
                direction *= Mathf.Sign(dot); 
                if(CanMove(hit.normal))
                    // Move towards the target
                    velocity = direction * speed;
                else {
                    canMove = false;
                }

                // Rotate towards the target
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), 3.5f * speed * Time.deltaTime);
            }
            lastVelocity = velocity;
            velocity += accumulatedVel;
            accumulatedVel = Vector3.zero;
        } else {
            // Momentum
            lastVelocity -= lastVelocity * Mathf.Clamp01(airDrag * Time.deltaTime);
            velocity = lastVelocity;
            velocity = Vector3.ClampMagnitude(velocity, Mathf.Abs(maxAirSpeed));
            velocity += (target - transform.position).normalized * speed * airControl;
            velocity = Vector3.ClampMagnitude(velocity, Mathf.Abs(maxAirSpeed));
            canMove = false;
            // Gravity
            accumulatedVel += Time.deltaTime * gravityScale * Physics.gravity;
            // Rotate towards the target
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(Vector3.right * Mathf.Sign(target.x - transform.position.x)), 3.5f * speed * Time.deltaTime);
        }
        rigidBody.velocity = velocity + accumulatedVel;
        return canMove;
    }

    protected void Jump() {
        if(IsGrounded(out RaycastHit hit)) {
            accumulatedVel += Vector3.up * jumpSpeed;
        }
    }
}
