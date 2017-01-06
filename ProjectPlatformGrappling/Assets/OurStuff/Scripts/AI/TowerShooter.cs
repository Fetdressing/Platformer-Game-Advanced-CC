using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TowerShooter : BaseClass {
    private Transform target;
    public Transform shooterObj;
    public Animation animationH;

    public AnimationClip idleA;
    public AnimationClip aggressiveA;

    public float maxShootRange = 50;
    public float shootForce = 120;
    public float cooldown_Time = 0.2f;
    private float cooldonwTimer = 0.0f;
    private float projectilePowerCost = 0.05f;
    public float offsetYAim = 1.2f;

    public GameObject projectileType;
    private int poolSize = 100;
    private List<GameObject> projectilePool = new List<GameObject>();

    public float activasionTime = 2.0f;
    private float activasionTimer = 0.0f;

    public Renderer emissionActivasionObject;
    private Material emissiveActivasionMaterial;
    // Use this for initialization
    void Start () {
        Init();
	}

    public override void Init()
    {
        base.Init();
        target = GameObject.FindGameObjectWithTag("Player").transform;
        emissiveActivasionMaterial = emissionActivasionObject.material;

        if (shooterObj == null)
        {
            shooterObj = transform;
        }

        for (int i = 0; i < poolSize; i++)
        {
            GameObject temp = Instantiate(projectileType.gameObject);
            projectilePool.Add(temp);
            temp.GetComponent<ProjectileBase>().SetShooter(transform);
            temp.SetActive(false);
        }

        Reset();
        initTimes++;
    }
    public override void Reset()
    {
        base.Reset();
        cooldonwTimer = 0.0f;
        activasionTimer = 0.0f;
        bActivated = true;

        for (int i = 0; i < projectilePool.Count; i++)
        {
            if (projectilePool[i].activeSelf == true)
            {
                projectilePool[i].SetActive(false);
            }
        }
    }
    public override void Dealloc()
    {
        base.Dealloc();
        for (int i = 0; i < projectilePool.Count; i++)
        {
            projectilePool[i].GetComponent<ProjectileBase>().Dealloc();
            Destroy(projectilePool[i]);
        }
        projectilePool.Clear();
    }
    public override void Deactivate()
    {
        base.Deactivate();
        bActivated = false;

        for (int i = 0; i < projectilePool.Count; i++)
        {
            if (projectilePool[i].activeSelf == true)
            {
                projectilePool[i].SetActive(false);
            }
        }
        
        emissiveActivasionMaterial.SetColor("_EmissionColor", new Color(1, 1, 1) * 0); //lys igång
    }

    void Update()
    {
        if (bActivated == false) return;
        if (initTimes == 0) return;
        if (Vector3.Distance(target.position, transform.position) < maxShootRange)
        {
            if(animationH != null)
                animationH.CrossFade(aggressiveA.name, activasionTime);

            if (activasionTimer < Time.time)
            {
                Fire();
            }
        }
        else
        {
            activasionTimer = Time.time + activasionTime;

            if (animationH != null)
                animationH.CrossFade(idleA.name, activasionTime);
        }

        float emissionValue = (1 - (activasionTimer - Time.time));
        emissiveActivasionMaterial.SetColor("_EmissionColor", new Color(1, 1, 1) * emissionValue); //lys igång
    }

    void Fire()
    {
        if (cooldonwTimer > Time.time) return;
        cooldonwTimer = Time.time + cooldown_Time;

        GameObject currProj = null;
        for (int i = 0; i < projectilePool.Count; i++)
        {
            if (projectilePool[i].activeSelf == false)
            {
                currProj = projectilePool[i];
                break;
            }
        }
        if (currProj == null) return;
        Rigidbody currRig = currProj.GetComponent<Rigidbody>();
        ProjectileBase currProjectile = currProj.GetComponent<ProjectileBase>();

        
        Vector3 vecToTarget = ((target.position + new Vector3(0, offsetYAim, 0)) - transform.position).normalized;

        currProj.transform.position = shooterObj.position; //kommer från en random riktning kanske?
        currProj.transform.forward = vecToTarget;
        currProjectile.Fire(2, currProjectile.transform.forward * shootForce);

    }
}
