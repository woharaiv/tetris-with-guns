using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileCrateSmall : MonoBehaviour
{
    private void OnDestroy()
    {
        if(GameManager.instance.gameRunning && gameObject.scene.isLoaded) //isLoaded will be false if the destruction is happening because of the scene closing
        {
            FindAnyObjectByType<Gun>().AddAmmo(5);
            Instantiate(Resources.Load<GameObject>("Prefabs/InfoPopup"), transform.position, Quaternion.identity, null)
                .GetComponent<InfoPopup>().Initialize(InfoMode.AMMO);
        }
    }
    public void TryMergeCrates()
    {
        /* Possible state of surrounding crates:
         * XX?  ?XX  ???  ???  
         * XS?  ?SX  ?SX  XS?
         * ???  ???  ?XX  XX?
         *  OXO
         *  XSX
         *  OXX
         */

        //If crate above
        // Check and store if crate left
        //  If crate left && crate above left
        //      Spawn on top left corner; return;
        // Check and store if crate right
        //  If crate right && above right
        //      Spawn on top right corner; return;
        //If crate below
        //  If crate left && crate below left
        //      Spawn on bottom left corner; return;
        //  If crate right && below right
        //      Spawn on bottom right corner; return;
        //Else return;
    }
}
