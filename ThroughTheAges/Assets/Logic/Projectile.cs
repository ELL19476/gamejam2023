using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    float speed = 4f;

    bool flying = false;

    private void Start()
    {
        StartCoroutine(SpawnProjectile());

    }
    private void Update()
    {
        if (flying)
            transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }


    IEnumerator SpawnProjectile()
    {
        yield return new WaitForSeconds(1f);
        flying = true;
    }
}
