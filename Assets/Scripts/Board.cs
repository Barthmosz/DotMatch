using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour
{
    private Tile[,] allTiles;
    private GamePiece[,] allGamePieces;

    public int width, height, borderSize;
    public GameObject tilePrefab;
    public GameObject[] gamePiecePrefabs;

    private void Start()
    {
        this.allTiles = new Tile[width, height];
        this.allGamePieces = new GamePiece[width, height];

        SetupTiles();
        SetupCamera();
        FillRandom();
    }

    private void SetupTiles()
    {
        for (int i = 0; i < this.width; i++)
        {
            for (int j = 0; j < this.height; j++)
            {
                GameObject tile = Instantiate(this.tilePrefab, new Vector3(i, j, 0), Quaternion.identity) as GameObject;
                tile.name = $"Tile ({i},{j})";
                tile.transform.parent = this.transform;
                this.allTiles[i, j] = tile.GetComponent<Tile>();
                this.allTiles[i, j].Init(i, j, this);
            }
        }
    }

    private void SetupCamera()
    {
        Camera.main.transform.position = new Vector3((float)(this.width - 1)/2f, (float)(this.height - 1)/ 2f, -10f);

        float aspectRatio = (float)Screen.width / (float)Screen.height;
        float verticalSize = (float)this.height/2f * (float)this.borderSize;
        float horizontalSize = ((float)this.width/2f + (float)this.borderSize) / aspectRatio;

        Camera.main.orthographicSize = (verticalSize > horizontalSize) ? verticalSize : horizontalSize;
    }

    private void FillRandom()
    {
        for (int i = 0; i < this.width; i++)
        {
            for (int j = 0; j < this.height; j++)
            {
                GameObject randomPiece = Instantiate(GetRandomGamePiece(), Vector3.zero, Quaternion.identity) as GameObject;

                if (randomPiece != null)
                {
                    PlaceGamePiece(randomPiece.GetComponent<GamePiece>(), i, j);
                }
            }
        }
    }

    private GameObject GetRandomGamePiece()
    {
        int randomIndex = Random.Range(0, this.gamePiecePrefabs.Length);

        if (this.gamePiecePrefabs[randomIndex] == null)
        {
            Debug.LogWarning($"BOARD: {randomIndex} does not contain a valid GamePiece prefab.");
        }

        return this.gamePiecePrefabs[randomIndex];
    }

    private void PlaceGamePiece(GamePiece gamePiece, int x, int y)
    {
        if (gamePiece == null)
        {
            Debug.LogWarning("BOARD: Invalid GamePiece.");
            return;
        }

        gamePiece.transform.position = new Vector3(x, y, 0);
        gamePiece.transform.rotation = Quaternion.identity;
        gamePiece.SetCoordinates(x, y);
    }
}
