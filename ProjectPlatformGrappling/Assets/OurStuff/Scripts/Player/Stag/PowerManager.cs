using UnityEngine;
using System.Collections;

[RequireComponent(typeof(StagMovement))]

public class PowerManager : BaseClass {
    private Camera activeCamera;
    public Renderer hornRenderer;
    private Renderer[] allRenderers;
    private StagMovement stagMovement;
    private CameraShaker cameraShaker;

    private float[] uvStartOffsetHorns = { 0, 1.0f};
    private float uvOffsetMult = 0.3f; //hur mkt power från hornen som ska tas bort
    public Light[] lifeLights; //fadear med powern
    public float lightsMaxIntensity = 2;

    public Material emissiveStagMaterial; //det som fadeas ut när denne tappar powernn

    private float maxPower = 1;
    [HideInInspector] public float currPower;
    [HideInInspector]
    public float powerDecay = -0.02f;

    [HideInInspector]
    public bool isAlive;
    [HideInInspector]
    public Vector3 deathLocation;
    public GameObject deathParticleSystemObj;
    public float delayedDeathTime = 0;
    public GameObject animationObj;
    public AnimationClip deathAnimation;
    // Use this for initialization
    void Start () {
        Init();
	}

    public override void Init()
    {
        base.Init();
        activeCamera = GameObject.FindGameObjectWithTag("Manager").GetComponent<CameraManager>().cameraPlayerFollow;
        stagMovement = GetComponent<StagMovement>();
        cameraShaker = activeCamera.GetComponent<CameraShaker>();

        allRenderers = GetComponentsInChildren<Renderer>();

        Reset();
        //GameObject.FindGameObjectWithTag("Manager").GetComponent<SpawnManager>().Respawn(transform.position); //viktigt denna inte ligger i reset, infinite loop annars
    }

    public override void Reset()
    {
        base.Reset();
        StopAllCoroutines();
        transform.gameObject.SetActive(true);
        
        AddPower(maxPower);
        isAlive = true;

        for(int i = 0; i < allRenderers.Length; i++)
        {
            allRenderers[i].enabled = true;
        }
    }
    // Update is called once per frame
    void Update () {
        if (isLocked) return;
        AddPower(powerDecay * Time.deltaTime);
	}

    public void AddPower(float p)
    {
        currPower += p;

        if(currPower > maxPower)
        {
            currPower = maxPower;
        }
        else if( currPower <= 0)
        {
            Die();
        }

        float offsetV = (currPower / maxPower);

        hornRenderer.material.SetTextureOffset("_MainTex", new Vector2(uvStartOffsetHorns[0], uvStartOffsetHorns[1] - (offsetV * uvOffsetMult)));
        emissiveStagMaterial.SetColor("_EmissionColor", new Color(1,1,1) * offsetV);
        for (int i = 0; i < lifeLights.Length; i++)
        {
            lifeLights[i].intensity = (lightsMaxIntensity * currPower) - 0.3f;
        }
    }

    public void AddPower(float p, float maxPercentage) //tex ger max upp till 80% av max powern
    {
        if (p > 0 && ((currPower / maxPower) * 100) > maxPercentage) return; //kolla oxå så att värdet är positivt, dvs INTE gör skada

        currPower += p;

        if (currPower > maxPower)
        {
            currPower = maxPower;
        }
        else if (currPower <= 0)
        {
            Die();
        }

        float offsetV = (currPower / maxPower);

        hornRenderer.material.SetTextureOffset("_MainTex", new Vector2(uvStartOffsetHorns[0], uvStartOffsetHorns[1] - (offsetV * uvOffsetMult)));
        emissiveStagMaterial.SetColor("_EmissionColor", new Color(1, 1, 1) * offsetV);
        for (int i = 0; i < lifeLights.Length; i++)
        {
            lifeLights[i].intensity = (lightsMaxIntensity * currPower) - 0.3f;
        }
    }

    public bool SufficentPower(float p) //kolla ifall det finns tillräkligt med power för att dra
    {
        if ((currPower + p) <= 0)
        {
            return false;
        }
        return true;
    }

    public bool SufficentPower(float p, bool cameraShake) //kolla ifall det finns tillräkligt med power för att dra, med möjlighet att få den o skaka, bra feedback till spelaren
    {
        if ((currPower + p) <= 0)
        {
            if (cameraShake)
            {
                cameraShaker.ShakeCamera(0.2f, 1, true);
            }
            return false;
        }
        return true;
    }

    public void Die()
    {
        if (isAlive == false) return; //så den inte spammar
        Debug.Log("död");
        isAlive = false;
        currPower = 0; //så att det inte blir overkill och man dör massa gånger

        stagMovement.isLocked = true; //så man inte kan styra
        stagMovement.speedBreaker.Disable();
        //stagShooter.isLocked = true;

        deathLocation = transform.position;
        //if (aiBase.GetComponent<AgentBase>() != null)
        //{
        //    aiBase.GetComponent<AgentBase>().agent.enabled = false;
        //}
        if (deathParticleSystemObj != null)
        {
            GameObject tempPar = Instantiate(deathParticleSystemObj.gameObject);
            tempPar.transform.position = transform.position;
            Destroy(tempPar, delayedDeathTime);
            //deathParticleSystemSpawned.GetComponent<ParticleTimed>().StartParticleSystem();
        }
        if (deathAnimation != null)
        {
            animationObj.GetComponent<Animation>().Play(deathAnimation.name);
        }
        else
        {
            for (int i = 0; i < allRenderers.Length; i++)
            {
                allRenderers[i].enabled = false;
            }
        }

        activeCamera.GetComponent<CameraShaker>().ShakeCamera(0.7f, 4, true, true);

        StartCoroutine(DieDelayed());
    }

    public IEnumerator DieDelayed()
    {
        yield return new WaitForSeconds(delayedDeathTime);
        
        if (transform.tag == "Player")
        {
            transform.gameObject.SetActive(false);
            GameObject.FindGameObjectWithTag("Manager").GetComponent<SpawnManager>().Respawn(deathLocation);
        }
        else
        {
            transform.gameObject.SetActive(false);
        }
    }
}
