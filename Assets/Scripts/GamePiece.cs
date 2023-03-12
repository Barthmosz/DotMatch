using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GamePiece : MonoBehaviour
{
    public int xIndex, yIndex;

    public void SetCoordinates(int x, int y)
    {
        this.xIndex = x;
        this.yIndex = y;
    }
}
