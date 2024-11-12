using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "WeaponType", menuName = "Scriptable Objects/Weapon Type", order = 2)]
public class WeaponType : ScriptableObject
{
    new public string name;
    public float fireRate;
    public int ammo;
    public bool autoFire;
    public GameObject shootParticle;
    public AudioClip shootSound;
    [SerializeField, Tooltip("ScriptableObjects derived from IGunShoot won't show up in the selector list, but can be dragged into this spot.")] public IGunShoot shootScript;

    public void Shoot(Vector2 shootLocation)
    {
        shootScript.shoot(shootLocation, this);
    }
}
