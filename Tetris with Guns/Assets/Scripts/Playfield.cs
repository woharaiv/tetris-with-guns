using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Physics2D = RotaryHeart.Lib.PhysicsExtension.Physics2D;

public class Playfield : MonoBehaviour
{
    [SerializeField, Range(1, 20)] public int playfieldWidth, playfieldHeight;
    [SerializeField, Range(0.001f, 10f)] public static float tileSize = 0.5f;
    Vector2[] playfieldCorners = new Vector2[4];
    Vector3[] playfieldWallsStrip;
    Vector3[] playfieldGrid;
    [SerializeField] Transform topEdge, floor, leftWall, rightWall;
    List<float> clearedRowYs = new();
    public List<Tile> tilesInPlay = new();
    [SerializeField] bool drawClearCheck = false;

    public static Playfield instance { get; private set; }

    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("Found more than one playfield, destroying this one");
            Destroy(gameObject);
            return;
        }
        instance = this;

        CalcPlayfieldCorners();
        UpdateWell(false);
    }

    public List<float> RowClearCheck(out List<Particle> particles)
    {
        float rowCheckY;
        clearedRowYs.Clear();
        List<Tile> tilesToKill = new List<Tile>();
        for(int i = 0; i < playfieldHeight; i++) //Check for a line clear on each row
        {
            rowCheckY = playfieldCorners[2].y + tileSize * i + tileSize * 0.5f;
            Vector3 lineCastStart = new Vector2(playfieldCorners[0].x * 1.1f, rowCheckY);
            Vector3 linecastEnd = new Vector2(playfieldCorners[1].x * 1.1f, rowCheckY);
            RaycastHit2D[] hits = Physics2D.LinecastAll(lineCastStart, linecastEnd, LayerMask.GetMask("Default"), (drawClearCheck ? RotaryHeart.Lib.PhysicsExtension.PreviewCondition.Editor : RotaryHeart.Lib.PhysicsExtension.PreviewCondition.None), 10f); //Cast a ray across the playfield
            if (hits.Length < playfieldWidth) //Don't bother if we hit less things than tiles needed to make a line
                continue;
            
            HashSet<Tile> tiles = new HashSet<Tile>(); //Hashset to keep track of all the tiles found in this row
            int j = 0;
            foreach(RaycastHit2D hit in hits) //Go through the list of hits; add any tiles found to the hashset
            {
                Tile tileCheck = hit.transform.gameObject.GetComponent<Tile>();
                if (tileCheck != null)
                {
                    tiles.Add(tileCheck);
                    j++;
                }
            }
            if (tiles.Count == playfieldWidth) //If we found a number of tiles equal to the number of columns, we cleared that line. Delete all the tiles in it.
            {
                foreach (Tile tile in tiles)
                    tilesToKill.Add(tile);
                clearedRowYs.Add(rowCheckY);
            }
        }
        Tile.KillType clearType;
        switch(clearedRowYs.Count)
        {
            case 0:  clearType = Tile.KillType.Default; break;
            case 1:  clearType = Tile.KillType.ClearSingle; break;
            case 2:  clearType = Tile.KillType.ClearDouble; break;
            case 3:  clearType = Tile.KillType.ClearTriple; break;
            case 4:  clearType = Tile.KillType.ClearQuad; break;
            default: clearType = Tile.KillType.ClearMega; break;

        }
        particles = new List<Particle>();
        foreach(Tile t in tilesToKill)
        {
            //AddRange throws an error if you try to add null
            List<Particle> possibleParticles = t.KillTile(clearType, true);
            if(possibleParticles != null)
                particles.AddRange(possibleParticles);
        }
        foreach(float yPos in clearedRowYs)
        {
             Debug.LogFormat("Dropping tiles above y: {0:N}", yPos);
             Physics2D.Linecast(new Vector2(playfieldCorners[0].x * 1.1f, yPos), new Vector2(playfieldCorners[2].x * 1.1f, yPos), RotaryHeart.Lib.PhysicsExtension.PreviewCondition.None, 10f);
             foreach (Tile tile in tilesInPlay)
             {
                if (tile == null)
                {
                    tilesInPlay.Remove(tile);
                    continue;
                }
                else if(tile.transform.position.y + tileSize * 0.1 > yPos) //For each row cleared, drop all the tiles that lie above that line
                {
                    tile.QueueDrop();
                }
             }
        }
        return clearedRowYs;
    }

    public void DropAllTiles()
    {
        foreach(Tile tile in tilesInPlay)
        {
            tile.Drop();
        }
    }

    private void OnDrawGizmos()
    {   
        CalcPlayfieldCorners();

        UpdateWell(true);

        DrawGrid();
    }

    void CalcPlayfieldCorners()
    {
        //Calculate corners of playfield
        playfieldCorners[0].x = playfieldCorners [3].x = transform.position.x + (playfieldWidth * tileSize) / 2;
        playfieldCorners[1].x = playfieldCorners[2].x = transform.position.x - (playfieldWidth * tileSize) / 2;

        playfieldCorners[0].y = playfieldCorners[1].y = transform.position.y + (playfieldHeight * tileSize) / 2;
        playfieldCorners[2].y = playfieldCorners[3].y = transform.position.y - (playfieldHeight * tileSize) / 2;
    }

    void UpdateWell(bool drawGizmos)
    {
        if (drawGizmos)
        {
            //Draw walls and floor
            Gizmos.color = Color.white;
            playfieldWallsStrip = new Vector3[4] { playfieldCorners[0], playfieldCorners[3], playfieldCorners[2], playfieldCorners[1] };
            Gizmos.DrawLineStrip(playfieldWallsStrip, false);

            //Draw top limit
            Gizmos.color = Color.red;
            Gizmos.DrawLine(playfieldCorners[0], playfieldCorners[1]);
        }

        //Place and scale objects
        rightWall.position = transform.position + (Vector3.right * playfieldWidth * tileSize * 0.5f) + (Vector3.right * rightWall.localScale.x / 2);
        rightWall.localScale = new Vector3(rightWall.localScale.x, playfieldHeight * tileSize, 1);

        leftWall.position = transform.position + (Vector3.left * playfieldWidth * tileSize * 0.5f) + (Vector3.left * leftWall.localScale.x / 2);
        leftWall.localScale = new Vector3(leftWall.localScale.x, playfieldHeight * tileSize, 1);

        floor.position = transform.position + (Vector3.down * playfieldHeight * tileSize * 0.5f) + (Vector3.down * floor.localScale.y / 2);
        floor.localScale = new Vector3(playfieldWidth * tileSize * 1.1f, floor.localScale.y, 1);
        
        topEdge.position = transform.position + (Vector3.up * playfieldHeight * tileSize * 0.5f) + (Vector3.up * topEdge.localScale.y / 2);
        topEdge.localScale = new Vector3(playfieldWidth * tileSize * 1.1f, topEdge.localScale.y, 1);
    }

    void DrawGrid()
    {
        Gizmos.color = Color.gray;
        playfieldGrid = new Vector3[((playfieldWidth - 1) * 2) + ((playfieldHeight - 1) * 2)];
        int i = 0;
        for (int j = 1; j < playfieldWidth; j++)
        {
            playfieldGrid[i] = new Vector3(playfieldCorners[1].x + tileSize * j, playfieldCorners[3].y);
            i++;
            playfieldGrid[i] = new Vector3(playfieldCorners[1].x + tileSize * j, playfieldCorners[0].y);
            i++;
        }
        for (int j = 1; j < playfieldHeight; j++)
        {
            playfieldGrid[i] = new Vector3(playfieldCorners[0].x, playfieldCorners[2].y + tileSize * j);
            i++;
            playfieldGrid[i] = new Vector3(playfieldCorners[1].x, playfieldCorners[2].y + tileSize * j);
            i++;
        }
        Gizmos.DrawLineList(playfieldGrid);
    }
}
