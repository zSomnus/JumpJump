using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public enum MeleeProperty
{
    None,
    Juggling
}

public enum BloodData
{
    None,
    Light,
    Heavy
}

[Serializable]
public class ComboData
{
    public float damageMultiplier = 1;
    public Vector2 knockbackForce;
    public float knockbackDrag = 200;

    public bool isUsingDefaultPlayerPush = true;
    public MeleeProperty meleeProperty;
    public AudioClip onHitClip;
    public BloodData bloodData;
    public bool isFinisher;
}

[CreateAssetMenu(menuName = "ScriptableObjects/ComboDatabase")]
public class ComboDatabase : ScriptableObject
{

}
