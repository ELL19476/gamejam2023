using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class WinHandler : MonoBehaviour
{
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
        }
    }
}
