using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public class LevelManager : MonoBehaviour { //håller koll på alla tillgängliga levlar, väljer vilken level man vill ladda med denna
    public static LevelManager levelManager; //singleton

    public Level startMenuLevel;
    public Level[] standardLevels;

    public const int nr_ReplayMoves_Stored = 1000000; //för replay, hur många "frames" som ska storeas

	void Awake () { ///detta kanske behöver kallas varje gång LevelLoader loadar en ny level istället

        if (levelManager == null) //singleton
        {
            DontDestroyOnLoad(transform);
            levelManager = this;
        }
        else if (levelManager != this)
        {
            Destroy(this.gameObject);
        }

        for (int i = 0; i < standardLevels.Length; i++) //sätter alla index här
        {
            standardLevels[i].levelIndex = i; //viktigt att dessa index inte ändras nånstans
        }

        LoadLevelsData();
    }

    public Level GetNextLevel(Level currLevel) //används av LevelLoader
    {
        int indexT = currLevel.levelIndex + 1;
        if(indexT >= standardLevels.Length)
        {
            indexT = 0;
        }
        return standardLevels[indexT];
    }

    public Level GetLevel(int index) //gets level by index!
    {
        if(index >= standardLevels.Length) { print("Invalid level index"); return new Level(); }
        return standardLevels[index];
    }

    public void SetDataLevel(int indexLevel, Level newData) //ändrar datan för en level
    {
        Level lvTemp = new Level(newData); //vill inte ha någon referens, bara datan
        standardLevels[indexLevel] = lvTemp;
        SaveLevelsData(); //sparar all data, onödigt kanske men just to be safe
    }

    public void LoadLevelsData()
    {
        BinaryFormatter bf = new BinaryFormatter();

        for (int i = 0; i < standardLevels.Length; i++)
        {
            string fileName = Application.persistentDataPath + "/LevelI" + i.ToString() + ".dat";
            if (File.Exists(fileName))
            {
                //File.Delete(fileName); //ta bort all data där
                //continue; //ta bort all data där
                FileStream file = File.Open(fileName, FileMode.Open);
                LevelData data = (LevelData)bf.Deserialize(file);
                file.Close();

                int datacheck = data.datacheck;
                if (datacheck != 1337) { print("Data is corrupt"); return; }

                standardLevels[i].isCleared = data.isCleared;
                standardLevels[i].isLocked = data.isLocked;
                standardLevels[i].bestTime = data.bestTime;
                standardLevels[i].bestGlobesCollected = data.bestGlobesCollected;
            }
            else
            {
                print("File doesn't exist, so couldn't load");
            }
        }
    }

    public void SaveLevelsData()
    {
        BinaryFormatter bf = new BinaryFormatter();

        for (int i = 0; i < standardLevels.Length; i++)
        {
            FileStream file = File.Create(Application.persistentDataPath + "/LevelI" + i.ToString() + ".dat");

            LevelData data = new LevelData();
            data.datacheck = 1337;
            data.isCleared = standardLevels[i].isCleared;
            data.isLocked = standardLevels[i].isLocked;
            data.bestTime = standardLevels[i].bestTime;
            data.bestGlobesCollected = standardLevels[i].bestGlobesCollected;

            bf.Serialize(file, data);
            file.Close();
        }
    }


    public class ReplayData
    {
        public int datacheck = 0; //när jag läser så ska denna kollas hurvida den är 1337 eller inte, annars är datan korrupt eller inte fixad
        public Vector3[] positions = new Vector3[nr_ReplayMoves_Stored];
    }
}

[Serializable]
class LevelData
{
    public int datacheck = 0; //när jag läser så ska denna kollas hurvida den är 1337 eller inte, annars är datan korrupt eller inte fixad
    public bool isCleared;
    public bool isLocked;
    public float bestTime;
    public int bestGlobesCollected;
}

[System.Serializable]
public class Level
{
    public Level() { }
    public Level(Level copyLevel)
    {
        name = copyLevel.name;
        loadName = copyLevel.loadName;
        image = copyLevel.image;
        levelIndex = copyLevel.levelIndex;

        isCleared = copyLevel.isCleared;
        isLocked = copyLevel.isLocked;
        bestTime = copyLevel.bestTime;
        bestGlobesCollected = copyLevel.bestGlobesCollected;
    }

    public string name;
    public string loadName; //det namnet man loadar
    public Sprite image;

    [System.NonSerialized]
    public int levelIndex = 0;


    //förändrande värden
    [System.NonSerialized]
    public bool isCleared = false; //om man klarat mappen
    public bool isLocked = false; //om det är en secret map, den behöver ett startvärde då man vill kunna sätta maps som locked från start
    [System.NonSerialized]
    public float bestTime = Mathf.Infinity; //vilken ens bästa tid har varit på denna map
    [System.NonSerialized]
    public int bestGlobesCollected = 0;
}
