using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Enemy : Mover
{
    public float shootRange = 5f;
    public Vector3 direction;

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
    }

    public void SetTarget(Vector3 target)
    {
        t = target;
        moveToTarget = true;
    }
    private void Update()
    {
        
        MoveTowards(transform.position + direction);
    }

    protected override bool CanMove(Vector3 normal)
    {
        return moveToTarget && base.CanMove(normal);
    }

    IEnumerator Attack()
    {
        // AUDIO: Attack
        
        moveToTarget = false;
        var anim = GetComponentInChildren<Animator>();
        anim.SetTrigger("attack");
        yield return new WaitForSeconds(1f);
        moveToTarget = true;
        var obstacle = Instantiate(obstaclePrefab, obstacleSpawnPoint.transform.position, transform.rotation);
    }

    protected override void EnableRagdoll(bool enable)
    {
        if(enable)
        GetComponentsInChildren<Animator>().ToList().ForEach(a => {
            a.enabled = false;
        });
        if(enable)
            Visuals.instance.ActiveAnimator().gameObject.SetActive(true);

        base.EnableRagdoll(enable);
    }
    public void AttackTrigger() {
        Debug.Log("AttackTrigger");
        StartCoroutine(Attack());
    }
    public void MoveTrigger() {
        moveToTarget = true;
    }
}


public static class VectorExtensions
{
    public static Vector3 IgnoreY(this Vector3 v)
    {
        return new Vector3(v.x, 0, v.z);
    }
    
}