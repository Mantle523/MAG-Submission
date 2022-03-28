using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile
{
    public TileType tileType { private set; get; }
    public Vector2Int currentPos { private set; get; }

    public Tile(TileType type, int x, int y)
    {
        tileType = type;
        currentPos = new Vector2Int(x, y);
    }

    public void MoveTile(Vector2Int v)
    {
        currentPos = v;
    }
}
