using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BouncePad : MonoBehaviour
{
    [SerializeField] float force = 2f;
    [SerializeField] float minVelocity = 7.5f;
    private void OnCollisionEnter(Collision other) {
        if(other.gameObject.TryGetComponent(out Mover mover)) {
            mover.SetVelocity(transform.up * 
            Mathf.Max(force * (other.relativeVelocity.magnitude) * (other.rigidbody? 1f / other.rigidbody.mass : 1),
                minVelocity
            ));
        }
    }
}
