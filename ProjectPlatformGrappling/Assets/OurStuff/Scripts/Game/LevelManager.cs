using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelManager : MonoBehaviour { //håller koll på alla tillgängliga levlar, väljer vilken level man vill ladda med denna
    public Level startMenuLevel;

    public Level[] standardLevels;

	// Use this for initialization
    void Start()
    {
        for (int i = 0; i < standardLevels.Length; i++) //sätter alla index här
        {
            standardLevels[i].levelIndex = i;
        }
    }

	void Awake () {
        for (int i = 0; i < standardLevels.Length; i++) //sätter alla index här
        {
            standardLevels[i].levelIndex = i;
        }
    }

    public Level GetNextLevel(Level currLevel)
    {
        int indexT = currLevel.levelIndex + 1;
        if(indexT >= standardLevels.Length)
        {
            indexT = 0;
        }
        return standardLevels[indexT];
    }
}

[System.Serializable]
public class Level
{
    public string name;
    public string loadName; //det namnet man loadar
    public Image image;
    [System.NonSerialized]
    public bool isCleared = false; //om man klarat mappen
    public bool isLocked = false; //om det är en secret map

    [System.NonSerialized]
    public int levelIndex = 0;
}
