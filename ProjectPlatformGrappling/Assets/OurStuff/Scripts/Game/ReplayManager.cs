using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public class ReplayManager : BaseClass {
    LevelManager levelManager;
    LevelLoader levelLoader;

    public bool useReplays = true;

    protected bool validReplayData = false;

    [System.NonSerialized]
    public ReplayData bestLevelReplayData; //den man har från tidigare games och som man använder för o visa replayen
    [System.NonSerialized]
    public List<Vector3Serializer> currPositions = new List<Vector3Serializer>(); //denna man får storea i!

    bool simulate = false;
    int everyXUpdate = 8; //hur ofta den ska hämta data från spelaren
    int currUpdateNumber = 0;
    Vector3 currMovePos = Vector3.zero;
    public Transform replayObject;
    Transform player;
    // Use this for initialization
    void Awake () {
        levelManager = GameObject.FindGameObjectWithTag("LevelManager").GetComponent<LevelManager>();
        levelLoader = GameObject.FindGameObjectWithTag("LevelManager").GetComponent<LevelLoader>();

        bestLevelReplayData = new ReplayData(levelManager.Nr_ReplayMoves_Stored);

        //LoadReplayData(levelLoader.GetCurrLevel().levelIndex);

        simulate = false;
	}

    public void BeginSimulation(Transform playerT)
    {
        LoadReplayData(levelLoader.GetCurrLevel().levelIndex);
        player = playerT;
        replayObject.transform.position = player.position;

        simulate = true;
        currUpdateNumber = 0;
    }

    public void EndSimulation()
    {
        simulate = false;
        currUpdateNumber = 0;
    }

    void FixedUpdate()
    {
        if (!useReplays) return;
        if (!simulate) return;

        if (currUpdateNumber % everyXUpdate != 0) { currUpdateNumber++; return; } //så att man inte köra varje update
        else { currUpdateNumber++; }

        int currMoveIndex = (currUpdateNumber/ everyXUpdate) - 1;
        currMoveIndex = Mathf.Max(0, currMoveIndex);
        if (currMoveIndex < bestLevelReplayData.positions.Length)
        {
            if (bestLevelReplayData.positions[currMoveIndex] != null)
            {
                currMovePos = bestLevelReplayData.positions[currMoveIndex].V3;

                replayObject.gameObject.SetActive(true);
            }
            else
            {
                replayObject.gameObject.SetActive(false); //finns ingen mer rekordad data
            }
        }

        if (currMoveIndex < levelManager.Nr_ReplayMoves_Stored)
        {
            Vector3Serializer v = new Vector3Serializer(player.position.x, player.position.y, player.position.z);
            currPositions.Add(v);
            //currPositions[currMoveIndex] = v;
        }
        else
        {
            EndSimulation(); //det blev för många helt enkelt, du är för noob för att ha replay :'(
        }
    }

    void Update()
    {
        if (!useReplays) return;
        if (!simulate) return;

        float lerpSpeed = 10; //behöver bara vara snabbare fixedupdaten
        replayObject.position = Vector3.Lerp(replayObject.position, currMovePos, Time.deltaTime * lerpSpeed);

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
            ReplayData data = Deserialize(file);
            //ReplayData data = (ReplayData)bf.Deserialize(file);
            file.Close();

            int datacheck = data.datacheck;
            print(datacheck);
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

        ReplayData data = new ReplayData(currPositions.Count);
        data.datacheck = 1337;
        data.nrPositions = currPositions.Count;

        for(int i = 0; i < currPositions.Count; i++)
        {
            data.positions[i] = currPositions[i];
        }
        //data.positions = currPositions;

        Serialize(file, data);

        //bf.Serialize(file, data);
        file.Close();
    }


    public void Serialize(Stream stream, ReplayData replayD)
    {
        BinaryWriter sw = new BinaryWriter(stream);
        sw.Write(replayD.datacheck);
        sw.Write(replayD.nrPositions);

        foreach (Vector3Serializer vec in replayD.positions)
        {
            if (vec == null) {  break; }
            sw.Write(vec.x);
            sw.Write(vec.y);
            sw.Write(vec.z);
        }
    }

    public ReplayData Deserialize(Stream stream)
    {
        BinaryReader br = new BinaryReader(stream);

        int datacheck = br.ReadInt32();
        if(datacheck != 1337)
        {
            print("Data is corrupt");
        }
        int nrPos = br.ReadInt32();

        ReplayData replayD = new ReplayData(nrPos);

        replayD.datacheck = datacheck;
        replayD.nrPositions = nrPos;

        for (int i = 0; i < replayD.positions.Length; i++)
        {
            float x = br.ReadSingle();
            float y = br.ReadSingle();
            float z = br.ReadSingle();

            Vector3 ve = new Vector3(x, y, z);
            Vector3Serializer vec = new Vector3Serializer();
            vec.Fill(ve);

            replayD.positions[i] = vec;
        }

        return replayD;
    }

}
