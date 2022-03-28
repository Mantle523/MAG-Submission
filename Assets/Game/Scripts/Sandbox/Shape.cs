using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shape
{
    //A shape in this context is a group of same type adjacent tiles
    //As tiles are added to the grid we check to see if they fit in any adjacent shapes
    public TileType tileType { private set; get; } 

    List<Tile> shapeContent = new List<Tile>();

    public Shape(TileType type)
    {
        tileType = type;
    }

    public void AddTileToShape(Tile tile)
    {
        shapeContent.Add(tile);
    }

    public bool ShapeContainsTile(Tile tile)
    {
        if (shapeContent.Contains(tile))
        {
            return true;
        }
        return false;
    }

    private protected void MergeContentIntoThisShape(List<Tile> content)
    {
        shapeContent.AddRange(content);
    }

    public void MergeWithShape(Shape shape)
    {
        shape.MergeContentIntoThisShape(shapeContent);
        shapeContent.Clear();
    }

    public bool RemoveTileFromShape(Tile t) //Removes tile t from the shape. If the shape is now empty, returns true
    {
        shapeContent.Remove(t);
        if (shapeContent.Count == 0)
        {
            return true;
        }
        return false;
    }

    public Vector2Int[] GetTiles()
    {
        //Get the positions of all of the tiles in the shape.
        //We do it in this format to easily pass it to both the front and backend
        List<Vector2Int> CoOrdList = new List<Vector2Int>(); 

        foreach (Tile tile in shapeContent)
        {
            CoOrdList.Add(tile.currentPos);
        }

        return CoOrdList.ToArray();
    }

}
