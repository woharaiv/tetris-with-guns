using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class Tile : MonoBehaviour
{
    public float timestamp = -1f;
    public Tetramino owner;
    int distanceToDrop = 0;
    [SerializeField] int maxHealth;
    int tileHealth;
    [SerializeField] Sprite[] damageSprites;
    public Color color = Color.gray;

    private void Start()
    {
        Playfield.instance.tilesInPlay.Add(this);
        tileHealth = maxHealth;
    }

    public void QueueDrop(int tiles = 1)
    {
        distanceToDrop++;
    }

    public void Drop()
    {
        if (distanceToDrop > 0)
            transform.position += Vector3.down * Playfield.tileSize * distanceToDrop;
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

    
}
