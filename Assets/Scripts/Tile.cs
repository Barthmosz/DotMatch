using UnityEngine;

public class Tile : MonoBehaviour
{
    private Board board;

    public int xIndex, yIndex;

    public void Init(int x, int y, Board board)
    {
        xIndex = x;
        yIndex = y;
        this.board = board;
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
}
