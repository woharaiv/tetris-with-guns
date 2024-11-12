using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

[CreateAssetMenu(fileName = "DefaultGunShoot", menuName = "Scriptable Objects/Gun Shoot/Default Gun Shoot", order = 2)]
public class IGunShoot : ScriptableObject
{
    public GameObject bulletHole;
    public int shotDamage = 1;
    public float spreadRadius = 0;
    public float shakeStrength = 3;
    protected WeaponType weapon;
    public virtual void shoot(Vector2 clickPosition, WeaponType weapon)
    {
        clickPosition += (Random.insideUnitCircle * spreadRadius);
        this.weapon = weapon;

        ShootParticle(clickPosition);
        ShootSound(clickPosition);
        ShootVFX(clickPosition);
        ShootCheck(clickPosition);
    }
    public virtual void ShootParticle(Vector2 hitPosition)
    {
        Instantiate(weapon.shootParticle, (Vector3)hitPosition + Vector3.back, Quaternion.Euler(0, 0, Random.Range(0f, 360f)));
    }

    public virtual void ShootSound(Vector2 hitPosition)
    {
        AudioSource.PlayClipAtPoint(weapon.shootSound, hitPosition);
    }

    public virtual void ShootVFX(Vector2 hitPosition)
    {
        //Screen flash
        DOTween.Sequence()
            .Append(Gun.instance.screenFlashAsset.DOColor(new Color(1, 1, 1, 0.25f), 0.05f))
            .Append(Gun.instance.screenFlashAsset.DOColor(new Color(1, 1, 1, 0), 0.1f));

        //Screen shake
        Gun.impulseSource.GenerateImpulseAt(hitPosition, Random.insideUnitCircle.normalized * 0.1f * shakeStrength);
    }

    public virtual void ShootCheck(Vector2 bulletPosition)
    {
        bool hitBackground = true;
        Collider2D[] hit = Physics2D.OverlapPointAll(bulletPosition);
        foreach (Collider2D col in hit)
        {
            ICanBeShot hitCheck = col.gameObject.GetComponent<ICanBeShot>();
            if (col.CompareTag("Obstruction") || hitCheck != null)
                hitBackground = false;

            if (hitCheck != null)
                hitCheck.OnShot(bulletPosition, shotDamage);
        }

        if (hitBackground)
            Background.instance.ShootBackground(bulletPosition, bulletHole);
    }
}