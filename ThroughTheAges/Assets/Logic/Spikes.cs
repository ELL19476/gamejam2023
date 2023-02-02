using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spikes : MonoBehaviour
{
    private void OnCollisionEnter(Collision other) {
        if(other.gameObject.TryGetComponent<IDamagable>(out var damagable))
        {
            damagable.Health = 0;
        }
    }
}
