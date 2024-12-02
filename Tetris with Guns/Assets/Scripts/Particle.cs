using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(Collider2D)), RequireComponent(typeof(Rigidbody2D))]
public class Particle : MonoBehaviour
{
    static readonly float moveDuration = 1.0f;
    public Sequence ScheduleMoveToPoint(Vector2 point, float delay)
    {
        Sequence ret = DOTween.Sequence();
        ret.AppendInterval(delay)
            .AppendCallback(()=>DisableCollision())
            .Append(GetComponent<Rigidbody2D>().DOMove(point, moveDuration))
            .AppendCallback(() => Destroy(gameObject));
        return ret;
    }
    void DisableCollision()
    {
        GetComponent<BoxCollider2D>().enabled = false;
    }
}
