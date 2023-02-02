using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CopyPosition : MonoBehaviour
{
    public Transform target;

    private void FixedUpdate()
    {
        if (target != null)
        {
            transform.position = target.position;
        }
    }
}
