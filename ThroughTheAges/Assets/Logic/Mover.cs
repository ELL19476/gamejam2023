using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Mover : MonoBehaviour
{
    protected Rigidbody rigidBody;
    protected CapsuleCollider capsule;
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
    protected Vector3 lastVelocity = Vector3.zero;

    bool grounded = false;
    Vector3 lastColDir = Vector3.up;

    protected virtual void Start() {
        rigidBody = GetComponent<Rigidbody>();
        // rigidBody.interpolation = RigidbodyInterpolation.None;
        capsule = GetComponent<CapsuleCollider>();
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
        capsule.enabled = !enable;
        foreach (Rigidbody rb in childBodies) {
            rb.isKinematic = !enable;
            rb.detectCollisions = enable;
        }
    }
    protected bool IsGrounded(out Vector3 dist) {
        dist = Vector3.zero;
        if(Vector3.Dot(accumulatedVel, Vector3.down) < 0)
            return false;

        Vector3 lowerCapsuleSphereCenter = transform.TransformPoint(capsule.center) - transform.up * (capsule.height / 2f - capsule.radius);
        Vector3 groundPos = lowerCapsuleSphereCenter - transform.up * (capsule.radius - 0.05f);
        Collider[] cols = Physics.OverlapSphere(lowerCapsuleSphereCenter, capsule.radius + 0.05f, 1 << 6);
        if(cols.Length > 0) {
            int i = 0;
            if(cols.Length > 1) {
                Physics.Raycast(groundCheck.position, -transform.up, out RaycastHit grnd, 1f, 1 << 6);
                for(int j = 0; j < cols.Length; j++) {
                    if(cols[j].gameObject == grnd.collider.gameObject) {
                        i = j;
                        break;
                    }
                }
            }
            Vector3 p = cols[i].ClosestPoint(groundPos);
            dist = (groundPos - p);
            lastColDir = dist;
            return true;
        }

        return false;
    }
    protected virtual bool CanMove(Vector3 normal) {
        return Physics.Raycast(groundCheck.position, -normal, out _, 1f, 1 << 6);
    }
    protected virtual bool MoveTowards(Vector3 target, bool slide = false)
    {
        Vector3 velocity = Vector3.zero;
        bool canMove = true;
        if(IsGrounded(out Vector3 dist)) {
            Vector3 normal = dist.normalized;
            // Get the direction to the target
            Vector3 direction = Vector3.Cross(normal, Vector3.forward);
            float dot = Vector3.Dot(target - transform.position, direction);
            direction *= Mathf.Sign(dot); 
            if(Mathf.Abs(dot) > stoppingDist) {   
                if(CanMove(normal))
                    // Move towards the target
                    velocity = direction * speed;
                else {
                    canMove = false;
                }

            }
            // Rotate towards the target
            rigidBody.MoveRotation(Quaternion.Slerp(rigidBody.rotation, Quaternion.LookRotation(direction), 3.5f * speed * Time.deltaTime));
            
            lastVelocity = velocity;
            if(!grounded) {
                // Move to ground
                grounded = true;
            }
            if(slide) {
                // Slide down slopes
                AddGravity();
                accumulatedVel = Vector3.ProjectOnPlane(accumulatedVel, normal);
            } else {
                // Stop Vertical Velocity
                accumulatedVel = Vector3.zero;
            }
        } else {
            grounded = false;
            // Momentum
            lastVelocity -= lastVelocity * Mathf.Clamp01(airDrag * Time.deltaTime);
            velocity = lastVelocity;
            velocity = Vector3.ClampMagnitude(velocity, Mathf.Abs(maxAirSpeed));
            if(CanMove(Vector3.up)) 
                velocity += (target - transform.position).normalized * speed * airControl;

            velocity = Vector3.ClampMagnitude(velocity, Mathf.Abs(maxAirSpeed));
            AddGravity();
            canMove = false;
            // Rotate towards the target
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(Vector3.right * Mathf.Sign(target.x - transform.position.x)), 3.5f * speed * Time.deltaTime);
        }
        rigidBody.velocity = velocity + accumulatedVel;
        return canMove;
    }

    protected void AddGravity() {
        // Gravity
        accumulatedVel += Time.deltaTime * gravityScale * Physics.gravity;
    }

    protected virtual void Jump() {
        accumulatedVel.y = 0;
        accumulatedVel += Vector3.up * jumpSpeed;
    }
}
