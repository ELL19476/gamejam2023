using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AgeStats", menuName = "AgeStats", order = 0)]
public class AgeStats : ScriptableObject
{
    [Tooltip("0 = Baby, 1 = Teen, 2 = Old")]
    public CapsuleCollider[] colliders;
    [Tooltip("0 = Baby, 1 = Teen, 2 = Old")]
    public float[] speeds;


}
