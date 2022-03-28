using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField] GlobalGameSettings settings;

    public static GameManager instance;
    
    // Start is called before the first frame update
    void Awake()
    {
        instance = this;
        if (settings == null)
        {
            Debug.Log("Game started without settings loaded!");
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public Color GetColor(int type)
    {
        if (type >= settings.tileColours.Length)
        {
            return Color.white;
        }

        return settings.tileColours[type];
    }

    public int GetStartingCount()
    {
        return settings.startingCount;
    }

    public int GetMinMatches()
    {
        return settings.minAdjacents;
    }

    public void ResetPuzzle()
    {
        SceneManager.LoadScene(0);
    }
}
