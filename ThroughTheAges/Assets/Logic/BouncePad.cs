using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BouncePad : MonoBehaviour
{
    [SerializeField] float force = 2f;
    [SerializeField] float minVelocity = 7.5f;
    private void OnCollisionEnter(Collision other) {
        Audio.PlayRandom("GameJam23SL/BouncePad/BoingBncpad");
        
        Vector3 f = transform.up * 
            Mathf.Max(force * (other.relativeVelocity.magnitude) * (other.rigidbody? 1f / other.rigidbody.mass : 1),
                minVelocity
            );
        if(other.gameObject.TryGetComponent(out Mover mover)) {
            mover.SetVelocity(f);
        } else if(other.rigidbody){
            other.rigidbody.velocity = Vector3.zero;
            other.rigidbody.AddForce(f, ForceMode.VelocityChange);
        }
    }
}
