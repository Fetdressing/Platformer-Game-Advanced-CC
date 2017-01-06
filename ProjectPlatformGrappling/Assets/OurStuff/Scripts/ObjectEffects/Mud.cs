using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Mud : BaseClass { //slöar ner och gör så att spelaren inte kan dasha (hoppa ?)
    StagMovement stagMovement;

    private float moveSpeedChange = 0.80f;
    private static float timePointEntered;

    public GameObject splashObject;
    private List<GameObject> splashPool = new List<GameObject>();
    public int poolsize = 5;
    private float splashCooldown = 0.5f;
    private float splashTimer = 0.0f;
	// Use this for initialization
	void Start () {
        Init();
	}

    public override void Init()
    {
        base.Init();
        stagMovement = GameObject.FindGameObjectWithTag("Player").GetComponent<StagMovement>();

        for(int i = 0; i < poolsize; i++)
        {
            GameObject tempO = Instantiate(splashObject);
            splashPool.Add(tempO.gameObject);
            tempO.SetActive(false);
        }

        Reset();
    }

    public override void Reset()
    {
        base.Reset();
        splashTimer = 0.0f;
        timePointEntered = -5;
    }

    void Update()
    {
        if((timePointEntered + 0.2f) > Time.time)
        {
            if(stagMovement.ySpeed > 0) //försöker hoppa
            {
                Splash(stagMovement.transform.position);
            }
            stagMovement.ySpeed = -10;
            stagMovement.dashTimePoint = Time.time;
            stagMovement.ApplySpeedMultiplier(moveSpeedChange, 0.2f);
        }
    }

    void OnTriggerStay(Collider col)
    {
        if(col.tag == "Player")
        {
            timePointEntered = Time.time;            
        }
    }

    void OnTriggerEnter(Collider col)
    {
        if (col.tag == "Player")
        {
            Splash(col.transform.position);
        }
    }

    void OnTriggerExit(Collider col)
    {
        if (col.tag == "Player")
        {
            Splash(col.transform.position);
        }
    }

    public void Splash(Vector3 pos)
    {
        if (splashPool.Count <= 0) return;
        if (splashTimer > Time.time) return;
        splashTimer = splashCooldown + Time.time;
        for(int i = 0; i < splashPool.Count; i++)
        {
            if (splashPool[i].activeSelf == false)
            {
                splashPool[i].transform.position = pos;
                splashPool[i].SetActive(true);
                splashPool[i].GetComponent<AudioSource>().Play();
                splashPool[i].GetComponent<ParticleTimed>().StartParticleSystem();
                break;
            }
        }
    }
}
