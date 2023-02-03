using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;

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

    public void PlayNamedParticles(string name) {
        var p = GetComponentsInChildren<ParticleSystem>().FirstOrDefault(x => x.name == name);
        if (p != null) {
            p.Play();
        }
    }

    public void DestroySelf() {
        Destroy(gameObject);
    }

    public void StartParticles(string name) {
        var p = GetComponentsInChildren<ParticleSystem>().FirstOrDefault(x => x.name == name);
        if (p != null) {
            p.Play();
        }
    }

    public void ShakeScreen() {
        AnimationEvents.ShakeScreenEffect();
    }

    public void StopParticles(string name) {
        var p = GetComponentsInChildren<ParticleSystem>().FirstOrDefault(x => x.name == name);
        if (p != null) {
            p.Stop();
        }
    }

    public static void ShakeScreenEffect() {
        var cam = GameObject.Find("GameCam");
        var ccam = cam.GetComponent<Cinemachine.CinemachineVirtualCamera>();
        ccam.GetCinemachineComponent<Cinemachine.CinemachineBasicMultiChannelPerlin>().m_AmplitudeGain = 1;
        ccam.StartCoroutine(AnimationEvents.SetTimeout(0.5f, () => {
            ccam.GetCinemachineComponent<Cinemachine.CinemachineBasicMultiChannelPerlin>().m_AmplitudeGain = 0;
        }));
    }

    public static void ReloadScene() {
        GameObject.Find("Darkening").GetComponent<Image>().StartCoroutine(Fade());
    }

    static IEnumerator SetTimeout(float time, System.Action callback) {
        yield return new WaitForSeconds(time);
        callback();
    }

    static IEnumerator Fade() {
        var darkening = GameObject.Find("Darkening").GetComponent<Image>();
        var color = darkening.color;
        while (color.a < 1) {
            color.a += Time.deltaTime * .5f;
            darkening.color = color;
            yield return null;
        }
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }

    public void LoadScene(int scene) {
        if(scene == -1) 
            scene = PlayerPrefs.GetInt("LastScene", 2);
        StartCoroutine(FadeTo(scene));
        if(scene > 2) {
            PlayerPrefs.SetInt("LastScene", scene);
        }
    }
    static IEnumerator FadeTo(int scene) {
        var darkening = GameObject.Find("Darkening").GetComponent<Image>();
        var color = darkening.color;
        while (color.a < 1) {
            color.a += Time.deltaTime * .5f;
            darkening.color = color;
            yield return null;
        }
        UnityEngine.SceneManagement.SceneManager.LoadScene(scene);
    }

    IEnumerator FadeQuit() {
        var darkening = GameObject.Find("Darkening").GetComponent<Image>();
        var color = darkening.color;
        while (color.a < 1) {
            color.a += Time.deltaTime * .5f;
            darkening.color = color;
            yield return null;
        }
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }

    public void StartMyCoroutine(string name) {
        StartCoroutine(name);
    }

    private void OnDestroy() {
        PlayerPrefs.Save();
    }
}
