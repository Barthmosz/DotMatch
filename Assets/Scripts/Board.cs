using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class Board : MonoBehaviour
{
    private Tile[,] allTiles;
    private GamePiece[,] allGamePieces;
    private Tile clickedTile;
    private Tile targetTile;
    private ParticleManager particleManager;
    private bool playerInputEnabled = true;

    public GameObject[] gamePiecePrefabs;
    public GameObject tileNormalPrefab;
    public GameObject tileObstaclePrefab;
    public int width, height, borderSize;
    public float swapTime = 0.5f;

    public StartingTile[] startingTiles;

    [Serializable]
    public class StartingTile
    {
        public GameObject tilePrefab;
        public int x, y, z;
    }

    private void Start()
    {
        this.allTiles = new Tile[width, height];
        this.allGamePieces = new GamePiece[width, height];
        particleManager = GameObject.FindWithTag("ParticleManager").GetComponent<ParticleManager>();

        SetupTiles();
        SetupCamera();
        FillBoard(10, 0.5f);
    }

    private void SetupTiles()
    {
        foreach (StartingTile tile in startingTiles)
        {
            if (tile != null)
            {
                MakeTile(tile.tilePrefab, tile.x, tile.y, tile.z);
            }
        }

        for (int i = 0; i < this.width; i++)
        {
            for (int j = 0; j < this.height; j++)
            {
                if (allTiles[i, j] == null)
                {
                    MakeTile(tileNormalPrefab, i, j);
                }
            }
        }
    }

    private void MakeTile(GameObject prefab, int x, int y, int z = 0)
    {
        if (prefab != null)
        {
            GameObject tile = Instantiate(prefab, new Vector3(x, y, z), Quaternion.identity) as GameObject;
            tile.name = $"Tile ({x},{y})";
            tile.transform.parent = this.transform;
            this.allTiles[x, y] = tile.GetComponent<Tile>();
            this.allTiles[x, y].Init(x, y, this);
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

    private void FillBoard(int falseYOffset = 0, float moveTime = 0.1f)
    {
        int maxIterations = 100;

        for (int i = 0; i < this.width; i++)
        {
            for (int j = 0; j < this.height; j++)
            {
                if (this.allGamePieces[i, j] == null && allTiles[i, j].tileType != TileType.Obstacle)
                {
                    GamePiece piece = FillRandomAt(i, j, falseYOffset, moveTime);
                    int iterations = 0;

                    while (HasMatchOnFill(i, j))
                    {
                        ClearPieceAt(i, j);
                        piece = FillRandomAt(i, j, falseYOffset, moveTime);
                        iterations++;

                        if (iterations >= maxIterations)
                        {
                            break;
                        }
                    }
                }
            }
        }
    }

    private GamePiece FillRandomAt(int x, int y, int falseYOffset = 0, float moveTime = 0.1f)
    {
        GameObject randomPiece = Instantiate(GetRandomGamePiece(), Vector3.zero, Quaternion.identity) as GameObject;

        if (randomPiece != null)
        {
            randomPiece.GetComponent<GamePiece>().Init(this);
            PlaceGamePiece(randomPiece.GetComponent<GamePiece>(), x, y);

            if (falseYOffset != 0)
            {
                randomPiece.transform.position = new Vector3(x, y + falseYOffset, 0);
                randomPiece.GetComponent<GamePiece>().Move(x, y, moveTime);
            }

            randomPiece.transform.parent = transform;
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
        if (this.playerInputEnabled)
        {
            GamePiece clickedPiece = this.allGamePieces[clickedTile.xIndex, clickedTile.yIndex];
            GamePiece targetPiece = this.allGamePieces[targetTile.xIndex, targetTile.yIndex];

            if (targetPiece != null && clickedTile != null && allTiles[clickedTile.xIndex, clickedTile.yIndex].tileType != TileType.Obstacle)
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
        if (this.allTiles[x, y].tileType != TileType.Breakable)
        {
            SpriteRenderer spriteRenderer = this.allTiles[x, y].GetComponent<SpriteRenderer>();
            spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 0);
        }
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

    private List<GamePiece> FindAllMatches()
    {
        List<GamePiece> combinedMatches = new();

        for (int i = 0; i < this.width; i++)
        {
            for (int j = 0; j < this.height; j++)
            {
                List<GamePiece> matches = FindMatchesAt(i, j);
                combinedMatches = combinedMatches.Union(matches).ToList();
            }
        }

        return combinedMatches;
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

        //HighlightTileOff(x, y);
    }

    private void ClearPieceAt(List<GamePiece> gamePieces)
    {
        foreach (GamePiece piece in gamePieces)
        {
            if (piece != null)
            {
                ClearPieceAt(piece.xIndex, piece.yIndex);
                if (particleManager != null)
                {
                    particleManager.ClearPieceFXAt(piece.xIndex, piece.yIndex);
                }
            }
        }
    }

    private void BreakTileAt(int x, int y)
    {
        Tile tileToBreak = this.allTiles[x, y];

        if (tileToBreak != null && tileToBreak.tileType == TileType.Breakable)
        {
            if (particleManager != null)
            {
                particleManager.BreakTileFXAt(tileToBreak.breakableValue, x, y);
            }

            tileToBreak.BreakTile();
        }
    }

    private void BreakTileAt(List<GamePiece> gamePieces)
    {
        foreach (GamePiece piece in gamePieces)
        {
            if (piece != null)
            {
                BreakTileAt(piece.xIndex, piece.yIndex);
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
            if (this.allGamePieces[column, i] == null && allTiles[column, i].tileType != TileType.Obstacle)
            {
                for (int j = i + 1; j < height; j++)
                {
                    if (this.allGamePieces[column, j] != null)
                    {
                        this.allGamePieces[column, j].Move(column, i, collapseTime * (j - i));
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
        StartCoroutine(ClearAndRefillBoardRoutine(gamePieces));
    }

    private IEnumerator ClearAndRefillBoardRoutine(List<GamePiece> gamePieces)
    {
        this.playerInputEnabled = false;

        List<GamePiece> matches = gamePieces;

        do
        {
            yield return StartCoroutine(ClearAndCollapseRoutine(matches));
            yield return null;

            yield return StartCoroutine(RefillRoutine());
            matches = FindAllMatches();
            yield return new WaitForSeconds(0.2f);
        }
        while (matches.Count != 0);

        this.playerInputEnabled = true;
    }

    private IEnumerator RefillRoutine()
    {
        FillBoard(10, 0.5f);
        yield return null;
    }

    private IEnumerator ClearAndCollapseRoutine(List<GamePiece> gamePieces)
    {
        List<GamePiece> movingPieces = new();
        List<GamePiece> matches = new();

        //HighlightPieces(gamePieces);
        yield return new WaitForSeconds(0.2f);

        bool isFinished = false;

        while (!isFinished)
        {
            ClearPieceAt(gamePieces);
            BreakTileAt(gamePieces);

            yield return new WaitForSeconds(0.25f);

            movingPieces = CollapseColumn(gamePieces);

            while (!IsCollapsed(movingPieces))
            {
                yield return null;
            }

            yield return new WaitForSeconds(0.2f);

            matches = FindMatchesAt(movingPieces);

            if (matches.Count == 0)
            {
                break;
            }
            else
            {
                yield return StartCoroutine(ClearAndCollapseRoutine(matches));
            }
        }
        yield return null;
    }

    private bool IsCollapsed(List<GamePiece> gamePieces)
    {
        foreach (GamePiece piece in gamePieces)
        {
            if (piece != null)
            {
                if (piece.transform.position.y - (float)piece.yIndex > 0.001f)
                {
                    return false;
                }
            }
        }

        return true;
    }
}
