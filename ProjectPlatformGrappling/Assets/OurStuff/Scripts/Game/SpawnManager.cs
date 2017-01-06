using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class SpawnManager : BaseClass {
    public int maxLives = 3;
    [HideInInspector]
    public int currLives;

    public Transform player;
    private StagMovement stagMovement;
    public WoWCCamera mainCameraS;

    private List<Transform> spawnPoints = new List<Transform>();
    private Transform closestSpawn;
    public Transform startSpawn;
    public GameObject spawnEffectObject; //som ett particlesystem
    private Transform latestSpawn;

    private bool isRespawning;

    [HideInInspector]
    public bool levelStarted = false;
    private float timePointLevelStarted = 0;
    [HideInInspector] public float timePassed = 0;
    public Text timeText;

    public Text powerGlobeText;
    PowerPickup[] powerPickups;
    private int collectedPowerGlobes = 0;
	// Use this for initialization
	void Start () {
        Init();
	}

    public override void Init()
    {
        base.Init();
        collectedPowerGlobes = 0;
        powerPickups = FindObjectsOfType(typeof(PowerPickup)) as PowerPickup[];

        GameObject[] spawnpointObjects = GameObject.FindGameObjectsWithTag("SpawnPoint");
        if(startSpawn == null)
        {
            startSpawn = spawnpointObjects[0].transform;
            latestSpawn = startSpawn;
        }

        for(int i = 0; i < spawnpointObjects.Length; i++)
        {
            spawnPoints.Add(spawnpointObjects[i].transform);
        }

        if(latestSpawn == null)
        {
            latestSpawn = spawnPoints[0];
        }

        stagMovement = player.GetComponent<StagMovement>();
        PowerGlobeCollected(0);

        Reset();
    }

    public override void Reset()
    {
        base.Reset();
        currLives = maxLives;
        isRespawning = false;

        StartSpawn();
    }

    void Update()
    {
        if(!levelStarted)
        {
            if(stagMovement.movementStacks > 5)
            {
                stagMovement.movementStacks = 5; //innan spelet har börjat så kan man inte stacka en massa
            }
            
        }

        if(Vector3.Distance(player.position, startSpawn.position) > 100 && levelStarted == false && isRespawning == false)
        {
            LevelBegin();
        }

        if(levelStarted)
        {
            timePassed = Time.time - timePointLevelStarted;
            timeText.text = timePassed.ToString("F1");
        }
    }

    void LevelBegin()
    {
        levelStarted = true;
        timePointLevelStarted = Time.time;
    }

    public void SetLatestSpawn(Transform spawn)
    {
        latestSpawn = spawn; //det senaste spawnpointet man passerar
    }

    public void StartSpawn()
    {
        if (startSpawn == null) return;
        if (isRespawning == true) return;
        
        isRespawning = true;
        stagMovement.isLocked = true;

        player.GetComponent<Rigidbody>().velocity = new Vector3(0, 0, 0);
        StartCoroutine(SpawnPlayerAtLocation(startSpawn.position, true));
    }

    public void Respawn(Vector3 playerDeathPos)
    {
        if (isRespawning == true) return;
        isRespawning = true;
        stagMovement.isLocked = true;

        //Vector3 closestSpawnPos = new Vector3(1000000000, 1000000000, 10000000000);

        //for(int i = 0; i < spawnPoints.Count; i++)
        //{
        //    if(Vector3.Distance(playerDeathPos, spawnPoints[i].position) < Vector3.Distance(playerDeathPos, closestSpawnPos))
        //    {
        //        if (spawnPoints[i].GetComponent<Spawnpoint>().isPassed)
        //        {
        //            closestSpawn = spawnPoints[i];
        //            closestSpawnPos = spawnPoints[i].position;
        //        }
        //    }
        //}

        player.GetComponent<Rigidbody>().velocity = new Vector3(0, 0, 0);
        StartCoroutine(SpawnPlayerAtLocation(latestSpawn.position, false));
        
        for (int i = 0; i < powerPickups.Length; i++)
        {
            powerPickups[i].Reset();
        }
        //player.position = closestSpawnPos;
    }

    IEnumerator SpawnPlayerAtLocation( Vector3 pos, bool instantMove)
    {
        if (!instantMove)
        {
            while (Vector3.Distance(player.position, pos) > 4.0f)
            {
                player.position = Vector3.Lerp(player.position, pos, Time.deltaTime * 4);
                yield return new WaitForEndOfFrame();
            }
        }
        else
        {
            player.position = pos;
        }
        //Vector3 forwNoY = new Vector3(mainCameraS.transform.forward.x, 0, mainCameraS.transform.forward.z);
        yield return mainCameraS.SetRot(latestSpawn.forward, false);
        player.GetComponent<PowerManager>().Reset();
        player.GetComponent<StagMovement>().Reset();
        player.GetComponent<StagShooter>().Reset();
        player.GetComponent<Rigidbody>().velocity = new Vector3(0, 0, 0);

        if(spawnEffectObject != null)
        {
            GameObject tempPar = Instantiate(spawnEffectObject, player.position, Quaternion.identity) as GameObject;
            Destroy(tempPar.gameObject, 5);
        }

        stagMovement.isLocked = false;
        isRespawning = false;

        mainCameraS.Reset(); //viktig så den låser upp kontrollen igen
    }

    public void PowerGlobeCollected(int value)
    {
        collectedPowerGlobes += value;
        powerGlobeText.text = collectedPowerGlobes.ToString() + " / " + powerPickups.Length;
    }
}
