using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Enemy : Mover
{
    Vector3 t = Vector3.zero;
    bool moveToTarget = false;


    protected override void Start() {
        base.Start();
    }

    public void SetTarget(Vector3 target) {
        t = target;
        moveToTarget = true;
    }
    private void Update() {
        MoveTowards(t);
    }

    protected override bool CanMove(Vector3 normal)
    {
        return moveToTarget && base.CanMove(normal);
    }
}
