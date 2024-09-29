using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitParticle : MonoBehaviour
{
    private void OnDestroy()
    {
        Debug.Log(gameObject);
    }
}
