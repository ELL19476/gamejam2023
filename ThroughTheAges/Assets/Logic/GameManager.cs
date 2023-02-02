using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static Player player;

    private void Awake()
    {
        player = FindObjectOfType<Player>();
    }

}
