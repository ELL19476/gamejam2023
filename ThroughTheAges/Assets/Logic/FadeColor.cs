using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FadeColor : MonoBehaviour
{
    private void Start()
    {
        StartCoroutine(Fade());
    }

    IEnumerator Fade() {
        var image = GetComponent<Image>();

        var t = 0f;
        var dur = .3f;
        while (t < dur) {
            t += Time.deltaTime;
            image.color = Color.Lerp(Color.black, Color.clear, t / dur);
            yield return null;
        }
    }
}
