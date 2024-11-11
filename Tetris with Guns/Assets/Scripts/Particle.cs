using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(Collider2D)), RequireComponent(typeof(Rigidbody2D))]
public class Particle : MonoBehaviour
{
    static readonly float moveDuration = 1.0f;
    public IEnumerator ScheduleMoveToPoint(Vector2 point, float delay)
    {
        yield return new WaitForSeconds(delay);
        Tween moveTween = GetComponent<Rigidbody2D>().DOMove(point, moveDuration);
        yield return moveTween.WaitForCompletion();
        Destroy(gameObject);
    }
}
