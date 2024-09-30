using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Physics2D = RotaryHeart.Lib.PhysicsExtension.Physics2D;

public class Tetramino : MonoBehaviour
{
    List<Tile> tiles = new();
    float tileSize;
    float timestamp;
    int rotationState = 0;
    [SerializeField] bool showGrid;
    [SerializeField, Range(3, 4)] int width = 3;
    [SerializeField] Color color;
    public bool isActivePiece = true;

    [SerializeField] public Sprite piecePreview;

    private void Start()
    {
        tiles.AddRange(GetComponentsInChildren<Tile>());
        tileSize = Playfield.tileSize;
        timestamp = Time.realtimeSinceStartup;
        foreach (Tile tile in tiles)
        {
            tile.timestamp = timestamp;
            tile.owner = this;
            tile.color = color;
        }
    }

    public bool IsObstructed(Vector2 direction, Vector2? offset = null, Color? raycastColor = null)
    {
        foreach (Tile tile in tiles)
        {
            foreach(RaycastHit2D hit in Physics2D.BoxCastAll(tile.transform.position + (Vector3)(offset != null ? offset * tileSize : Vector2.zero), Vector2.one * tileSize * 0.9f, 0, direction, tileSize, (raycastColor != null? RotaryHeart.Lib.PhysicsExtension.PreviewCondition.Both : RotaryHeart.Lib.PhysicsExtension.PreviewCondition.None), 5f, raycastColor, raycastColor)) //Cast a tile-sized box in the intended direction from each tile in the mino
            {
                if(hit.collider == null) continue; //If it didn't hit anything, skip this loop
                if(hit.collider.gameObject.CompareTag("Obstruction")) //If it hit an obstruction, it's obstructed, unless that thing it hit was one of the other tiles in the mino
                {
                    var tileComponent = hit.collider.gameObject.GetComponent<Tile>();
                    if (tileComponent == null || tileComponent.timestamp != tile.timestamp)
                        return true;
                }
            }
        }
        return false;
    }

    //Possible wall kick directions
    readonly Vector2Int m2m1 = new Vector2Int(-2, -1);
    readonly Vector2Int m2p0 = new Vector2Int(-2,  0);
    readonly Vector2Int m2p1 = new Vector2Int(-2,  1);

    readonly Vector2Int m1m2 = new Vector2Int(-1, -2);
    readonly Vector2Int m1m1 = new Vector2Int(-1, -1);
    readonly Vector2Int m1p0 = new Vector2Int(-1,  0);
    readonly Vector2Int m1p1 = new Vector2Int(-1,  1);
    readonly Vector2Int m1p2 = new Vector2Int(-1,  2);
    
    readonly Vector2Int p0m2 = new Vector2Int( 0, -2);
    readonly Vector2Int p0p2 = new Vector2Int( 0,  2);
    
    readonly Vector2Int p1m2 = new Vector2Int( 1, -2);
    readonly Vector2Int p1m1 = new Vector2Int( 1, -1);
    readonly Vector2Int p1p0 = new Vector2Int( 1,  0);
    readonly Vector2Int p1p1 = new Vector2Int( 1,  1);
    readonly Vector2Int p1p2 = new Vector2Int( 1,  2);
    
    readonly Vector2Int p2m1 = new Vector2Int( 2, -1);
    readonly Vector2Int p2p1 = new Vector2Int( 2,  1);
    readonly Vector2Int p2p0 = new Vector2Int( 2,  0);
    

    public bool TryRotate(float quarterTurns)
    {
        //Try to rotate
        transform.localEulerAngles += Vector3.forward * 90 * quarterTurns;
        UnityEngine.Physics2D.SyncTransforms();
        bool rotateObstructed = IsObstructed(Vector2.zero);
        //If that rotation would clip into something, try various wall kicks https://tetris.fandom.com/wiki/SRS#Wall_Kicks
        if (rotateObstructed)
        {
            Vector2Int? moveAttempt = null;
            if(width == 3) //Most piece wall kicks
            {
                if(quarterTurns < 0) //Clockwise rotations
                {
                    switch(rotationState)
                    {
                        case 0:
                            moveAttempt = wallKickTests(m1p0, m1p1, p0m2, m1m2);
                            break;
                        case 1:
                            moveAttempt = wallKickTests(p1p0, p1m1, p0p2, p1p2);
                            break;
                        case 2:
                            moveAttempt = wallKickTests(p1p0, p1p1, p0m2, p1m2);
                            break;
                        case 3:
                            moveAttempt = wallKickTests(m1p0, m1m1, p0p2, m1p2);
                            break;
                    }
                }
                else //Counterclockwise rotations
                {
                    switch (rotationState)
                    {
                        case 0:
                            moveAttempt = wallKickTests(p1p0, p1p1, p0m2, p1m2);
                            break;
                        case 1:
                            moveAttempt = wallKickTests(p1p0, p1m1, p0m2, p1p2);
                            break;
                        case 2:
                            moveAttempt = wallKickTests(m1p0, m1p1, p0m2, m1m2);
                            break;
                        case 3:
                            moveAttempt = wallKickTests(m1p0, m1m1, p0p2, m1p2);
                            break;
                    }
                }
            }
            else //I-piece wall kicks (O piece has width 4 but doesn't move when rotated and thus can't ever kick)
            {
                if (quarterTurns < 0) //Clockwise rotations
                {
                    switch (rotationState)
                    {
                        case 0:
                            moveAttempt = wallKickTests(m2p0, p1p0, m2m1, p1p2);
                            break;
                        case 1:
                            moveAttempt = wallKickTests(m1p0, p2p0, m1p2, p2m1);
                            break;
                        case 2:
                            moveAttempt = wallKickTests(p2p0, m1p0, p2p1, m1m2);
                            break;
                        case 3:
                            moveAttempt = wallKickTests(p1p0, m2p0, p1m2, m2p1);
                            break;
                    }
                }
                else //Counterclockwise rotations
                {
                    switch (rotationState)
                    {
                        case 0:
                            moveAttempt = wallKickTests(m1p0, p2p0, m1p2, p2m1);
                            break;
                        case 1:
                            moveAttempt = wallKickTests(p2p0, m1p0, p2p1, m1m2);
                            break;
                        case 2:
                            moveAttempt = wallKickTests(p1p0, m2p0, p1m2, m2p1);
                            break;
                        case 3:
                            moveAttempt = wallKickTests(m2p0, p1p0, m2m1, p1p2);
                            break;
                    }
                }
            }
            if(moveAttempt == null)
            {
                transform.localEulerAngles -= Vector3.forward * 90 * quarterTurns;
                UnityEngine.Physics2D.SyncTransforms();
                return false;
            }
            else
            {
                transform.position += (Vector3)(Vector2)moveAttempt * Playfield.tileSize;
                rotationState = (rotationState - (int)quarterTurns) % 4;
                if (rotationState < 0)
                    rotationState += 4;
                Debug.Log(rotationState);
                foreach (Tile tile in tiles)
                {
                    tile.transform.localEulerAngles -= Vector3.forward * 90 * quarterTurns;
                }
                UnityEngine.Physics2D.SyncTransforms();
                return true;
            }
        }
        //If it's successful, rotate the tiles the opposite direction so they stay upright
        else
        {
            rotationState = (rotationState - (int)quarterTurns) % 4;
            if (rotationState < 0)
                rotationState += 4;
            foreach (Tile tile in tiles)
            {
                tile.transform.localEulerAngles -= Vector3.forward * 90 * quarterTurns;
            }
            UnityEngine.Physics2D.SyncTransforms();
            return true;
        }
    }

   public Vector2Int? wallKickTests(Vector2Int test2, Vector2Int test3, Vector2Int test4, Vector2Int test5) //Test1 being the base location
    {
        if (!IsObstructed(Vector2.zero, test2))
            return test2;
        if (!IsObstructed(Vector2.zero, test3))
            return test3;
        if (!IsObstructed(Vector2.zero, test4))
            return test4;
        if (!IsObstructed(Vector2.zero, test5))
            return test5;

        return null;
    }
    
    public void TileKilled(Tile tile)
    {
        tiles.Remove(tile);
        if (tiles.Count <= 0)
            Destroy(gameObject);
    }

    //.25 -.25
    Vector3[] threeWideOffsetGrid = new Vector3[8]
    {
        new Vector3(-0.25f, 0.75f),
        new Vector3(0.25f, 0.75f),

        new Vector3(-0.25f, -0.75f),
        new Vector3(0.25f, -0.75f),

        new Vector3(-0.25f, -0.75f),
        new Vector3(-0.25f, 0.75f),

        new Vector3(0.25f, -0.75f),
        new Vector3(0.25f, 0.75f)
    };
    Vector3[] fourWideOffsetGrid = new Vector3[12]
    {
        new Vector3(-0.5f, 1),
        new Vector3(-0.5f, -1),

        new Vector3(0, 1),
        new Vector3(0, -1),
        
        new Vector3(0.5f, 1),
        new Vector3(0.5f, -1),
        
        new Vector3(-1, 0.5f),
        new Vector3(1, 0.5f),

        new Vector3(-1, 0),
        new Vector3(1, 0),

        new Vector3(-1, -0.5f),
        new Vector3(1, -0.5f)
    };
    Vector3[] gizmoGrid;
    private void OnDrawGizmos()
    {
        if(!showGrid) return;

        gizmoGrid = new Vector3[(width == 3 ? 8 : 12)];
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, Vector3.one*Playfield.tileSize*width);
        for(int i = 0; i < (width == 3 ? threeWideOffsetGrid : fourWideOffsetGrid).Length; i++)
        {
            gizmoGrid[i] = (width == 3 ? threeWideOffsetGrid : fourWideOffsetGrid)[i] * Playfield.tileSize * 2 + transform.position;
        }
        Gizmos.DrawLineList(gizmoGrid);
    }

}

