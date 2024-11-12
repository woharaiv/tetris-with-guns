using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Physics2D = RotaryHeart.Lib.PhysicsExtension.Physics2D;

public interface ITileCrate
{
    public void SmashCrate() { }
}
public class TileCrateSmall : MonoBehaviour, ITileCrate
{
    public Tile tileScript;
    
    public void SmashCrate()
    {
        Gun.instance.AddAmmo(5);
        Instantiate(Resources.Load<GameObject>("Prefabs/InfoPopup"), transform.position, Quaternion.identity, null)
            .GetComponent<InfoPopup>().Initialize(InfoMode.AMMO);
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
        
        /*0  1  2
         *3 [ ] 4
         *5  6  7*/
        TileCrateSmall[] surroundingTiles = new TileCrateSmall[8] {null, null, null, null, null, null, null, null};
        
        // Check and store if crate left
        surroundingTiles[3] = CrateInDirection(Vector2.left, true);
        // Check and store if crate right
        surroundingTiles[4] = CrateInDirection(Vector2.right, true);
        
        //If crate above
        surroundingTiles[1] = CrateInDirection(Vector2.up, true);
        if(surroundingTiles[1])
        {
            //  If crate left && crate above left
            surroundingTiles[0] = CrateInDirection(Vector2.up + Vector2.left, true);
            if (surroundingTiles[3] && surroundingTiles[0])
            {
                surroundingTiles[0].GetComponent<Tile>().KillTile();
                surroundingTiles[1].GetComponent<Tile>().KillTile();
                surroundingTiles[3].GetComponent<Tile>().KillTile();
                Destroy(gameObject);
                Instantiate(Resources.Load<GameObject>("Prefabs/BigCrate"), transform.position + new Vector3(-0.25f, 0.25f), Quaternion.identity);
                //Spawn on top left corner;
                return;
            }
            //  If crate right && above right
            surroundingTiles[2] = CrateInDirection(Vector2.up + Vector2.right, true);
            if (surroundingTiles[4] && surroundingTiles[2])
            {
                surroundingTiles[1].GetComponent<Tile>().KillTile();
                surroundingTiles[2].GetComponent<Tile>().KillTile();
                surroundingTiles[4].GetComponent<Tile>().KillTile();
                Destroy(gameObject);
                Instantiate(Resources.Load<GameObject>("Prefabs/BigCrate"), transform.position + new Vector3(0.25f, 0.25f), Quaternion.identity);
                return;
            }
        }
        //If crate below
        surroundingTiles[6] = CrateInDirection(Vector2.down, true);
        if (surroundingTiles[6])
        {
            //If crate left && crate below left
            surroundingTiles[5] = CrateInDirection(Vector2.down + Vector2.left, true);
            if (surroundingTiles[3] && surroundingTiles[5])
            {
                surroundingTiles[3].gameObject.GetComponent<Tile>().KillTile();
                surroundingTiles[5].gameObject.GetComponent<Tile>().KillTile();
                surroundingTiles[6].gameObject.GetComponent<Tile>().KillTile();
                Destroy(gameObject);
                //Spawn on bottom left corner;
                Instantiate(Resources.Load<GameObject>("Prefabs/BigCrate"), transform.position + new Vector3(-0.25f, -0.25f), Quaternion.identity);
                return;
            }
            //If crate right && below right
            surroundingTiles[7] = CrateInDirection(Vector2.down + Vector2.right, true);
            if (surroundingTiles[4] && surroundingTiles[7])
            {
                surroundingTiles[4].GetComponent<Tile>().KillTile();
                surroundingTiles[6].GetComponent<Tile>().KillTile();
                surroundingTiles[7].GetComponent<Tile>().KillTile();
                Destroy(gameObject);
                //Spawn on bottom right corner;
                Instantiate(Resources.Load<GameObject>("Prefabs/BigCrate"), transform.position + new Vector3(0.25f, -0.25f), Quaternion.identity);
                return;
            }
        }
        return;
    }

    TileCrateSmall CrateInDirection(Vector2 direction, bool showCast = false)
    {
        foreach (Collider2D hit in Physics2D.OverlapPointAll((Vector2)transform.position + direction * 0.5f, 0.1f, RotaryHeart.Lib.PhysicsExtension.PreviewCondition.Both, 3))
        {
            if (hit.gameObject.GetComponent<TileCrateSmall>() != null)
            {
                return hit.gameObject.GetComponent<TileCrateSmall>();
            }
        }
        return null;
    }
}
