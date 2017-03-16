using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class SpawnManager : BaseClass {
    public int maxLives = 3;
    [System.NonSerialized]
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

    //rimligt att denna håller koll på tid då den hanterar när man lämnat första spawnen!
    [System.NonSerialized]
    public bool levelStarted = false;
    private float timePointLevelStarted = 0;
    [System.NonSerialized] public float timePassed = 0;
    public Text timeText;

    public Text powerGlobeText;
    PowerPickup[] powerPickups;
    HealthSpirit[] healthSpirits; //så man kan respawna fiender n stuff
    ScoreManager scoreManager;
	// Use this for initialization
	void Start () {
        Init();
	}

    public override void Init()
    {
        base.Init();
        powerPickups = FindObjectsOfType(typeof(PowerPickup)) as PowerPickup[];
        healthSpirits = FindObjectsOfType(typeof(HealthSpirit)) as HealthSpirit[];
        scoreManager = GameObject.FindGameObjectWithTag("LevelManager").GetComponent<ScoreManager>();

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
            if(stagMovement.realMovementStacks.value > 5)
            {
                stagMovement.realMovementStacks.value = 5; //innan spelet har börjat så kan man inte stacka en massa
            }
            
        }

        if(Vector3.Distance(player.position, startSpawn.position) > 100 && levelStarted == false && isRespawning == false)
        {
            LevelBegin();
        }

        if(levelStarted)
        {
            timePassed = Time.time - timePointLevelStarted;
            timeText.text = timePassed.ToString("F1"); //format
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
        //if (isRespawning == true) return;
        
        isRespawning = true;
        stagMovement.isLocked = true;

        player.GetComponent<Rigidbody>().velocity = new Vector3(0, 0, 0);
        SetLatestSpawn(startSpawn);
        StartCoroutine(SpawnPlayerAtLocation(latestSpawn.position, true));
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

        for (int i = 0; i < healthSpirits.Length; i++)
        {
            if (healthSpirits[i].respawn)
            {
                healthSpirits[i].Reset(); //respawna dem!
            }
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
        stagMovement.currMomentum = Vector3.zero;
        stagMovement.currMomentum = latestSpawn.forward * 100;
        player.GetComponent<PowerManager>().Reset();
        stagMovement.Reset();
        //player.GetComponent<StagShooter>().Reset();
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

    public void UpdateGlobesText(int g)
    {
        //scoreManager.collectedPowerGlobes += value;
        powerGlobeText.text = g + " / " + scoreManager.GetBestGlobesCollected();
    }

    public int GetMaxNrGlobes()
    {
        if(powerPickups == null)
        {
            powerPickups = FindObjectsOfType(typeof(PowerPickup)) as PowerPickup[];
        }
        return powerPickups.Length; //returnerar hur många globes som finns ute på mappen
    }
}
