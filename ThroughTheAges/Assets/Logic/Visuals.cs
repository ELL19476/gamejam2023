using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Visuals : MonoBehaviour
{
    const float BABY_SPEED = 1.5f;
    const float TEENIE_SPEED = 1.5f;
    const float GRANNY_SPEED = 1.5f;

    public enum Age { Baby, Teenie, Granny }
    public static Visuals instance;

    Animator baby;
    Animator teenie;
    Animator granny;

    public Animator ActiveAnimator() {
        switch (age) {
            case Age.Baby:
                return baby;
            case Age.Teenie:
                return teenie;
            case Age.Granny:
                return granny;
            default:
                return null;
        }
    }

    Age _age = Age.Baby;

    public Age age
    {
        get { return _age; }
        set
        {
            if (_age == value) return;

            // AUDIO: Change

            GameObject.Find("Change").GetComponent<ParticleSystem>().Play(true);
            
            Hide(ActiveAnimator());
            
            _age = value;
            
            Show(ActiveAnimator());
        }
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        foreach (var a in GetComponentsInChildren<Animator>(true))
        {
            if (a.gameObject.name == "Baby")
            {
                baby = a;
            }
            else if (a.gameObject.name == "Teenie")
            {
                teenie = a;
            }
            else if (a.gameObject.name == "Granny")
            {
                granny = a;
            }
        }
    }

    private void Start()
    {
            Show(ActiveAnimator());
        
    }

    public void Land(bool hard) {
        // AUDIO: Land
        
        if(hard)
        GameObject.Find("LandHard").GetComponent<ParticleSystem>().Play();
        else
        GameObject.Find("LandSoft").GetComponent<ParticleSystem>().Play();

        foreach (var a in GetComponentsInChildren<Animator>(true)) {
            a.SetTrigger("land");
        }
    }

    public void SetSpeed(float speed) {

        // switch (age) {
        //     case Age.Baby:
        //         speed = BABY_SPEED;
        //         break;
        //     case Age.Teenie:
        //         speed = TEENIE_SPEED;
        //         break;
        //     case Age.Granny:
        //         speed = GRANNY_SPEED;
        //         break;
        // }

        foreach (var a in GetComponentsInChildren<Animator>(true)) {
            a.SetFloat("speed", speed);
            a.SetBool("running", speed > Mathf.Epsilon);
        }
    }

    public void Special() {
        foreach (var a in GetComponentsInChildren<Animator>(true)) {
            switch (age) {
                case Age.Baby:
                    a.GetComponent<AnimationEvents>()?.StartParticles("Roll");
                    a.SetTrigger("roll");

                    // AUDIO: Roll
                    break;
                case Age.Teenie:
                    a.SetTrigger("jumpStart");

                    // AUDIO: Jump
                    break;
                case Age.Granny:
                    a.SetTrigger("attack");

                    // AUDIO: Attack
                    break;
            }
        }
    }
    public void EndSpecial() {
        foreach (var a in GetComponentsInChildren<Animator>(true)) {
            switch (age) {
                case Age.Baby:
                    a.SetTrigger("endRoll");

                    // AUDIO: End Roll
                    break;
                case Age.Teenie:
                    a.SetTrigger("jumpApex");
                    break;
                case Age.Granny:
                    a.SetTrigger("endAttack");
                    break;
            }
        }
    }

    void Hide(Animator anim) {
        foreach (Transform child in anim.transform) {
            if (child.GetComponent<MaxScale>() != null) {
                StartCoroutine(ScaleDown(child));
            }

            if (child.GetComponent<Renderer>() != null) {
                StartCoroutine(FadeEmission(child.GetComponent<Renderer>().material));
            }
        }
    }

    void Show(Animator anim) {
        foreach (Transform child in anim.transform) {
            if (child.GetComponent<MaxScale>() != null) {
                StartCoroutine(ScaleUp(child));
            }

            if (child.GetComponent<Renderer>() != null) {
                StartCoroutine(UnFadeEmission(child.GetComponent<Renderer>().material));
            }
        }
    }

    IEnumerator FadeEmission(Material mat) {
        var from = -0f;
        var to = 10f;
        var dur = .4f;

        var t = 0f;
        while (t < dur) {
            t += Time.deltaTime;
            var val = Mathf.Lerp(from, to, t / dur);
            mat.SetColor("_EmissionColor", new Color(val, val, val, 1f));
            yield return null;
        }
    }

    IEnumerator ScaleDown(Transform armature) {
        var from = armature.GetComponent<MaxScale>().maxScale;
        var to = 0f;
        var dur = .4f;

        var t = 0f;
        while (t < dur) {
            t += Time.deltaTime;
            var val = Mathf.Lerp(from, to, t / dur);
            armature.localScale = new Vector3(val, val, val);
            yield return null;
        }
    }

    IEnumerator UnFadeEmission(Material mat) {
        var from = 10f;
        var to = -0f;
        var dur = .4f;

        var t = 0f;
        while (t < dur) {
            t += Time.deltaTime;
            var val = Mathf.Lerp(from, to, t / dur);
            mat.SetColor("_EmissionColor", new Color(val, val, val, 1f));
            yield return null;
        }
    }

    IEnumerator ScaleUp(Transform armature) {
        var to = armature.GetComponent<MaxScale>().maxScale;
        
        var from = 0f;
        var dur = .4f;

        var t = 0f;
        while (t < dur) {
            t += Time.deltaTime;
            var val = Mathf.Lerp(from, to, t / dur);
            armature.localScale = new Vector3(val, val, val);
            yield return null;
        }
    }

    private void Update()
    {
    }

    public void WinState()
    {
        ActiveAnimator().enabled = false;
        StartCoroutine(Fly());

        // AUDIO: Win
    }

    IEnumerator Fly() {
        var start = transform.position;
        var height = start + Vector3.up * 10f;

        var t = 0f;

        while (t < 1f) {
            t += Time.deltaTime * .1f;
            transform.position = Vector3.Lerp(start, height, t);
            yield return null;
        }
    }
}
