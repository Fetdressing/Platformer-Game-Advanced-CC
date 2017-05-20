using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DigitalRuby.SimpleLUT;

[RequireComponent(typeof(StagMovement))]

public class PowerManager : BaseClass {
    public bool godMode = false;
    private Camera activeCamera;
    public Renderer hornRenderer;
    public Renderer stagRenderer;
    private Renderer[] allRenderers;
    private StagMovement stagMovement;
    private CameraShaker cameraShaker;
    private CharacterController cController;

    private SimpleLUT sLut;
    private float startSaturation = 0; //man ändrar saturationen när man tappar mkt power
    private float wantedSaturation;

    private float[] uvStartOffsetHorns = { 0, 1.0f};
    private float uvOffsetMult = 0.3f; //hur mkt power från hornen som ska tas bort
    public Light[] lifeLights; //fadear med powern
    public float lightsMaxIntensity = 2;

    public Material emissiveStagMaterial; //det som fadeas ut när denne tappar powernn
    protected Material emiStagMat; //den materialet som moddas (istället för emissiveStagMaterial så man slipper gitta ändringarna)
    public Renderer emissiveStagRenderer;
    public Material damagedMaterial;
    List<List<Material>> originalMats = new List<List<Material>>(); //så man kan återställa efter changemat
    public Renderer[] changeMatRenderers;
    IEnumerator changeMatIE;

    private float maxPower = 1;
    [System.NonSerialized] public float currPower;
    [System.NonSerialized]
    public float powerDecay = -0.05f;

    [System.NonSerialized]
    public bool isAlive;
    [System.NonSerialized]
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
        sLut = activeCamera.GetComponent<SimpleLUT>();
        cController = GetComponent<CharacterController>();

        emiStagMat = new Material(emissiveStagMaterial);
        emiStagMat.CopyPropertiesFromMaterial(emissiveStagMaterial);
        stagRenderer.material = emiStagMat; //ger den det nya temp materialet så jag inte moddar originalet
        allRenderers = GetComponentsInChildren<Renderer>();

        wantedSaturation = startSaturation;

        for (int i = 0; i < changeMatRenderers.Length; i++)
        {
            List<Material> matT = new List<Material>();

            for (int y = 0; y < changeMatRenderers[i].materials.Length; y++)
            {
                matT.Add(changeMatRenderers[i].materials[y]);
            }
            originalMats.Add(matT);

        }

        Reset();
        //GameObject.FindGameObjectWithTag("Manager").GetComponent<SpawnManager>().Respawn(transform.position); //viktigt denna inte ligger i reset, infinite loop annars
    }

    public override void Reset()
    {
        base.Reset();
        StopAllCoroutines();
        cController.enabled = true;
        transform.gameObject.SetActive(true);
        
        AddPower(maxPower);
        isAlive = true;

        for(int i = 0; i < allRenderers.Length; i++)
        {
            allRenderers[i].enabled = true;
        }

        ResetChangeMat(); //kan vara risky
    }
    // Update is called once per frame
    void Update () {
        if (isLocked) return;
        AddPower(powerDecay * Time.deltaTime);
        LerpToWantedSaturation();

        if(godMode)
        {
            stagMovement.dashUsed = false;
            stagMovement.jumpAmount = 1000;
        }
	}

    public void AddPower(float p, bool showDamageMat = false)
    {
        if(godMode)
        {
            p = Mathf.Abs(p);
        }
        
        currPower += p;

        if (showDamageMat)
        {
            if (damagedMaterial != null && p < 0.0f)
            {
                ApplyMaterial(damagedMaterial, 0.1f);
            }
        }

        if (currPower > maxPower)
        {
            currPower = maxPower;
        }
        else if( currPower <= 0)
        {
            Die();
        }

        float currPowerPer = (currPower / maxPower);
        CalculateSaturation(currPowerPer);

        CalculateLight(currPowerPer);
    }

    public void AddPower(float p, float maxPercentage, float minPercentage = 0, bool showDamageMat = false) //tex ger max upp till 80% av max powern
    {
        float wantedMaxPower = maxPower * maxPercentage * 0.01f;
        float wantedMinPower = maxPower * minPercentage * 0.01f;
        if (currPower > wantedMaxPower || currPower < wantedMinPower) //redan över gränsen
        {
            return;
        }

        if ((currPower + p) > wantedMaxPower) //kommer hamna över gränsen om jag lägger på p, reducera den så att man hamnar jämnt!
        {
            p = wantedMaxPower - currPower;
        }
        else if((currPower + p) < wantedMinPower) //negativ här
        {
            p = wantedMinPower - currPower;
        }
        //if (p < 0) return; //kolla oxå så att värdet är positivt, dvs INTE gör skada

        AddPower(p, showDamageMat);
    }

    public void AddPowerPercentage(float p, bool showDamageMat = false) //i decimaltal
    {
        float powP = p * maxPower;
        AddPower(powP, showDamageMat);
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
        stagMovement.AddMovementStack(-(int)(stagMovement.realMovementStacks.value * 0.3f)); //tappar för man dör, den updateras inte dirr i texten dock ifall du undrar varför det inte försvinner dirr :)
        cController.enabled = false;
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


    public void ApplyMaterial(Material m, float time)
    {
        if (transform.gameObject.activeSelf == false) return;

        if (changeMatIE != null) return;

        changeMatIE = MarkMaterial(m, time);

        StartCoroutine(changeMatIE);
    }

    public IEnumerator MarkMaterial(Material m, float time)
    {
        //allRenderers.material = m;


        //for (int i = 0; i < changeMatRenderers.Length; i++)
        //{
        //    Material[] matsSetTemp = changeMatRenderers[i].materials; //temporär så att man ska kunna sätta allRenderers[i].materials till ett värde

        //    for (int y = 0; y < changeMatRenderers[i].materials.Length; y++)
        //    {
        //        matsSetTemp[y] = m;
        //    }
        //    changeMatRenderers[i].materials = matsSetTemp;

        //}
        emissiveStagRenderer.material = m;

        yield return new WaitForSeconds(time);
        emissiveStagRenderer.material = emissiveStagMaterial;
        //for (int i = 0; i < changeMatRenderers.Length; i++)
        //{
        //    Material[] matsSetTemp = changeMatRenderers[i].materials; //temporär så att man ska kunna sätta allRenderers[i].materials till ett värde
        //    for (int y = 0; y < originalMats[i].Count; y++)
        //    {
        //        matsSetTemp[y] = originalMats[i][y];
        //    }
        //    changeMatRenderers[i].materials = matsSetTemp;
        //}
        changeMatIE = null;
    }

    public void ResetChangeMat()
    {
        emissiveStagRenderer.material = emissiveStagMaterial;
        //if (originalMats.Count == 0) return;
        //for (int i = 0; i < changeMatRenderers.Length; i++)
        //{
        //    Material[] matsSetTemp = changeMatRenderers[i].materials; //temporär så att man ska kunna sätta allRenderers[i].materials till ett värde
        //    for (int y = 0; y < originalMats[i].Count; y++)
        //    {
        //        matsSetTemp[y] = originalMats[i][y];
        //    }
        //    changeMatRenderers[i].materials = matsSetTemp;
        //}
    }

    void CalculateSaturation(float f) //hur mycket procent power man har kvar
    {
        //float bSatVal = 0.8f; //hur mycket den ska utgå ifrån
        float perThreshold = 0.35f; //under detta värde på f så kommer det att bli lägre saturation
        float satValue = 0;
        float satDecreaseMult = 2.9f;

        if (f < perThreshold)
        {
            satValue = perThreshold - f;
        }

        wantedSaturation = startSaturation - satValue * satDecreaseMult;
        //sLut.Saturation = startSaturation - satValue * satDecreaseMult;
    }

    void CalculateLight(float f)
    {
        hornRenderer.material.SetTextureOffset("_MainTex", new Vector2(uvStartOffsetHorns[0], uvStartOffsetHorns[1] - (f * uvOffsetMult)));
        emiStagMat.SetColor("_EmissionColor", new Color(1, 1, 1) * f);
        for (int i = 0; i < lifeLights.Length; i++)
        {
            lifeLights[i].intensity = (lightsMaxIntensity * f) - 0.3f;
        }
        if (changeMatIE == null)
        {
            stagRenderer.material = emiStagMat;
        }
    }

    void LerpToWantedSaturation()
    {
        float lerpSpeed = 1.2f;
        sLut.Saturation = Mathf.Lerp(sLut.Saturation, wantedSaturation, Time.deltaTime * lerpSpeed);
    }
}
