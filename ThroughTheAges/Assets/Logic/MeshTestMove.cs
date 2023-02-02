using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(DeformablePlane))]
public class MeshTestMove : MonoBehaviour
{
    public ParticleSystem particles;
    [SerializeField]
    float rad, mag, size, tc, speed;

    DeformablePlane dp;

    Action dynamicMeshMods;
    void Start() {
        // Destroy(this, 2f);
        dp = GetComponent<DeformablePlane>();
    }
    private void FixedUpdate() {
        dynamicMeshMods?.Invoke();
        dp.UpdateMesh();
    }
    private void OnCollisionEnter(Collision other) {
        if(other.relativeVelocity.magnitude < 2f) return;

        IEnumerator coroutine () {
            float startTime = Time.time;
            Vector3 hitPoint = other.GetContact(0).point;
            float relVel = other.relativeVelocity.magnitude;

            if(relVel > 5f) {
                var ps = Instantiate(particles, hitPoint, transform.rotation);
                ParticleSystem.MainModule main = ps.main;
                main.startSpeed = 0.45f * relVel;
            }

            relVel = Mathf.Clamp(relVel - 2, 0f, 10f) / 10f;
            Action ripple = () => {
                float time = Time.time - startTime;
                dp.AddRipple(hitPoint, rad, mag * (relVel), size, speed, tc * time, 1.5f);
            };  
            dynamicMeshMods += ripple;
            yield return new WaitForSeconds(3.5f);
            dynamicMeshMods -= ripple;
        }
        StartCoroutine(coroutine());
    }
}
