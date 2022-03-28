using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileGrid : MonoBehaviour
{
    [SerializeField] Vector2Int Dimensions;

    Tile[,] gridContent;

    List<Shape> shapes = new List<Shape>();

    private void Awake()
    {
        gridContent = new Tile[Dimensions.x, Dimensions.y];
        PopulateGrid();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public int GetGridCapacity()
    {
        return (int)gridContent.LongLength;
    } //Total 1D size of gridContent

    public int GetGridOccupancy() //How many tiles are actually in use
    {
        int count = 0;

        foreach (Tile t in gridContent)
        {
            if (t != null)
            {
                count++;
            }
        }

        return count;
    }

    public Vector2Int GetGridDimensions()
    {
        if (gridContent == null) //This should never happen, but just to be sure
        {
            Awake();
        }

        Vector2Int result = new Vector2Int(gridContent.GetLength(0), gridContent.GetLength(1));

        return result;
    }

    public Vector2Int[] OnTileClicked(int x, int y)
    {
        //A tile was clicked on the UI end
        Tile tile = GetTileAtCoord(x, y);

        if (tile == null)
        {
            Debug.Log("Could not find tile at " + x + " , " + y);
        }

        Vector2Int[] matchingTiles = null;

        //Once we've confirmed that we have a tile, we can start looking for matches
        foreach (Shape shape in shapes)
        {
            if (shape.ShapeContainsTile(tile))
            {
                matchingTiles = shape.GetTiles();
            }
        }

        if (matchingTiles == null || matchingTiles.Length < GameManager.instance.GetMinMatches())
        {
            //not enough tiles in the shape to count
            return null;
        }

        ClearTiles(matchingTiles);

        return matchingTiles;
    }

    public Vector2Int[] AdjustTiles() //Iterates through the grid, moving down ant tiles that have space below them
                                      //The pre-adjustment co-ordinates of the tiles are recorded and sent to the UI
    {
        List<Vector2Int> adjustedTiles = new List<Vector2Int>();

        //Iterate through the gridContent, starting from the bottom. We skip the bottom row as they cant go further down
        for (int y = 1; y < Dimensions.y; y++)
        {
            for (int x = 0; x < Dimensions.x; x++)
            {
                Tile tile = gridContent[x, y];


                if (tile == null) //if this point is an empty space
                {
                    continue;
                }

                Tile neighbourSouth = GetSouthernNeighbour(tile);

                if (neighbourSouth != null && neighbourSouth.currentPos.y == (tile.currentPos.y - 1)) //the tile is still directly above another tile
                {
                    continue;
                }

                if (neighbourSouth == null) //nothing was below the tile - move to y=0
                {
                    tile.MoveTile(new Vector2Int(tile.currentPos.x, 0));
                }
                else
                {
                    tile.MoveTile(new Vector2Int(neighbourSouth.currentPos.x, neighbourSouth.currentPos.y + 1));
                }

                //update gridContent about the move
                gridContent[tile.currentPos.x, tile.currentPos.y] = tile;
                gridContent[x, y] = null;

                //update shapes
                OnTileCreatedOrMoved(tile);

                //tile has been adjusted, so we add it to the list
                adjustedTiles.Add(new Vector2Int(x, y));
            }
        }

        return adjustedTiles.ToArray();
    }

    public Tile GetTileAtCoord(int _x, int _y)
    {
        Vector2Int gridSize = GetGridDimensions();

        if (_x >= gridSize.x || _y >= gridSize.y || _x < 0 || _y < 0)
        {
            return null;
        }

        return gridContent[_x,_y];
    }

    public Tile[] GetNeighbours(Tile tile)
    {
        //Format is [0] = North, [1] = East, [2] = South and [3] = West
        Tile[] neighbours = new Tile[4];
        int xPos = tile.currentPos.x;
        int yPos = tile.currentPos.y;

        neighbours[0] = GetTileAtCoord(xPos, yPos +1);
        neighbours[1] = GetTileAtCoord(xPos + 1, yPos);
        neighbours[2] = GetTileAtCoord(xPos, yPos - 1);
        neighbours[3] = GetTileAtCoord(xPos-1, yPos);

        return neighbours;
    }

    public Tile GetSouthernNeighbour(Tile tile) //returns the first tile below the given tile
    {
        for (int y = tile.currentPos.y; y-->0;)
        {
            if (gridContent[tile.currentPos.x, y] != null)
            {
                return gridContent[tile.currentPos.x, y];
            }
        }

        return null;
    }

    private void PopulateGrid()
    {
        if (gridContent == null)
        {
            return;
        }

        System.Random rnd = new System.Random();

        //For now, just populate the whole grid with tiles
        for (int y = 0; y < Dimensions.y; y++)
        {
            for (int x = 0; x < Dimensions.x; x++)
            {
                int num = rnd.Next(0, 4);
                CreateTile(x, y, num);
            }
        }
    }

    void CreateTile(int x, int y, int defaultType = -1)
    {
        //Create the tile, giving it a random colour type if not specified
        int type = defaultType;

        if (type == -1)
        {
            System.Random rnd = new System.Random();
            type = rnd.Next(0, 4);
        }
        TileType tileType = (TileType)type;

        Tile tile = new Tile(tileType, x, y);

        gridContent[x, y] = tile;

        OnTileCreatedOrMoved(tile);
    }

    void OnTileCreatedOrMoved(Tile tile)
    {
        //See if the newly created or moved tile is now adjacent to a matching shape
        // If there are no shapes yet, Congrats! You're now shape no1.
        if (shapes.Count == 0)
        {
            Shape newShape = new Shape(tile.tileType);
            shapes.Add(newShape);

            newShape.AddTileToShape(tile);
            return;
        }
        else // if there are shapes, we need to remove this tile from whichever shape it resides in.
            // it may well end up right back in the same shape, but we don't know this at runtime
        {
            Shape oldShape = FindTileInShapes(tile);
            if (oldShape != null && oldShape.RemoveTileFromShape(tile)) //returns true if shape is now empty
            {
                shapes.Remove(oldShape);
            }
        }

        // If there are shapes, we then need to check to see if our neighbours are in any shapes of matching type
        //Get Neighbours
        Tile[] neighbours = GetNeighbours(tile);

        // What we do beyond here depends on how many matching shapes we're adjacent to.
        // 0 shapes -> make a new shape as above
        // 1 shape -> add tile to that shape
        // 1 < shapes -> merge all matching adjacent shapes into 1 mega shape (assuming they aren't already)
        List<Shape> TempNeighbourList = new List<Shape>();

        foreach (Tile t in neighbours)
        {
            if (t == null)
            {
                continue;
            }
            if (t.tileType == tile.tileType)
            {
                Shape neighbourShape = FindTileInShapes(t);
                if (!TempNeighbourList.Contains(neighbourShape)) //Shapes can be (very) irregular, so we should make sure we don't add the same shape twice
                {
                    TempNeighbourList.Add(neighbourShape);
                }
            }
        }

        if (TempNeighbourList.Count == 0)
        {
            Shape newShape = new Shape(tile.tileType);
            shapes.Add(newShape);

            newShape.AddTileToShape(tile);
            return;
        }
        else if (TempNeighbourList.Count == 1)
        {
            Shape shapeToJoin = TempNeighbourList[0]; ;

            shapeToJoin.AddTileToShape(tile);
            return;
        }
        else //More than 1 matching neighbour (there should be no dupes)
        {
            //not only do we add this tile to the shape,
            //but we must also merge the additional matching shapes together
            TempNeighbourList[0].AddTileToShape(tile);
            for (int i = 1; i < TempNeighbourList.Count; i++) //Skip i=0 since that's what we merge into
            {
                TempNeighbourList[i].MergeWithShape(TempNeighbourList[0]);
                shapes.Remove(TempNeighbourList[i]);
            }

            return;
        }
    }

    Shape FindTileInShapes(Tile tileToFind)
    {
        foreach (Shape shape in shapes)
        {
            if (shape.ShapeContainsTile(tileToFind))
            {
                return shape;
            }
        }

        return null;
    }

    void ClearTiles(Vector2Int[] matches)
    {
        foreach (Vector2Int match in matches)
        {
            gridContent[match.x, match.y] = null;
        }
    }
}
