using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MoveDown))]
public class MoveDownCondition : MonoBehaviour
{
    public Enemy[] enemiesToKill; 
    public bool moveOnPlayerEnter = false;
    public bool stopOnPlayerLeave = true;
    private MoveDown moveDown;

    bool playerEntered, enemiesKilled;

    private void Start() {
        moveDown = GetComponent<MoveDown>();
        moveDown.canMove = false;
        int enemiesAlive = enemiesToKill.Length;

        playerEntered = false;
        enemiesKilled = enemiesAlive <= 0;

        foreach(var enemy in enemiesToKill)
        {
            enemy.onDie += () => {
                enemiesAlive--;
                if(enemiesAlive <= 0)
                {
                    enemiesKilled = true;
                }
            };
        }
    }

    private void OnTriggerEnter(Collider other) {
        if(other.gameObject.TryGetComponent<Player>(out var player))
        {
            playerEntered = true;
        }
    }
    private void OnTriggerExit(Collider other) {
        if(!stopOnPlayerLeave) return;
        if(other.gameObject.TryGetComponent<Player>(out var player))
        {
            playerEntered = false;
        }
    }

    private void Update() {
        moveDown.canMove = (playerEntered || !moveOnPlayerEnter) && enemiesKilled;
    }
}
