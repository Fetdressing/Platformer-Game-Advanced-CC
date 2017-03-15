using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreManager : BaseClass {
    SpawnManager spawnManager; //den håller koll på när banan "börjat", alltså när man börjar räkna tid och hur mkt score man har förtillfället
    
    // Use this for initialization
    void Start () {
        Init();
	}

    public override void Init()
    {
        base.Init();

    }

    // Update is called once per frame
    void Update () {
		
	}

    public int GetBestScore(LevelManager lm)
    {
        //använd lm:s namn för att hitta rätt data
        return 0;
    }
}
