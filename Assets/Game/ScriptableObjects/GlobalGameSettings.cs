using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Global Settings Asset", menuName = "ScriptableObjects/Global Settings", order = 1)]
public class GlobalGameSettings : ScriptableObject
{

    public Color[] tileColours;


    //public Vector2Int gridSize;
    public int minAdjacents = 3;
    public int startingCount = 72;
}
