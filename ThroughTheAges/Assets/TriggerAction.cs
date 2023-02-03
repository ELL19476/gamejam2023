using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TriggerAction : MonoBehaviour
{
    public UnityEvent onTrigger;
    private void OnTriggerEnter(Collider other) {
        if(other.TryGetComponent(out Player p))
        {
            onTrigger.Invoke();
            enabled = false;
        }
    }
}
