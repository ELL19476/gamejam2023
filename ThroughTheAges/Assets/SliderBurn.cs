using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class SliderBurn : MonoBehaviour
{
    [HideInInspector]
    public float value = 1;
    public Image[] images;
    public Sprite[] icons;
    public Animator sliderAnim;
    public Image iconImage;
    public Transform iconTransform;

    float totalTime;

    float[] marks = new float[3];

    private void Start() {
        transform.localScale = new Vector3(1, 1, 1);
        totalTime = GameManager.player.stateSwitches.Sum(s => s.time);

        float to = 0;
        var states = GameManager.player.stateSwitches;
        for(int i = states.Count - 1; i >= 0; i--) {
            to += states[i].time / totalTime;
            images[(int)states[i].state].transform.localScale = new Vector3(to, 1, 1);
        }
        to = 0;
        for(int i = 0; i < states.Count; i++) {
            marks[i] = 1 - to;
            to += states[i].time / totalTime;
        }

        GameManager.player.onStateSwitch += () => {
            iconImage.sprite = icons[(int)GameManager.player.state];
            sliderAnim.SetTrigger("transition");
            value = marks[(int)GameManager.player.state];
        };

        Visuals.instance.winAction += () => {
            enabled = false;
        };
    }
    void LateUpdate()
    {
        float lastValue = value;
        value -= Time.deltaTime / totalTime;
        value = Mathf.Clamp01(value);
        transform.localScale = new Vector3(value, transform.localScale.y, transform.localScale.z);

        iconImage.transform.position = iconTransform.position;

        for(int i = 0; i < marks.Length; i++) {
            if(marks[i] > value) {
                if((int)GameManager.player.state < i) {
                    GameManager.player.state = (Player.PlayerState)i;
                    Debug.Log("State: " + GameManager.player.state);
                }
            }
        }

        if(value == 0) {
            GameManager.player.Health = 0;
        }
    }
}
