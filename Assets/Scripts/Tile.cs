using System.Collections;
using System.Collections.Generic;
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
}
