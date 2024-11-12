using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DefaultShotgunShoot", menuName = "Scriptable Objects/Gun Shoot/Shotgun Shoot", order = 2)]
public class ShotgunShoot : IGunShoot
{
    public int bulletCount = 7;
    public override void shoot(Vector2 clickPosition, WeaponType weapon)
    {
        this.weapon = weapon;

        ShootParticle(clickPosition);
        ShootSound(clickPosition);
        ShootVFX(clickPosition);
        for (int i = 0; i < bulletCount; i++)
        {
            Vector2 bulletPosition = clickPosition + (Random.insideUnitCircle * spreadRadius);
            ShootCheck(bulletPosition);
        }
    }
}