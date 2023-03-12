using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour
{
    public int width, height;
    public GameObject tilePrefab;

    private Tile[,] allTiles;

    private void Start()
    {
        this.allTiles = new Tile[width, height];
        SetupTiles();
    }

    private void SetupTiles()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                GameObject tile = Instantiate(tilePrefab, new Vector3(i, j, 0), Quaternion.identity) as GameObject;
                tile.name = $"Tile ({i},{j})";
                this.allTiles[i, j] = tile.GetComponent<Tile>();
                tile.transform.parent = this.transform;
            }
        }
    }
}
