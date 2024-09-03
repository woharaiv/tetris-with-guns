using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PieceSpawner : MonoBehaviour
{
    [SerializeField] GameObject[] pieces;
    readonly Vector3 SPAWN_POS_3WIDE = new Vector3(-0.25f, 4.25f);
    readonly Vector3 SPAWN_POS_4WIDE = new Vector3(0f, 4.5f);
    public GameObject GetRandomPieceObject()
    {
        GameObject pieceToSpawn = pieces[Random.Range(0, pieces.Length - 1)];

        return pieceToSpawn;
    }
    public Tetramino GetRandomPiece() => GetRandomPieceObject().GetComponent<Tetramino>();

    public GameObject GetPieceObject(char shape)
    {
        GameObject pieceToGet;
        switch (shape.ToString().ToUpper())
        {
            case "I":
                pieceToGet = pieces[0];
                break;
            case "J":
                pieceToGet = pieces[1];
                break;
            case "L":
                pieceToGet = pieces[2];
                break;
            case "O":
                pieceToGet = pieces[3];
                break;
            case "S":
                pieceToGet = pieces[4];
                break;
            case "T":
                pieceToGet = pieces[5];
                break;
            case "Z":
                pieceToGet = pieces[6];
                break;
            default:
                Debug.LogError("\"" + shape.ToString() + "\" is not a valid piece shape");
                pieceToGet = null;
                break;

        }

        return pieceToGet;
    }
    public Tetramino GetPiece(char shape) => GetPieceObject(shape).GetComponent<Tetramino>();

    public Tetramino SpawnRandomPiece()
    {
        GameObject pieceToSpawn = Instantiate(GetRandomPieceObject());
        pieceToSpawn.transform.SetParent(FindAnyObjectByType<Playfield>().transform);
        return pieceToSpawn.GetComponent<Tetramino>();
    }
}
