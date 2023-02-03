using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using System;

public class WinHandler : MonoBehaviour
{
    public float wintime;
    CinemachineVirtualCamera vcam;

    private void Awake()
    {
        vcam = GetComponentInChildren<CinemachineVirtualCamera>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out Player player))
        {
            player.enabled = false;
            
            var anim = player.GetComponentInChildren<Animator>();
            
            Visuals.instance.WinState();
            vcam.Priority = 100;

            anim.SetTrigger("win");

            GameObject.Find("Bloom").GetComponent<Animator>().enabled = true;

            IEnumerator Wait() {
                yield return new WaitForSeconds(wintime);
                AnimationEvents.instance.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex + 1);
            }
            StartCoroutine(Wait());
        }
    }
}
