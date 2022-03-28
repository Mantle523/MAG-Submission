using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GridDisplay : MonoBehaviour
{
    [SerializeField] Vector2 tileSpacing;
    
    [SerializeField] List<TileDisplay> tileDisplayPool; //tileDisplays not currently in use will be stroed here
    [SerializeField] GameObject tileDisplayPrefab;

    TileDisplay[,] gridContentDisplay;
    RectTransform gridRect;

    [SerializeField] TileGrid UITarget; //The grid that the UI is displaying
    
    // Start is called before the first frame update
    void Start()
    {
        if (UITarget == null)
        {
            Debug.Log("UI has no grid to target!");
            return;
        }
        
        SetupGridDisplay();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnTileClicked(int x, int y)
    {
        //Retrieves the co-ordinates of all the tiles we've determined to match
        Vector2Int[] matches = UITarget.OnTileClicked(x, y);

        if (matches == null)
        {
            return;
        }

        foreach (Vector2Int match in matches)
        {
            TileDisplay td = gridContentDisplay[match.x, match.y];
            td.Hide();

            //Add to the pool of spare tileDIsplays
            tileDisplayPool.Add(td);

            gridContentDisplay[match.x, match.y] = null;
        }

        //Retrieves the co-ordinates of tiles that need to be moved to fill the gaps
        Vector2Int[] tilesToMove = UITarget.AdjustTiles();

        //For now I'm just gonna teleport them to their new positions
        foreach (Vector2Int v in tilesToMove)
        {
            TileDisplay tile = gridContentDisplay[v.x, v.y];

            //Place the newly created tile correctly on the grid display
            RectTransform tileRectTransform = tile.GetRectTransform();
            Vector2Int newPos = tile.GetPosition();
            Vector2 posOnGrid = CoordToGridPosition(newPos.x, newPos.y);
            tileRectTransform.anchoredPosition = posOnGrid;

            //Inform gridContentDisplay of the change
            gridContentDisplay[v.x, v.y] = null;
            gridContentDisplay[newPos.x, newPos.y] = tile;
        }
    }

    void SetupGridDisplay()
    {
        gridRect = GetComponent<RectTransform>();
        
        Vector2Int dimensions = UITarget.GetGridDimensions();
        gridContentDisplay = new TileDisplay[dimensions.x, dimensions.y];

        //TileGrid was populated in Awake() so we can populate it on the UI side now
        int tilesInUse = UITarget.GetGridOccupancy();

        PopulateGridDisplay();
    }

    Vector2 CoordToGridPosition( int x, int y)
    {
        //TileGrid passes in the x,y of the tile in its gridContent
        //We translate that here into a position on the UI Display
        if (gridRect == null)
        {
            Debug.Log("Could not find RectTransform!");
            return Vector2.zero;
        }

        if (x >= gridContentDisplay.GetLength(0) || y >= gridContentDisplay.GetLength(1))
        {
            //Coordinate passed is too big
            Debug.Log("vector cannot fit on gridDisplay!");
            return Vector2.zero;
        }

        float xCap = gridContentDisplay.GetLength(0);
        float yCap = gridContentDisplay.GetLength(1);

        float xRatio = (x + 0.5f) / xCap;
        float yRatio = (y + 0.5f) / yCap;

        float xPos = xRatio * gridRect.rect.width;
        float yPos = yRatio * gridRect.rect.height;

        return new Vector2(xPos, yPos);
    }

    void PopulateGridDisplay()
    {
        int _x = gridContentDisplay.GetLength(0);
        int _y = gridContentDisplay.GetLength(1);

        //Create a tile display for each of the initial set of Tiles when the game starts
        // As this class is not in control of any data, we must assume that data provided is correct

        for (int y = 0; y < _y; y++)
        {
            for (int x = 0; x < _x; x++)
            {
                Tile t = UITarget.GetTileAtCoord(x,y);
                if (t == null)
                {
                    continue;
                }

                //Since this is the first time Tile are being added, it should be safe to just instantiate
                GameObject _TileObj = Instantiate(tileDisplayPrefab, transform);
                TileDisplay td = _TileObj.GetComponent<TileDisplay>();

                gridContentDisplay[x, y] = td;
                td.Display(t,this);

                //Place the newly created tile correctly on the grid display
                RectTransform tileRectTransform = td.GetRectTransform();
                Vector2 posOnGrid = CoordToGridPosition(x, y);
                tileRectTransform.anchoredPosition = posOnGrid;
                AdjustDisplayTileSize( tileRectTransform);


            }
        }
    }

    void AdjustDisplayTileSize(RectTransform rt)
    {
        //This function will adjust a tileDisplay's rect dimensions such that they will the maximum
        // allowed space without any overlay

        //first, width
        float xMaxSize = gridRect.rect.width / gridContentDisplay.GetLength(0);

        //the tile's width == the max width - (2* xMargin). (Left and right)
        float xSize = xMaxSize - (2 * tileSpacing.x);

        //same for height
        float yMaxSize = gridRect.rect.height / gridContentDisplay.GetLength(1);
        float ySize = yMaxSize - (2 * tileSpacing.y);

        rt.sizeDelta = new Vector2(xSize,ySize);

    }

    void AddTileFromPool(Tile tile)
    {
        if (tileDisplayPool.Count == 0)
        {
            Debug.Log("Attempted to add a tile but pool was empty");
        }

        TileDisplay pooledTileDisplay = tileDisplayPool[0];

        tileDisplayPool.Remove(pooledTileDisplay);        

        //Add it to content display
        gridContentDisplay[tile.currentPos.x, tile.currentPos.y] = pooledTileDisplay;
        pooledTileDisplay.Display(tile, this);

        //Place the newly created tile correctly on the grid display
        RectTransform tileRectTransform = pooledTileDisplay.GetRectTransform();
        Vector2 posOnGrid = CoordToGridPosition(tile.currentPos.x, tile.currentPos.y);
        tileRectTransform.anchoredPosition = posOnGrid;

    }
}
