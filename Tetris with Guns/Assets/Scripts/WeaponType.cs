using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WeaponType", menuName = "Scriptable Objects/Weapon Type", order = 2)]
public class WeaponType : ScriptableObject
{
    new public string name;
    public float fireRate;
    public int ammo;
    public bool autoFire;
    public float spreadRadius;
    public float shakeStrength;
}
