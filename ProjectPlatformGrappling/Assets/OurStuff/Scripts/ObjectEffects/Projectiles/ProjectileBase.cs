using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ProjectileBase : BaseClass {
    public LayerMask targetsLM;
    protected Rigidbody o_Rigidbody;
    public string[] healTags = { "Player" };
    public string[] damageTags = { "Enemy" };
    public string[] noGoTags; //tags som inte kan bli träffade

    public GameObject particleExplosionObj;
    protected List<GameObject> particleObjectPool = new List<GameObject>();
    public int particleObjectPoolSize = 3;

    public bool isGhost = true; //går igenom objekt
    
    protected Transform shooter;
    protected float projLifeTime;
    protected Vector3 forceDirection;

    void Awake()
    {
        Init();
    }

    void Start()
    {
        Init();
    }


    public void SetShooter(Transform t)
    {
        shooter = t;
    }

    public override void Init()
    {
        if (initTimes > 0) return;
        initTimes++;

        base.Init();
        o_Rigidbody = transform.GetComponent<Rigidbody>();

        for(int i = 0; i < particleObjectPoolSize; i++)
        {
            GameObject tempPar = Instantiate(particleExplosionObj.gameObject);
            particleObjectPool.Add(tempPar);
            tempPar.SetActive(false);
        }
    }

    public override void Dealloc()
    {
        base.Dealloc();
        
        for(int i = 0; i < particleObjectPool.Count; i++)
        {
            Destroy(particleObjectPool[i]);
        }
        particleObjectPool.Clear();
    }

    public virtual void Fire( float lifeTime, Vector3 forceDir)
    {
        if (initTimes == 0) return;
        o_Rigidbody.velocity = new Vector3(0, 0, 0);
        Reset();
        transform.gameObject.SetActive(true);
        projLifeTime = lifeTime;
        forceDirection = forceDir;
        //StartCoroutine(LifeTime(lifeTime));
        //o_Rigidbody.AddForce(forceDir, ForceMode.Impulse);
    }

    public virtual IEnumerator LifeTime(float lifeTime)
    {
        yield return new WaitForSeconds(lifeTime);
        Die();
    }

    public override void Reset()
    {
        base.Reset();
        o_Rigidbody.velocity = new Vector3(0, 0, 0);
        StopAllCoroutines();
    }

    public virtual void Die()
    {
        PlayParticleSystem();
        transform.gameObject.SetActive(false);
    }

    public virtual void PlayParticleSystem()
    {
        for(int i = 0; i < particleObjectPool.Count; i++)
        {
            if(particleObjectPool[i].activeSelf == false)
            {
                particleObjectPool[i].transform.position = transform.position;
                ParticleTimed pT = particleObjectPool[i].GetComponent<ParticleTimed>();
                pT.StartParticleSystem();
                break;
            }
        }
    }
}
