using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public class ReplayManager : MonoBehaviour {
    LevelManager levelManager;
    LevelLoader levelLoader;

    public bool useReplays = true;

    protected bool validReplayData = false;
    [System.NonSerialized]
    public LevelManager.ReplayData bestLevelReplayData; //den man har från tidigare games och som man använder för o visa replayen

    [System.NonSerialized]
    public Vector3[] currPositions = new Vector3[LevelManager.nr_ReplayMoves_Stored]; //denna man får storea i!

    bool simulate = false;
    Vector3 currMovePos;
    int currMoveIndex = 0;
    Transform moveObject;
    // Use this for initialization
    void Awake () {
        levelManager = GameObject.FindGameObjectWithTag("LevelManager").GetComponent<LevelManager>();
        levelLoader = GameObject.FindGameObjectWithTag("LevelManager").GetComponent<LevelLoader>();

        LoadReplayData(levelLoader.GetCurrLevel().levelIndex);
	}

    public void BeginSimulation()
    {
        simulate = true;
        currMoveIndex = 0;
    }
    
    void FixedUpdate()
    {
        currMovePos = currPositions[currMoveIndex];
        currMoveIndex++;

        moveObject.transform.position = currMovePos;
    }

    public void LoadReplayData(int levelIndex)
    {
        if (!useReplays) return;
        BinaryFormatter bf = new BinaryFormatter();

        string fileName = Application.persistentDataPath + "/LevelReplayData" + levelIndex.ToString() + ".dat";
        if (File.Exists(fileName))
        {
            //File.Delete(fileName); //ta bort all data där
            //continue; //ta bort all data där
            FileStream file = File.Open(fileName, FileMode.Open);
            LevelManager.ReplayData data = (LevelManager.ReplayData)bf.Deserialize(file);
            file.Close();

            int datacheck = data.datacheck;
            if (datacheck != 1337) { print("Data is corrupt"); validReplayData = false; return; }

            bestLevelReplayData.positions = data.positions;

            validReplayData = true;
        }
        else
        {
            print("File doesn't exist, so couldn't load");
            validReplayData = false;
        }
    }

    public void SaveReplayData(int levelIndex)
    {
        if (!useReplays) return;
        BinaryFormatter bf = new BinaryFormatter();

        FileStream file = File.Create(Application.persistentDataPath + "/LevelReplayData" + levelIndex.ToString() + ".dat");

        LevelManager.ReplayData data = new LevelManager.ReplayData();
        data.datacheck = 1337;
        data.positions = currPositions;

        bf.Serialize(file, data);
        file.Close();
    }

}
