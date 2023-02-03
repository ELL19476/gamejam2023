using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Audio : MonoBehaviour
{
    static AudioSource source;
    static bool playedMusic = false;

    private void Awake()
    {
        if(playedMusic) return;
        
        var source = new GameObject("Music").AddComponent<AudioSource>();
        DontDestroyOnLoad(source.gameObject);
        var clip = Resources.Load<AudioClip>("Sounds/GameJam23SL/" + "GameMusik");
        if (clip == null) {
            Debug.Log("Audio clip not found: " + "GameMusik");
            return;
        }
        
        source.loop = true;
        source.clip = clip;
        source.volume = 0.3f;
        source.Play();

        playedMusic = true;
    }
    public static void Play(string name, float pitchRandomness = .2f) {
        var clip = Resources.Load<AudioClip>("Sounds/" + name);
        if (clip == null) {
            Debug.Log("Audio clip not found: " + name);
            return;
        }
        var source = GetSource();
        source.pitch = 1 + Random.Range(-pitchRandomness, pitchRandomness);
        source.PlayOneShot(clip);
    }


    public static void PlayRandom(string name, float pitchRandomness = .2f) {
        name = name + Random.Range(1, 4);
        var clip = Resources.Load<AudioClip>("Sounds/" + name);
        if (clip == null) {
            Debug.Log("Audio clip not found: " + name);
            return;
        }
        var source = GetSource();
        Debug.Log(source);
        source.pitch = 1 + Random.Range(-pitchRandomness, pitchRandomness);
        source.PlayOneShot(clip);
    }

    public static void PlayRandomSilent(string name, float pitchRandomness = .2f) {
        name = name + Random.Range(1, 4);
        var clip = Resources.Load<AudioClip>("Sounds/" + name);
        if (clip == null) {
            Debug.Log("Audio clip not found: " + name);
            return;
        }
        var source = GetSource();
        source.pitch = 1 + Random.Range(-pitchRandomness, pitchRandomness);
        source.PlayOneShot(clip, 2.0f);
    }

    // playsoundloud
    public static void PlayLoud(string name, float pitchRandomness = .2f) {
        var clip = Resources.Load<AudioClip>("Sounds/" + name);
        if (clip == null) {
            Debug.Log("Audio clip not found: " + name);
            return;
        }
        var source = GetSource();
        source.pitch = 1 + Random.Range(-pitchRandomness, pitchRandomness);
        source.PlayOneShot(clip, 2.0f);
    }

    public void Play(string name) {
        Audio.Play(name);
    }

    public void PlayRandom(string name) {
        Audio.PlayRandom(name);
    }

    public void PlayRandomSilent(string name) {
        Audio.PlayRandomSilent(name);
    }

    public void PlayLoud(string name) {
        Audio.PlayLoud(name);
    }

    static AudioSource GetSource() {
        if (source == null) {
            // instantiante empty gameobject
            var go = new GameObject("AudioSource");
            source = go.AddComponent<AudioSource>();
            DontDestroyOnLoad(go);
        }
        return source;
    }
}
