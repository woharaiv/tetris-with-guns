using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DefaultMachineGunShoot", menuName = "Scriptable Objects/Gun Shoot/Machine Gun Shoot", order = 2)]
public class MachineGunShoot : IGunShoot
{
    public override void ShootVFX(Vector2 hitPosition)
    {
        //Screen flash
        DOTween.Sequence()
            .Append(Gun.instance.screenFlashAsset.DOColor(new Color(1, 1, 1, 0.1f), 0.05f))
            .Append(Gun.instance.screenFlashAsset.DOColor(new Color(1, 1, 1, 0), 0.1f));

        //Screen shake
        Gun.impulseSource.GenerateImpulseAt(hitPosition, Random.insideUnitCircle.normalized * 0.1f * shakeStrength);
    }
}
