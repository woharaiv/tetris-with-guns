using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileCrateSmall : MonoBehaviour
{
    private void OnDestroy()
    {
        if(GameManager.instance.gameRunning)
        {
            FindAnyObjectByType<Gun>().AddAmmo(5);
        }
    }
}
