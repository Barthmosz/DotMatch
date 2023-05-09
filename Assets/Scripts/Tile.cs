using System.Collections;
using UnityEngine;

public enum TileType
{
    Normal,
    Obstacle,
    Breakable
}

[RequireComponent(typeof(SpriteRenderer))]
public class Tile : MonoBehaviour
{
    private Board board;
    private SpriteRenderer spriteRenderer;

    public int xIndex, yIndex, breakableValue = 0;
    public TileType tileType = TileType.Normal;
    public Sprite[] breakableSprites;
    public Color normalColor;

    private void Awake()
    {
        this.spriteRenderer = this.GetComponent<SpriteRenderer>();
    }

    public void Init(int x, int y, Board board)
    {
        xIndex = x;
        yIndex = y;
        this.board = board;

        if (this.tileType == TileType.Breakable)
        {
            if (this.breakableSprites[breakableValue] != null)
            {
                this.spriteRenderer.sprite = this.breakableSprites[breakableValue];
            }
        }
    }

    private void OnMouseDown()
    {
        if (this.board != null)
        {
            this.board.ClickTile(this);
        }
    }

    private void OnMouseEnter()
    {
        if (this.board != null)
        {
            this.board.DragToTile(this);
        }
    }

    private void OnMouseUp()
    {
        if (this.board != null)
        {
            this.board.ReleaseTile();
        }
    }

    public void BreakTile()
    {
        if (this.tileType != TileType.Breakable)
        {
            return;
        }

        StartCoroutine(BreakTileRoutine());
    }

    private IEnumerator BreakTileRoutine()
    {
        this.breakableValue--;
        this.breakableValue = Mathf.Clamp(this.breakableValue, 0, this.breakableValue);

        yield return new WaitForSeconds(0.25f);

        if (this.breakableSprites[breakableValue] != null)
        {
            this.spriteRenderer.sprite = this.breakableSprites[breakableValue];
        }

        if (this.breakableValue <= 0)
        {
            this.tileType = TileType.Normal;
            this.spriteRenderer.color = normalColor;
        }
    }
}
