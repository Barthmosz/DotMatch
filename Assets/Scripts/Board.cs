using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour
{
    private Tile[,] allTiles;
    private GamePiece[,] allGamePieces;
    private Tile clickedTile;
    private Tile targetTile;

    public GameObject[] gamePiecePrefabs;
    public GameObject tilePrefab;
    public int width, height, borderSize;
    public float swapTime = 0.5f;

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
                    randomPiece.GetComponent<GamePiece>().Init(this);
                    randomPiece.transform.parent = transform;
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

    public void PlaceGamePiece(GamePiece gamePiece, int x, int y)
    {
        if (gamePiece == null)
        {
            Debug.LogWarning("BOARD: Invalid GamePiece.");
            return;
        }

        gamePiece.transform.position = new Vector3(x, y, 0);
        gamePiece.transform.rotation = Quaternion.identity;
        if (IsWithinBounds(x, y))
        {
            this.allGamePieces[x, y] = gamePiece;
        }
        gamePiece.SetCoordinates(x, y);
    }

    private bool IsWithinBounds(int x, int y)
    {
        return (x >= 0 && x < this.width && y >= 0 && y < this.height);
    }

    public void ClickTile(Tile tile)
    {
        if (this.clickedTile == null)
        {
            this.clickedTile = tile;
        }
    }

    public void DragToTile(Tile tile)
    {
        if (this.clickedTile != null && IsNextTo(tile, this.clickedTile))
        {
            this.targetTile = tile;
            Debug.Log($"Clicked tile: {tile.name}");
        }
    }

    public void ReleaseTile()
    {
        if (this.clickedTile != null && this.targetTile != null)
        {
            SwitchTiles(this.clickedTile, this.targetTile);
        }
        this.clickedTile = null;
        this.targetTile = null;
    }

    private void SwitchTiles(Tile clickedTile, Tile targetTile)
    {
        GamePiece clickedPiece = this.allGamePieces[clickedTile.xIndex, clickedTile.yIndex];
        GamePiece targetPiece = this.allGamePieces[targetTile.xIndex, targetTile.yIndex];

        clickedPiece.Move(targetTile.xIndex, targetTile.yIndex, this.swapTime);
        targetPiece.Move(clickedTile.xIndex, clickedTile.yIndex, this.swapTime);
    }

    private bool IsNextTo(Tile start, Tile end)
    {
        if (Mathf.Abs(start.xIndex - end.xIndex) == 1 && start.yIndex == end.yIndex)
        {
            return true;
        }
        if (Mathf.Abs(start.yIndex - end.yIndex) == 1 && start.xIndex == end.xIndex)
        {
            return true;
        }
        return false;
    }

    private List<GamePiece> FindMatches(int startX, int startY, Vector2 searchDirection, int minLength = 3)
    {
        List<GamePiece> matches = new();
        GamePiece startPiece = null;

        if (IsWithinBounds(startX, startY))
        {
            startPiece = this.allGamePieces[startX, startY];
        }

        if (startPiece != null)
        {
            matches.Add(startPiece);
        }
        else
        {
            return null;
        }

        int nextX;
        int nextY;

        int maxValue = (this.width > this.height) ? width : height;

        for (int i = 1; i < maxValue - 1; i++)
        {
            nextX = startX + (int)Mathf.Clamp(searchDirection.x, -1, 1) * i;
            nextY = startY + (int)Mathf.Clamp(searchDirection.y, -1, 1) * i;

            if (!IsWithinBounds(nextX, nextY))
            {
                break;
            }

            GamePiece nextPiece = this.allGamePieces[nextX, nextY];

            if (nextPiece.matchValue == startPiece.matchValue && !matches.Contains(nextPiece))
            {
                matches.Add(nextPiece);
            }
            else
            {
                break;
            }
        }

        if (matches.Count >= minLength)
        {
            return matches;
        }
        return null;
    }
}
