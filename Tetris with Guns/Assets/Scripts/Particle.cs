using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(Collider2D)), RequireComponent(typeof(Rigidbody2D))]
public class Particle : MonoBehaviour
{
    static readonly float lifespan = 10f;
    static readonly float despawnTime = 0.25f;
    Color color;
    Sequence despawnSequence;
    private void Start()
    {
        color = GetComponent<SpriteRenderer>().color;
    }
    public void StartDespawnSequence()
    {
        despawnSequence = DOTween.Sequence()
            .AppendInterval(lifespan)
            .Append(GetComponent<SpriteRenderer>().DOColor(new Color(color.r, color.g, color.b, 0f), despawnTime))
            .AppendCallback(() => Destroy(gameObject));
    }
    static readonly float moveDuration = 1.0f;
    public Sequence ScheduleMoveToPoint(Vector2 point, float delay)
    {
        despawnSequence.Kill();
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
