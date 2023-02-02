using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Enemy : Mover
{
    public Transform position1, position2;

    Vector3 t = Vector3.zero;
    bool moveToTarget = false;

    public float range = .3f;

    public float attackInterval = 5f;

    public float attackIntervalRandom = 2f;

    public GameObject obstaclePrefab;
    public GameObject obstacleSpawnPoint;

    protected override void Start()
    {
        base.Start();

        SetTarget(position2.position);

        StartCoroutine(Attack());
    }

    public void SetTarget(Vector3 target)
    {
        t = target;
        moveToTarget = true;
    }
    private void Update()
    {
        if (Vector3.Distance(transform.position, position1.position) < range)
        {
            SetTarget(position2.position);
        }
        // if reached position 2, move to position 1
        else if (Vector3.Distance(transform.position, position2.position) < range)
        {
            SetTarget(position1.position);
        }
        
        MoveTowards(t);
    }

    protected override bool CanMove(Vector3 normal)
    {
        return moveToTarget && base.CanMove(normal);
    }

    IEnumerator Attack()
    {
        // AUDIO: Attack
        
        yield return new WaitForSeconds(attackInterval + UnityEngine.Random.Range(-attackIntervalRandom, attackIntervalRandom));
        moveToTarget = false;
        var anim = GetComponentInChildren<Animator>();
        anim.SetTrigger("attack");
        yield return new WaitForSeconds(1f);
        moveToTarget = true;
        var obstacle = Instantiate(obstaclePrefab, obstacleSpawnPoint.transform.position, transform.rotation);
        StartCoroutine(Attack());
        
    }
}
