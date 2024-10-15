using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class MenuStartButton : MonoBehaviour, ICanBeShot
{
    public void OnShot(Vector2 hitScreenPosition, int shotDamage)
    {
        GetComponent<SpriteRenderer>().color = Color.gray;
        MenuManager.instance.StartButtonShot();
    }
}
