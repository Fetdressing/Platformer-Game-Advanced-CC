﻿using UnityEngine;
using System.Collections;

public class PowerPickup : BaseClass {
    public GameObject pickUpObj;
    public bool pickUpObjFollow = true;

    protected bool isAlive = true;

    [HideInInspector]public bool hasBeenPicked = false; //resettas bara när man kör om hela lvlen
    private Rigidbody thisRigidbody;
    private Transform player;
    private StagMovement stagMovement;
    public Renderer globeRenderer;
    private Vector3 startPos;
    public float playerChaseSpeed = 0;
    public float playerChaseDistance = 140;

    public float powerWorth = 0.1f;

    public bool returnToStartPos = true;
    public bool respawn = true;
    public float respawnTime = 10;

    public string[] acceptedTags;
    public ParticleTimed particlePicked;
    public AudioSource audioSource;
    public AudioClip[] audioClips;

    public int globeValue = 1; //hur mycket "score" den är värd
    private SpawnManager spawnManager; 
    private ScoreManager scoreManager; //håller koll på hur många globes som plockats

    private Vector3 wantedPos = Vector3.zero; //sätts externaly
    private bool moveToWantedPos = false;

    private float cooldownChase = 0.0f; //jagar ej spelaren när den är på cooldownChase
    public Material takenMaterial;

    void Start () {
        Init();
	}

    void Awake()
    {
        Init();
    }

    public override void Init()
    {
        base.Init();
        spawnManager = GameObject.FindGameObjectWithTag("Manager").GetComponent<SpawnManager>();
        player = spawnManager.player;
        stagMovement = player.GetComponent<StagMovement>();
        thisRigidbody = this.transform.GetComponent<Rigidbody>();

        try {
            scoreManager = GameObject.FindGameObjectWithTag("LevelManager").GetComponent<ScoreManager>();
        }
        catch
        {
            Debug.Log("Hittade ingen scoremanager");
        }
        startPos = this.transform.position;

        if(audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        Reset();
    }

    public override void Reset()
    {
        base.Reset();
        Spawn();
    }

    //void FixedUpdate()
    //{
    //    if (playerChaseSpeed < 0.1f || thisRigidbody.isKinematic) return;

    //    if (Vector3.Distance(player.position, this.transform.position) < playerChaseDistance)
    //    {
    //        Vector3 dir = (player.position - this.transform.position).normalized;
    //        thisRigidbody.AddForce(dir * playerChaseSpeed * Time.deltaTime, ForceMode.Force);
    //        //pickUpObj.transform.position = Vector3.Lerp(pickUpObj.transform.position, player.position, Time.deltaTime * playerChaseSpeed);
    //    }
    //    else if (Vector3.Distance(startPos, this.transform.position) > (playerChaseDistance))
    //    {
    //        Vector3 dir = (startPos - this.transform.position).normalized;
    //        thisRigidbody.AddForce(dir * playerChaseSpeed * Time.deltaTime, ForceMode.Force);
    //    }
    //    else
    //    {
    //        thisRigidbody.velocity = thisRigidbody.velocity * 0.99f;
    //    }
    //}

    void FixedUpdate()
    {
        if (!pickUpObjFollow && pickUpObj.gameObject.activeSelf == false) return;
        if (isAlive == false && particlePicked.gameObject.activeSelf == false) return;
        if (Time.timeScale == 0) return;
        if (player.gameObject.activeSelf == false) return;
        if (stagMovement.isLocked) return;
        if (playerChaseSpeed < 0.1f) return;

        float distanceToPlayer = Vector3.Distance(player.position, transform.position);
        if (distanceToPlayer > playerChaseDistance * 2) return;

        if (distanceToPlayer < playerChaseDistance && cooldownChase < Time.time)
        {
            transform.position = Vector3.Lerp(transform.position, player.position + stagMovement.yMiddlePointOffset, Time.fixedDeltaTime * playerChaseSpeed / distanceToPlayer);
        }
        else if (moveToWantedPos) //en external position
        {
            if (Vector3.Distance(transform.position, wantedPos) < 0.1f)
            {
                moveToWantedPos = false;
            }

            transform.position = Vector3.Lerp(this.transform.position, wantedPos, Time.fixedDeltaTime * playerChaseSpeed * 0.1f);
        }
        else if (Vector3.Distance(startPos, transform.position) > (playerChaseDistance) && returnToStartPos)
        {
            transform.position = Vector3.Lerp(transform.position, startPos, Time.fixedDeltaTime * playerChaseSpeed);
        }
    }

    void OnTriggerEnter(Collider col)
    {
        if (pickUpObj.activeSelf == false) return;
        bool acceptedTag = false;
        for(int i = 0; i < acceptedTags.Length; i++)
        {
            if(col.transform.tag == acceptedTags[i])
            {
                acceptedTag = true;
                break;
            }
        }

        if (acceptedTag == false) return;

        if(col.transform.GetComponent<PowerManager>() != null)
        {
            col.transform.GetComponent<PowerManager>().AddPower(powerWorth);

            if(hasBeenPicked == false)
            {
                scoreManager.PowerGlobeCollected(globeValue);
            }
            hasBeenPicked = true;
            particlePicked.StartParticleSystem();
            Die();
        }
    }

    IEnumerator Respawn()
    {
        if (respawn)
        {
            yield return new WaitForSeconds(respawnTime);
            Spawn();
        }
        else
        {
            if(gameObject.activeSelf == false) //om det blivit deaktiverat så förstör det, då det inte behövs mer
            {
                Destroy(gameObject);
            }
        }
    }

    void Spawn()
    {
        thisRigidbody.velocity = new Vector3(0, 0, 0);
        this.transform.position = startPos;
        pickUpObj.gameObject.SetActive(true);
        isAlive = true;
    }

    void Die()
    {
        if(respawnTime > 0.0f)
        {
            StartCoroutine(Respawn());
        }

        if (audioClips.Length > 0)
        {
            int rand = Random.Range(0, audioClips.Length);
            audioSource.PlayOneShot(audioClips[rand]);
        }
        else
        {
            audioSource.Play();
        }

        if (takenMaterial != null && globeRenderer != null)
        {
            globeRenderer.material = takenMaterial;
        }

        pickUpObj.gameObject.SetActive(false);
        isAlive = false;
    }

    public void SetWantedPos(Vector3 pos, float cooldownChase)
    {
        wantedPos = pos;
        moveToWantedPos = true;
        cooldownChase = Time.time + cooldownChase;
    }
}
