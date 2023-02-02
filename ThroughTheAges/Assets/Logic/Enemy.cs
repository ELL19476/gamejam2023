using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Enemy : Mover
{
    Vector3 t = Vector3.zero;
    bool moveToTarget = false;

    public float range = 5f;


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
        SetTarget(GameManager.player.transform.position);
        
        MoveTowards(t);

        moveToTarget = Vector3.Distance(transform.position, t) > range;

        moveToTarget = true;
    }

    protected override bool CanMove(Vector3 normal)
    {
        return moveToTarget && base.CanMove(normal);
    }
}
