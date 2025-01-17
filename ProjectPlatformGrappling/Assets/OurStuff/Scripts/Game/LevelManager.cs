﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public class LevelManager : MonoBehaviour { //håller koll på alla tillgängliga levlar, väljer vilken level man vill ladda med denna
    public bool removeAllData = false; //rensa all sparad data

    public static LevelManager levelManager; //singleton
    public static LevelLoader levelLoader;

    public Level startMenuLevel;
    public Level[] standardLevels;
    
    ReplayManager replayManager;
    public int Nr_ReplayMoves_Stored
    {
        get { return nr_ReplayMoves_Stored; }
    }
    public const int nr_ReplayMoves_Stored = 1000000; //för replay, hur många "frames" som MAX ska storeas

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

        replayManager = GetComponent<ReplayManager>();
        levelLoader = GetComponent<LevelLoader>();

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
                if (removeAllData)
                {
                    File.Delete(fileName); //ta bort all data där
                    continue; //ta bort all data där
                }
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

        replayManager.SaveReplayData(levelLoader.GetCurrLevel().levelIndex); //sparar replay datan, låt den sköta själv ifall den vill spara över eller inte
    }

}

[Serializable]
public class ReplayData
{
    public ReplayData(int nrPos)
    {
        positions = new Vector3Serializer[nrPos];
        nrPositions = nrPos;
    }

    public int datacheck = 0; //när jag läser så ska denna kollas hurvida den är 1337 eller inte, annars är datan korrupt eller inte fixad
    public int nrPositions;
    public Vector3Serializer[] positions;
}

[Serializable]
public class Vector3Serializer
{
    public float x = 0;
    public float y = 0;
    public float z = 0;

    public void Fill(Vector3 v3)
    {
        x = v3.x;
        y = v3.y;
        z = v3.z;
    }

    public Vector3 V3 { get { return new Vector3(x, y, z); } set { Fill(value); } }


    public Vector3Serializer()
    {
        x = 0;
        y = 0;
        z = 0;
    }
    public Vector3Serializer(float xn, float yn, float zn)
    {
        x = xn;
        y = yn;
        z = zn;
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
