using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileCrateBig : MonoBehaviour, ITileCrate
{
    [SerializeField] TileCrateBig[] otherCrates;
    public void SmashCrate()
    {
        foreach (TileCrateBig crate in otherCrates)
        {
            Destroy(crate.gameObject);
        }
        string UnlockString = Gun.instance.UnlockRandomWeapon();
        if(UnlockString.Length > 1) 
        {
            GameManager.SpawnInfoPopup(transform.parent.position, InfoMode.CUSTOM_TEXT, UnlockString);
        }
    }
}
