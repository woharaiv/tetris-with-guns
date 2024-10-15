using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICanBeShot
{
    public void OnShot(Vector2 hitScreenPosition, int shotDamage);
}
