using System;
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

    public Action winAction;


    public Animator ActiveAnimator()
    {
        switch (age)
        {
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

            Audio.Play("GameJam23SL/AgeMorph");
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
        foreach (var a in GetComponentsInChildren<Animator>(true))
        {
            a.enabled = false;
        }

        Show(ActiveAnimator());
    }

    public void Land(bool hard)
    {
        Audio.PlayLoud("GameJam23SL/Landing");

        if (hard)
            GameObject.Find("LandHard").GetComponent<ParticleSystem>().Play();
        else
            GameObject.Find("LandSoft").GetComponent<ParticleSystem>().Play();

        foreach (var a in GetComponentsInChildren<Animator>(true))
        {
            a.SetTrigger("land");
        }
    }

    public void SetSpeed(float speed)
    {

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

        foreach (var a in GetComponentsInChildren<Animator>(true))
        {
            a.SetFloat("speed", speed);
            a.SetBool("running", speed > Mathf.Epsilon);
        }
    }

    public void Special()
    {
        foreach (var a in GetComponentsInChildren<Animator>(true))
        {
            switch (age)
            {
                case Age.Baby:
                    a.GetComponent<AnimationEvents>()?.StartParticles("Roll");
                    a.SetTrigger("roll");

                    Audio.Play("GameJam23SL/JumpWhoosh");
                    break;
                case Age.Teenie:
                    a.SetTrigger("jumpStart");

                    Audio.Play("GameJam23SL/JumpWhoosh");
                    break;
                case Age.Granny:
                    a.SetTrigger("attack");

                    Audio.Play("GameJam23SL/PunchHits/RolatorMetallPunch1");

                    break;
            }
        }
    }
    public void EndSpecial()
    {
        foreach (var a in GetComponentsInChildren<Animator>(true))
        {
            switch (age)
            {
                case Age.Baby:
                    a.SetTrigger("endRoll");

                    Audio.PlayLoud("GameJam23SL/JumpWhoosh");
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

    void Hide(Animator anim)
    {
        foreach (Transform child in anim.transform)
        {
            if (child.GetComponent<MaxScale>() != null)
            {
                StartCoroutine(ScaleDown(child));
            }

            if (child.GetComponent<Renderer>() != null)
            {
                StartCoroutine(FadeEmission(child.GetComponent<Renderer>().material));
            }
        }
    }

    void Show(Animator anim)
    {
        foreach (Transform child in anim.transform)
        {
            if (child.GetComponent<MaxScale>() != null)
            {
                StartCoroutine(ScaleUp(child));
            }

            if (child.GetComponent<Renderer>() != null)
            {
                StartCoroutine(UnFadeEmission(child.GetComponent<Renderer>().material));
            }
        }
    }

    IEnumerator FadeEmission(Material mat)
    {
        var from = -0f;
        var to = 10f;
        var dur = .4f;

        var t = 0f;
        while (t < dur)
        {
            t += Time.deltaTime;
            var val = Mathf.Lerp(from, to, t / dur);
            mat.SetColor("_EmissionColor", new Color(val, val, val, 1f));
            yield return null;
        }
    }

    IEnumerator ScaleDown(Transform armature)
    {
        var from = armature.GetComponent<MaxScale>().maxScale;
        var to = 0f;
        var dur = .4f;

        var t = 0f;
        while (t < dur)
        {
            t += Time.deltaTime;
            var val = Mathf.Lerp(from, to, t / dur);
            armature.localScale = new Vector3(val, val, val);
            yield return null;
        }

        armature.GetComponentInParent<Animator>().enabled = false;
    }

    IEnumerator UnFadeEmission(Material mat)
    {
        var from = 10f;
        var to = -0f;
        var dur = .4f;

        var t = 0f;
        while (t < dur)
        {
            t += Time.deltaTime;
            var val = Mathf.Lerp(from, to, t / dur);
            mat.SetColor("_EmissionColor", new Color(val, val, val, 1f));
            yield return null;
        }
    }

    IEnumerator ScaleUp(Transform armature)
    {
        armature.GetComponentInParent<Animator>().enabled = true;

        var to = armature.GetComponent<MaxScale>().maxScale;

        var from = 0f;
        var dur = .4f;

        var t = 0f;
        while (t < dur)
        {
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
        winAction?.Invoke();

        // AUDIO: Win
        Audio.PlayLoud("GameJam23SL/HeavenAscend");
        
    }

    IEnumerator Fly()
    {
        var start = transform.position;
        var height = start + Vector3.up * 10f;

        var t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime * .1f;
            transform.position = Vector3.Lerp(start, height, t);
            yield return null;
        }
    }
}
