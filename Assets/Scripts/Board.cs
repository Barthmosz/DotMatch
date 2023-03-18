using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        FillBoard();
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
        Camera.main.transform.position = new Vector3((float)(this.width - 1) / 2f, (float)(this.height - 1) / 2f, -10f);

        float aspectRatio = (float)Screen.width / (float)Screen.height;
        float verticalSize = (float)this.height / 2f * (float)this.borderSize;
        float horizontalSize = ((float)this.width / 2f + (float)this.borderSize) / aspectRatio;

        Camera.main.orthographicSize = (verticalSize > horizontalSize) ? verticalSize : horizontalSize;
    }

    private void FillBoard()
    {
        int maxIterations = 100;
        int iterations = 0;

        for (int i = 0; i < this.width; i++)
        {
            for (int j = 0; j < this.height; j++)
            {
                GamePiece piece = FillRandomAt(i, j);
                iterations = 0;

                while (HasMatchOnFill(i, j))
                {
                    ClearPieceAt(i, j);
                    piece = FillRandomAt(i, j);
                    iterations++;

                    if (iterations >= maxIterations)
                    {
                        break;
                    }
                }
            }
        }
    }

    private GamePiece FillRandomAt(int x, int y)
    {
        GameObject randomPiece = Instantiate(GetRandomGamePiece(), Vector3.zero, Quaternion.identity) as GameObject;

        if (randomPiece != null)
        {
            randomPiece.GetComponent<GamePiece>().Init(this);
            randomPiece.transform.parent = transform;
            PlaceGamePiece(randomPiece.GetComponent<GamePiece>(), x, y);
            return randomPiece.GetComponent<GamePiece>();
        }
        return null;
    }

    private bool HasMatchOnFill(int x, int y, int minLength = 3)
    {
        List<GamePiece> leftMatches = FindMatches(x, y, new Vector2(-1, 0), minLength);
        List<GamePiece> downwardMatches = FindMatches(x, y, new Vector2(0, -1), minLength);

        if (leftMatches == null)
        {
            leftMatches = new();
        }
        if (downwardMatches == null)
        {
            downwardMatches = new();
        }

        return (leftMatches.Count > 0 || downwardMatches.Count > 0);
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
        StartCoroutine(SwitchTilesRoutine(clickedTile, targetTile));
    }

    private IEnumerator SwitchTilesRoutine(Tile clickedTile, Tile targetTile)
    {
        GamePiece clickedPiece = this.allGamePieces[clickedTile.xIndex, clickedTile.yIndex];
        GamePiece targetPiece = this.allGamePieces[targetTile.xIndex, targetTile.yIndex];

        if (targetPiece != null && clickedTile != null)
        {
            clickedPiece.Move(targetTile.xIndex, targetTile.yIndex, this.swapTime);
            targetPiece.Move(clickedTile.xIndex, clickedTile.yIndex, this.swapTime);

            yield return new WaitForSeconds(this.swapTime);

            List<GamePiece> clickedPieceMatches = FindMatchesAt(clickedTile.xIndex, clickedTile.yIndex);
            List<GamePiece> targetPieceMatches = FindMatchesAt(targetTile.xIndex, targetTile.yIndex);

            if (targetPieceMatches.Count == 0 && clickedPieceMatches.Count == 0)
            {
                clickedPiece.Move(clickedTile.xIndex, clickedTile.yIndex, swapTime);
                targetPiece.Move(targetTile.xIndex, targetTile.yIndex, swapTime);
            }
            else
            {
                yield return new WaitForSeconds(this.swapTime);
                ClearAndRefillBoard(clickedPieceMatches.Union(targetPieceMatches).ToList());
            }
        }
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

    private void HighlightMatches()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                HighlightMatchesAt(i, j);
            }
        }
    }

    private void HighlightPieces(List<GamePiece> gamePieces)
    {
        foreach (GamePiece piece in gamePieces)
        {
            if (piece != null)
            {
                HighlightTileOn(piece.xIndex, piece.yIndex, piece.GetComponent<SpriteRenderer>().color);
            }
        }
    }

    private void HighlightMatchesAt(int x, int y)
    {
        HighlightTileOff(x, y);
        List<GamePiece> combinedMatches = FindMatchesAt(x, y);

        if (combinedMatches.Count > 0)
        {
            foreach (GamePiece piece in combinedMatches)
            {
                HighlightTileOn(piece.xIndex, piece.yIndex, piece.GetComponent<SpriteRenderer>().color);
            }
        }
    }

    private void HighlightTileOff(int x, int y)
    {
        SpriteRenderer spriteRenderer = this.allTiles[x, y].GetComponent<SpriteRenderer>();
        spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 0);
    }

    private void HighlightTileOn(int x, int y, Color color)
    {
        SpriteRenderer spriteRenderer = this.allTiles[x, y].GetComponent<SpriteRenderer>();
        spriteRenderer.color = color;
    }

    private List<GamePiece> FindMatchesAt(int x, int y, int minLength = 3)
    {
        List<GamePiece> horizontalMatches = FindHorizontalMatches(x, y, minLength);
        List<GamePiece> verticalMatches = FindVerticalMatches(x, y, minLength);

        if (horizontalMatches == null)
        {
            horizontalMatches = new();
        }
        if (verticalMatches == null)
        {
            verticalMatches = new();
        }

        List<GamePiece> combinedMatches = horizontalMatches.Union(verticalMatches).ToList();
        return combinedMatches;
    }

    private List<GamePiece> FindMatchesAt(List<GamePiece> gamePieces, int minLength = 3)
    {
        List<GamePiece> matches = new();

        foreach (GamePiece piece in gamePieces)
        {
            matches = matches.Union(FindMatchesAt(piece.xIndex, piece.yIndex, minLength)).ToList();
        }

        return matches;
    }

    private List<GamePiece> FindVerticalMatches(int startX, int startY, int minLength = 3)
    {
        List<GamePiece> upwardMatches = FindMatches(startX, startY, new Vector2(0, 1), 2);
        List<GamePiece> downwardMatches = FindMatches(startX, startY, new Vector2(0, -1), 2);

        if (upwardMatches == null)
        {
            upwardMatches = new();
        }
        if (downwardMatches == null)
        {
            downwardMatches = new();
        }

        List<GamePiece> combinedMatches = upwardMatches.Union(downwardMatches).ToList();
        return (combinedMatches.Count >= minLength) ? combinedMatches : null;
    }

    private List<GamePiece> FindHorizontalMatches(int startX, int startY, int minLength = 3)
    {
        List<GamePiece> rightMatches = FindMatches(startX, startY, new Vector2(1, 0), 2);
        List<GamePiece> leftMatches = FindMatches(startX, startY, new Vector2(-1, 0), 2);

        if (rightMatches == null)
        {
            rightMatches = new();
        }
        if (leftMatches == null)
        {
            leftMatches = new();
        }

        List<GamePiece> combinedMatches = rightMatches.Union(leftMatches).ToList();
        return (combinedMatches.Count >= minLength) ? combinedMatches : null;
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

            if (nextPiece == null)
            {
                break;
            }
            else
            {
                if (nextPiece.matchValue == startPiece.matchValue && !matches.Contains(nextPiece))
                {
                    matches.Add(nextPiece);
                }
                else
                {
                    break;
                }
            }
        }

        if (matches.Count >= minLength)
        {
            return matches;
        }
        return null;
    }

    private void ClearBoard()
    {
        for (int i = 0; i < this.width; i++)
        {
            for (int j = 0; j < this.height; j++)
            {
                ClearPieceAt(i, j);
            }
        }
    }

    private void ClearPieceAt(int x, int y)
    {
        GamePiece pieceToClear = this.allGamePieces[x, y];

        if (pieceToClear != null)
        {
            this.allGamePieces[x, y] = null;
            Destroy(pieceToClear.gameObject);
        }

        HighlightTileOff(x, y);
    }

    private void ClearPieceAt(List<GamePiece> gamePieces)
    {
        foreach (GamePiece piece in gamePieces)
        {
            if (piece != null)
            {
                ClearPieceAt(piece.xIndex, piece.yIndex);
            }
        }
    }

    private List<GamePiece> CollapseColumn(List<GamePiece> gamePieces)
    {
        List<GamePiece> movingPieces = new();
        List<int> columnsToCollapse = GetColumns(gamePieces);

        foreach (int column in columnsToCollapse)
        {
            movingPieces = movingPieces.Union(CollapseColumn(column)).ToList();
        }

        return movingPieces;
    }

    private List<GamePiece> CollapseColumn(int column, float collapseTime = 0.1f)
    {
        List<GamePiece> movingPieces = new();

        for (int i = 0; i < this.height - 1; i++)
        {
            if (this.allGamePieces[column, i] == null)
            {
                for (int j = i + 1; j < height; j++)
                {
                    if (this.allGamePieces[column, j] != null)
                    {
                        this.allGamePieces[column, j].Move(column, i, collapseTime);
                        this.allGamePieces[column, i] = this.allGamePieces[column, j];
                        this.allGamePieces[column, i].SetCoordinates(column, i);

                        if (!movingPieces.Contains(this.allGamePieces[column, i]))
                        {
                            movingPieces.Add(this.allGamePieces[column, i]);
                        }

                        this.allGamePieces[column, j] = null;
                        break;
                    }
                }
            }
        }

        return movingPieces;
    }

    private List<int> GetColumns(List<GamePiece> gamePieces)
    {
        List<int> collumns = new();

        foreach (GamePiece piece in gamePieces)
        {
            if (!collumns.Contains(piece.xIndex))
            {
                collumns.Add(piece.xIndex);
            }
        }

        return collumns;
    }

    private void ClearAndRefillBoard(List<GamePiece> gamePieces)
    {
        StartCoroutine(ClearAndFillBoardRoutine(gamePieces));
    }

    private IEnumerator ClearAndFillBoardRoutine(List<GamePiece> gamePieces)
    {
        StartCoroutine(ClearAndCollapseRoutine(gamePieces));
        yield return null;
    }

    private IEnumerator ClearAndCollapseRoutine(List<GamePiece> gamePieces)
    {
        List<GamePiece> movingPieces = new();
        List<GamePiece> matches = new();

        HighlightPieces(gamePieces);

        yield return new WaitForSeconds(0.25f);

        bool isFinished = false;

        while (!isFinished)
        {
            ClearPieceAt(gamePieces);
            yield return new WaitForSeconds(0.25f);

            movingPieces = CollapseColumn(gamePieces);
            yield return new WaitForSeconds(0.25f);

            matches = FindMatchesAt(movingPieces);

            if (matches.Count == 0)
            {
                isFinished = true;
                break;
            }
            else
            {
                yield return StartCoroutine(ClearAndCollapseRoutine(matches));
            }
        }
        yield return null;
    }
}
