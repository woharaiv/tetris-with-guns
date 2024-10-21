using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Physics2D = RotaryHeart.Lib.PhysicsExtension.Physics2D;

[RequireComponent(typeof(BoxCollider2D))]
public class Tile : MonoBehaviour, ICanBeShot
{
    static float gravityMod = 0.1f;

    public float timestamp = -1f;
    public Tetramino owner;
    int distanceToDrop = 0;
    [SerializeField] int maxHealth;
    int tileHealth;
    [SerializeField] Sprite[] damageSprites;
    public Color color = Color.gray;

    public bool checkedGravThisFrame = false;
    public bool alreadyCheckingGravity = false;
    public bool usingGravity = false;

    private void Start()
    {
        Playfield.instance.tilesInPlay.Add(this);
        tileHealth = maxHealth;
        color = GetComponent<SpriteRenderer>().color;
    }

    private void Update()
    {
        if (checkedGravThisFrame)
            checkedGravThisFrame = false;
        if(usingGravity)
        {
            //Check if the tile should fall this frame
            Vector3 fallingVelocity = Vector3.down * (Time.deltaTime * GameManager.instance.gravity * GameManager.instance.softDropSpeedMult * gravityMod);
            foreach(Collider2D other in Physics2D.OverlapPointAll(transform.position + fallingVelocity + Vector3.down / 2f * Playfield.tileSize, LayerMask.NameToLayer("Obstruction"),RotaryHeart.Lib.PhysicsExtension.PreviewCondition.None, 10f))
            {
                Tile tileComponent = other.GetComponent<Tile>();
                if (!tileComponent || !tileComponent.usingGravity)
                {
                    usingGravity = false;
                    GetComponent<SpriteRenderer>().color = color;
                    break;
                }
            }
            if(usingGravity) //If it should, move it down.
                transform.position += fallingVelocity;
            else //If it shouldn't, snap it to the tile.
            {
                float yPos = transform.position.y;
                float yPosTruncated = (float)System.Math.Truncate(yPos);
                float yPosDecimalPortion = Mathf.Abs(yPos - yPosTruncated);
                transform.position = new Vector3(transform.position.x, yPosTruncated + ((yPosDecimalPortion < 0.5 ? 0.25f : 0.75f) * Mathf.Sign(yPos)), transform.position.z);
                GetComponent<TileCrateSmall>()?.TryMergeCrates();
            }
        }
    }

    public void QueueDrop(int tiles = 1)
    {
        if(!usingGravity)
            distanceToDrop += tiles;
    }

    public void Drop()
    {
        if (distanceToDrop > 0)
        {
            if(!usingGravity)
                transform.position += distanceToDrop * Playfield.tileSize * Vector3.down;
            distanceToDrop = 0;
        }
    }
    public void OnShot(Vector2 hitScreenPosition, int shotDamage)
    {
        DamageTile(shotDamage);
    }

    public void DamageTile(int damage = 1)
    {
        tileHealth -= damage;
        if (tileHealth <= 0)
        {
            KillTile();
            return;
        }
        else
            GetComponent<SpriteRenderer>().sprite = damageSprites[maxHealth - tileHealth];
    }

    public void KillTile()
    {
        if (!owner || !owner.isActivePiece)
        {  
            //Makes gravity checks think this tile doesn't exist 
            alreadyCheckingGravity = true;
            //Make all neighboring tiles check for gravity
            //Left
            gravCheckNeighbor = Physics2D.OverlapPoint(transform.position + Vector3.left * Playfield.tileSize, LayerMask.NameToLayer("Obstruction"));
            if(gravCheckNeighbor)
            {
                gravCheckTile = gravCheckNeighbor.GetComponent<Tile>();
                if(gravCheckTile)
                    gravCheckTile.CheckForGravity();
            }
            //Right
            gravCheckNeighbor = Physics2D.OverlapPoint(transform.position + Vector3.right * Playfield.tileSize, LayerMask.NameToLayer("Obstruction"));
            if (gravCheckNeighbor)
            {
                gravCheckTile = gravCheckNeighbor.GetComponent<Tile>();
                if (gravCheckTile)
                    gravCheckTile.CheckForGravity();
            }
            //Up
            gravCheckNeighbor = Physics2D.OverlapPoint(transform.position + Vector3.up * Playfield.tileSize, LayerMask.NameToLayer("Obstruction"));
            if (gravCheckNeighbor)
            {
                gravCheckTile = gravCheckNeighbor.GetComponent<Tile>();
                if (gravCheckTile)
                    gravCheckTile.CheckForGravity();
            }
            //Down
            gravCheckNeighbor = Physics2D.OverlapPoint(transform.position + Vector3.down * Playfield.tileSize, LayerMask.NameToLayer("Obstruction"));
            if (gravCheckNeighbor)
            {
                gravCheckTile = gravCheckNeighbor.GetComponent<Tile>();
                if (gravCheckTile)
                    gravCheckTile.CheckForGravity();
            }
        }

        if (Playfield.instance != null)
        {
            Playfield.instance.tilesInPlay.Remove(this);
        }
        if (owner != null)
        {
            owner.TileKilled(this);
        }
        Destroy(gameObject);
    }

    Tile gravCheckTile;
    Collider2D gravCheckNeighbor;
    public bool CheckForGravity()
    {
        //Check all neighbors

        alreadyCheckingGravity = true;
        checkedGravThisFrame = true;

        //If a neighbor is a tile, return false if that one's check for gravity returns false.

        //Check down
        gravCheckNeighbor = Physics2D.OverlapPoint(transform.position + Vector3.down * Playfield.tileSize, LayerMask.NameToLayer("Obstruction"));
        if (gravCheckNeighbor)
        {
            gravCheckTile = gravCheckNeighbor.GetComponent<Tile>();
            //If there's a floor, return false.
            if (!gravCheckTile)
            {
                alreadyCheckingGravity = false;
                return false;
            }
        }
        if (gravCheckNeighbor && gravCheckTile && !gravCheckTile.alreadyCheckingGravity && (gravCheckTile.checkedGravThisFrame ? !gravCheckTile.usingGravity : gravCheckTile.CheckForGravity() == false))
        {
            alreadyCheckingGravity = false;
            usingGravity = false;
            GetComponent<SpriteRenderer>().color = color;
            return false;
        }
        /*
        if (gravCheckTile && gravCheckTile.alreadyCheckingGravity)
            GetComponent<SpriteRenderer>().color += Color.red;
        if (gravCheckTile && gravCheckTile.checkedGravThisFrame)
            GetComponent<SpriteRenderer>().color += Color.green;
        if(gravCheckTile && gravCheckTile.usingGravity)
            GetComponent<SpriteRenderer>().color += Color.blue;
        */

        //Check left
        gravCheckNeighbor = Physics2D.OverlapPoint(transform.position + Vector3.left * Playfield.tileSize, LayerMask.NameToLayer("Obstruction"));
        if (gravCheckNeighbor) 
            gravCheckTile = gravCheckNeighbor.GetComponent<Tile>();
        if (gravCheckNeighbor && gravCheckTile && !gravCheckTile.alreadyCheckingGravity && (gravCheckTile.checkedGravThisFrame ? !gravCheckTile.usingGravity : gravCheckTile.CheckForGravity() == false))
        {
            alreadyCheckingGravity = false;
            return false;
        }
        
        //Check right
        gravCheckNeighbor = Physics2D.OverlapPoint(transform.position + Vector3.right * Playfield.tileSize, LayerMask.NameToLayer("Obstruction"));
        if (gravCheckNeighbor)
            gravCheckTile = gravCheckNeighbor.GetComponent<Tile>();
        if (gravCheckNeighbor && gravCheckTile && !gravCheckTile.alreadyCheckingGravity && (gravCheckTile.checkedGravThisFrame ? !gravCheckTile.usingGravity : gravCheckTile.CheckForGravity() == false))
        {
            alreadyCheckingGravity = false;
            return false;
        }

        //Check up
        gravCheckNeighbor = Physics2D.OverlapPoint(transform.position + Vector3.up * Playfield.tileSize, LayerMask.NameToLayer("Obstruction"));
        if (gravCheckNeighbor)
            gravCheckTile = gravCheckNeighbor.GetComponent<Tile>();
        if (gravCheckNeighbor && gravCheckTile && !gravCheckTile.alreadyCheckingGravity && (gravCheckTile.checkedGravThisFrame ? !gravCheckTile.usingGravity : gravCheckTile.CheckForGravity() == false))
        {
            alreadyCheckingGravity = false;
            return false;
        }


        //If there are no neighbors (except for walls on the sides), return true.
        alreadyCheckingGravity = false;
        //GetComponent<SpriteRenderer>().color = Color.white;
        usingGravity = true;
        return true;
    }

}
