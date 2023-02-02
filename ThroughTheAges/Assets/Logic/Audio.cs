using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Audio : MonoBehaviour
{
    static AudioSource source;
    public static void Play(string name, float pitchRandomness = .2f) {
        var clip = Resources.Load<AudioClip>("Sounds/" + name);
        if (clip == null) {
            Debug.LogWarning("Audio clip not found: " + name);
            return;
        }
        var source = GetSource();
        source.pitch = 1 + Random.Range(-pitchRandomness, pitchRandomness);
        source.PlayOneShot(clip);
    }

    public void Play(string name) {
        Audio.Play(name);
    }

    static AudioSource GetSource() {
        if (source == null) {
            source = FindObjectOfType<MonoBehaviour>().gameObject.AddComponent<AudioSource>();
        }
        return source;
    }
}
