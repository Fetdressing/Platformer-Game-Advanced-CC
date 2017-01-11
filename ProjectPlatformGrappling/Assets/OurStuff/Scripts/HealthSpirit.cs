using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HealthSpirit : BaseClass
{
    private Transform thisTransform;
    private Rigidbody thisRigidbody;
    private AIBase aiBase;

    public Renderer[] thisRenderer; //om den inte sätts så kör den på default stuff
    private List<Material> thisMaterial = new List<Material>();
    public Material damagedMaterial;
    List<List<Material>> originalMats = new List<List<Material>>(); //så man kan återställa efter changemat
    IEnumerator changeMatIE;

    private Vector3 startPos;

    [HideInInspector]
    public Vector3 middlePoint; //var dennas mittpunkt ligger
    public float middlePointOffsetY = 0.5f;

    public int startHealth = 3;
    [HideInInspector]
    public int maxHealth; //public för att den skall kunna moddas från tex AgentStats
    private int currHealth;
    private Vector3 deathLocation; //spara ned vart denna dog, används för respawn
    public bool fadeOnDamage = true; //ska denna fadea ut när denne tappar hp?
    private float transparentValue = 1; //hur mkt denna fadeas ut

    public int startHealthRegAmount = 1;
    [HideInInspector]
    public int healthRegAmount;
    private float healthRegIntervall = 5.0f;
    private float healthRegTimer = 0.0f;

    public bool destroyOnDeath = false;
    public bool respawn = false;

    public GameObject animationObj;
    public AnimationClip deathAnimation;
    public GameObject deathParticleSystemObj;
    public float delayedDeathTime = 0;
    [HideInInspector]
    public bool isAlive = true;
    public bool isIndestructable = false;

    [Header("Drop on death")]
    public GameObject dropDeathObject;
    public int nrDropObjects = 0;
    public float dropObjLifeTime = 0; //0 så lever den förevigt

    void Start()
    {
        Init();
    }

    public override void Init()
    {
        base.Init();
        thisTransform = this.transform;
        thisRigidbody = thisTransform.GetComponent<Rigidbody>();
        startPos = thisTransform.position;
        if (thisRenderer == null)
        {
            thisRenderer = GetComponentsInChildren<Renderer>();
        }
        aiBase = thisTransform.GetComponent<AIBase>();
        int i = 0;
        foreach (Renderer re in thisRenderer)
        {
            //Debug.Log(re.material.name);
            thisMaterial.Add(re.material);
            i++;
        }

        for (int k = 0; k < thisRenderer.Length; k++)
        {
            List<Material> matT = new List<Material>();

            for (int y = 0; y < thisRenderer[k].materials.Length; y++)
            {
                matT.Add(thisRenderer[k].materials[y]);
            }
            originalMats.Add(matT);

        }

        Reset();
        initTimes++;
    }

    public override void Reset()
    {
        base.Reset();
        maxHealth = startHealth; //maxHealth kan påverkas av andra faktorer also
        isAlive = true;
        SetHealth(maxHealth);

        healthRegAmount = startHealthRegAmount;
        isAlive = true;
        thisTransform.gameObject.SetActive(true);
        transparentValue = 1;
        SetTransparency(transparentValue, false);

        thisTransform.position = startPos;

        //ApplyMaterial(damagedMaterial, 0.1f);
        ResetChangeMat();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateLoop();

    }

    public override void UpdateLoop()
    {
        if (initTimes == 0)
        {
            return;
        }
        base.UpdateLoop();

        if (isAlive == false)
        {
            if (fadeOnDamage)
            {
                transparentValue -= (Time.deltaTime * 0.00001f);
                SetTransparency(((float)currHealth / (float)maxHealth), true);
            }
            return;
        }

        middlePoint = new Vector3(thisTransform.position.x, thisTransform.position.y + middlePointOffsetY, thisTransform.position.z);

        if (healthRegTimer < Time.time)
        {
            healthRegTimer = Time.time + healthRegIntervall;
            AddHealth(healthRegAmount);
        }

        ApplyTransparency();
    }

    public virtual bool AddHealth(int h)
    {
        if (isAlive == false) return false;
        if (isIndestructable == true) return false;

        currHealth += h;
        if (h < 0.0f)
        {
            ApplyMaterial(damagedMaterial, 0.01f);
        }

        if (currHealth > maxHealth)
        {
            currHealth = maxHealth;
        }
        else if (currHealth <= 0)
        {
            if (fadeOnDamage)
            {
                SetTransparency(((float)currHealth / (float)maxHealth), true);
            }
            Die();
            return false; //target dog
            //die
        }

        if (fadeOnDamage)
        {
            SetTransparency(((float)currHealth / (float)maxHealth), true);
        }
        //Debug.Log(currHealth.ToString());
        return true; //target vid liv
    }

    public void SetHealth(int h)
    {
        currHealth = h;
        if (currHealth > maxHealth)
        {
            currHealth = maxHealth;
        }

        if (fadeOnDamage)
        {
            SetTransparency(((float)currHealth / (float)maxHealth), true);
        }

        if (currHealth <= 0)
        {
            Die();
        }
    }

    public void Die()
    {
        Debug.Log("död");
        for (int i = 0; i < thisRenderer.Length; i++)
        {
            thisRenderer[i].material = thisMaterial[i];
        }
        isAlive = false;

        BaseClass[] baseclassObj = GetComponentsInChildren<BaseClass>();
        for(int i = 0; i < baseclassObj.Length; i++)
        {
            baseclassObj[i].Deactivate();
        }

        deathLocation = transform.position;
        //if (aiBase.GetComponent<AgentBase>() != null)
        //{
        //    aiBase.GetComponent<AgentBase>().agent.enabled = false;
        //}
        if (deathParticleSystemObj != null)
        {
            GameObject deathParticleSystemSpawned = GameObject.Instantiate(deathParticleSystemObj.gameObject);
            deathParticleSystemSpawned.transform.position = middlePoint;
            deathParticleSystemSpawned.GetComponent<ParticleTimed>().StartParticleSystem();
            Destroy(deathParticleSystemSpawned, 5);
        }
        if (deathAnimation != null)
        {
            animationObj.GetComponent<Animation>().CrossFade(deathAnimation.name);
        }

        if(dropDeathObject != null && nrDropObjects > 0)
        {
            for(int i = 0; i < nrDropObjects; i++)
            {
                GameObject tempD = Instantiate(dropDeathObject);
                tempD.transform.position = transform.position + new Vector3(0, 2 + middlePointOffsetY, 0);

                PowerPickup pp = tempD.GetComponent<PowerPickup>();
                if(pp != null)
                {
                    pp.SetWantedPos(transform.position + new Vector3(Random.Range(1, 10), 0, Random.Range(1, 10)), 0.25f); //sprid drop objekten lite
                }

                if (dropObjLifeTime != 0)
                {
                    Destroy(tempD, dropObjLifeTime);
                }
            }
        }

        if (destroyOnDeath == true)
        {
            //if (thisTransform.GetComponent<AIBase>() != null)
            //{
            //    thisTransform.GetComponent<AIBase>().Dealloc();
            //}
            //Destroy(thisTransform.gameObject, delayedDeathTime);
        }
        else
        {
            StartCoroutine(DieDelayed());
        }
    }

    public IEnumerator DieDelayed()
    {
        yield return new WaitForSeconds(delayedDeathTime);
        thisTransform.gameObject.SetActive(false);
        if (transform.tag == "Player")
        {
            GameObject.FindGameObjectWithTag("Manager").GetComponent<SpawnManager>().Respawn(deathLocation);
        }
    }

    public bool IsAlive()
    {
        if (currHealth > 0 && isAlive == true)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public int GetCurrHealth()
    {
        return currHealth;
    }

    public void SetTransparency(float tra, bool limit)
    {
        if(limit)
            transparentValue = Mathf.Clamp(tra, 0.2f, 1); //min värde
        else
        {
            transparentValue = tra;
        }
        ApplyTransparency();
    }

    public float GetTransparency()
    {
        return transparentValue;
    }

    void ApplyTransparency()
    {
        for (int i = 0; i < thisMaterial.Count; i++)
        {
            Color c = thisMaterial[i].color;
            thisMaterial[i].color = new Color(c.r, c.g, c.b, transparentValue);
        }

        //for (int i = 0; i < thisRenderer.Length; i++)
        //{
        //    Color c = thisRenderer[i].material.color;
        //    thisRenderer[i].material.color = new Color(c.r, c.g, c.b, tVal);
        //}
    }

    public void ApplyMaterial(Material m, float time)
    {
        if (thisTransform.gameObject.activeSelf == false) return;

        if (changeMatIE != null) return;

        changeMatIE = MarkMaterial(m, time);

        StartCoroutine(changeMatIE);
    }

    public IEnumerator MarkMaterial(Material m, float time)
    {
        for (int i = 0; i < thisRenderer.Length; i++)
        {
            Material[] matsSetTemp = thisRenderer[i].materials; //temporär så att man ska kunna sätta allRenderers[i].materials till ett värde

            for (int y = 0; y < thisRenderer[i].materials.Length; y++)
            {
                matsSetTemp[y] = m;
            }
            thisRenderer[i].materials = matsSetTemp;

        }
        yield return new WaitForSeconds(time);
        for (int i = 0; i < thisRenderer.Length; i++)
        {
            Material[] matsSetTemp = thisRenderer[i].materials; //temporär så att man ska kunna sätta allRenderers[i].materials till ett värde
            for (int y = 0; y < originalMats[i].Count; y++)
            {
                matsSetTemp[y] = originalMats[i][y];
            }
            thisRenderer[i].materials = matsSetTemp;
        }
        changeMatIE = null;
    }

    public void ResetChangeMat()
    {
        if (originalMats.Count == 0) return;
        for (int i = 0; i < thisRenderer.Length; i++)
        {
            Material[] matsSetTemp = thisRenderer[i].materials; //temporär så att man ska kunna sätta allRenderers[i].materials till ett värde
            for (int y = 0; y < originalMats[i].Count; y++)
            {
                matsSetTemp[y] = originalMats[i][y];
            }
            thisRenderer[i].materials = matsSetTemp;
        }
    }

    //public IEnumerator MarkMaterial(Material m, float time)
    //{
    //    //thisRenderer.material = m;
    //    List<List<Material>> mats = new List<List<Material>>();

    //    for (int i = 0; i < thisRenderer.Length; i++)
    //    {
    //        List<Material> matT = new List<Material>();
    //        Material[] matsSetTemp = thisRenderer[i].materials; //temporär så att man ska kunna sätta thisRenderer[i].materials till ett värde

    //        for (int y = 0; y < thisRenderer[i].materials.Length; y++)
    //        {
    //            matT.Add(thisRenderer[i].materials[y]);
    //            matsSetTemp[y] = m;
    //        }
    //        thisRenderer[i].materials = matsSetTemp;
    //        mats.Add(matT);

    //    }
    //    yield return new WaitForSeconds(time);
    //    for (int i = 0; i < thisRenderer.Length; i++)
    //    {
    //        Material[] matsSetTemp = thisRenderer[i].materials; //temporär så att man ska kunna sätta thisRenderer[i].materials till ett värde
    //        for (int y = 0; y < mats[i].Count; y++)
    //        {
    //            matsSetTemp[y] = mats[i][y];
    //        }
    //        thisRenderer[i].materials = matsSetTemp;
    //    }
    //    changeMatIE = null;
    //}

    void OnCollisionEnter(Collision collision)
    {
        ////kolla så att man inte collidar med sig själv
        ////if(collision.)
        //float speed = collision.relativeVelocity.magnitude;
        //float ySpeed = Mathf.Abs(collision.relativeVelocity.y);

        //if (speed > speedDamageThreshhold)
        //{
        //    AddHealth(Mathf.Min(0, -(int)(speed - speedDamageThreshhold) + -(int)(ySpeed * 0.8f)));

        //    if (aiBase != null)
        //    {
        //        aiBase.ReportAttacked(collision.transform);
        //    }
        //}
    }

    void OnTriggerEnter(Collider col)
    {
        //return;
        //float speed = Vector3.Magnitude(col.transform.GetComponent<Rigidbody>().velocity - thisRigidbody.velocity);

        //if (speed > speedDamageThreshhold)
        //{
        //    AddHealth(Mathf.Min(0, -(int)(speed - speedDamageThreshhold)));

        //    if (aiBase != null)
        //    {
        //        aiBase.ReportAttacked(col.transform);
        //    }
        //}
    }

}

