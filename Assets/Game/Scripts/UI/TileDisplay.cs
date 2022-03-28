using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TileDisplay : MonoBehaviour
{
    RectTransform rt;
    Image image;

    //currentCoord do not control the tiles position, and needs to be updated whenever the tile is moved
    Tile targetTile;

    GridDisplay parentGrid;
    
    // Start is called before the first frame update
    void Awake()
    {
        rt = GetComponent<RectTransform>();
        image = GetComponent<Image>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Display(Tile _tile, GridDisplay gd)
    {
        parentGrid = gd;
        targetTile = _tile;
        gameObject.SetActive(true);

        image.color = GameManager.instance.GetColor((int)targetTile.tileType);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public RectTransform GetRectTransform()
    {
        return rt;
    }

    public Vector2Int GetPosition()
    {
        return targetTile.currentPos;
    }

    public void OnButtonPress()
    {
        //Debug.Log("Tile at " + targetTile.currentPos + " was pressed");
        parentGrid.OnTileClicked(targetTile.currentPos.x, targetTile.currentPos.y);
    }
}
