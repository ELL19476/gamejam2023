using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class AnimationEvents : MonoBehaviour
{
    public static AnimationEvents instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    public void PlayAllParticles() {
        foreach (var p in GetComponentsInChildren<ParticleSystem>()) {
            p.Play();
        }
    }

    public static void ScreenShake() {
        instance.StartCoroutine(instance.ShakeScreen());
    }

    IEnumerator ShakeScreen() {
        var t = Camera.main.transform;
        
        var duration = 0.2f;
        var magnitude = 0.1f;

        var elapsed = 0.0f;

        var originalPos = t.localPosition;

        while (elapsed < duration) {
            var x = Random.Range(-1f, 1f) * magnitude;
            var y = Random.Range(-1f, 1f) * magnitude;

            t.localPosition = new Vector3(x, y, originalPos.z);

            elapsed += Time.deltaTime;

            yield return null;
        }

        t.localPosition = originalPos;
    }

    public void PlayNamedParticles(string name) {
        var p = GetComponentsInChildren<ParticleSystem>().FirstOrDefault(x => x.name == name);
        if (p != null) {
            p.Play();
        }
    }

    public void DestroySelf() {
        Destroy(gameObject);
    }
}
