using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PieceSpawner : MonoBehaviour
{
    [SerializeField] GameObject[] baseBag;
    [SerializeField] List<GameObject> currentBag = new List<GameObject>();
    readonly Vector3 SPAWN_POS_3WIDE = new Vector3(-0.25f, 4.25f);
    readonly Vector3 SPAWN_POS_4WIDE = new Vector3(0f, 4.5f);
    public GameObject GetRandomPieceObject()
    {
        GameObject pieceToSpawn = baseBag[Random.Range(0, baseBag.Length - 1)];

        return pieceToSpawn;
    }
    public Tetramino GetRandomPiece() => GetRandomPieceObject().GetComponent<Tetramino>();

    public GameObject GetPieceObject(char shape)
    {
        GameObject pieceToGet;
        switch (shape.ToString().ToUpper())
        {
            case "I":
                pieceToGet = baseBag[0];
                break;
            case "J":
                pieceToGet = baseBag[1];
                break;
            case "L":
                pieceToGet = baseBag[2];
                break;
            case "O":
                pieceToGet = baseBag[3];
                break;
            case "S":
                pieceToGet = baseBag[4];
                break;
            case "T":
                pieceToGet = baseBag[5];
                break;
            case "Z":
                pieceToGet = baseBag[6];
                break;
            default:
                Debug.LogError("\"" + shape.ToString() + "\" is not a valid piece shape");
                pieceToGet = null;
                break;

        }

        return pieceToGet;
    }
    public Tetramino GetPiece(char shape) => GetPieceObject(shape).GetComponent<Tetramino>();

    public Tetramino SpawnPiece(GameObject pieceToSpawn)
    {
        GameObject spawnedPiece = Instantiate(pieceToSpawn);
        spawnedPiece.transform.SetParent(FindAnyObjectByType<Playfield>().transform);
        return spawnedPiece.GetComponent<Tetramino>();
    }

    public Tetramino SpawnRandomPiece()
    {
        return SpawnPiece(GetRandomPieceObject());
    }

    void RefillBag()
    {
        currentBag.AddRange(baseBag);
    }

    public Tetramino SpawnFromBag()
    {
        int randomIndex = Random.Range(0, currentBag.Count);
        if (currentBag.Count <= 0)
            RefillBag();
        Tetramino spawnedPiece = SpawnPiece(currentBag[randomIndex]);
        currentBag.RemoveAt(randomIndex);
        return spawnedPiece;
    }
}
