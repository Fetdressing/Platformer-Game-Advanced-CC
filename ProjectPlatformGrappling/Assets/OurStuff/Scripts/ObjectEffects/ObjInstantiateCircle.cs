using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ObjInstantiateCircle : BaseClass { //spawnar objekt som sedan tas bort vid en viss position (eller efter en tid?)
    public Transform spawnLocation;
    public GameObject o_object;
    public int poolSize = 5;
    protected List<GameObject> objPool = new List<GameObject>();

    public float spawnInterval = 5;

    public Animation animH;
    public float animSpeed = 1.2f;
    public AnimationClip idleClip;
    public AnimationClip spawnClip;
	// Use this for initialization
	void Start () {
        Init();
	}

    public override void Init()
    {
        base.Init();

        if(spawnLocation == null)
        {
            spawnLocation = transform;
        }

        for(int i = 0; i < poolSize; i++)
        {
            GameObject tO = Instantiate(o_object.gameObject);
            objPool.Add(tO);
            tO.SetActive(false);
        }

        foreach (AnimationState state in animH)
        {
            state.speed = animSpeed;
        }

        StartCoroutine(Spawn());
    }

    //void Update()
    //{
    //    if(animH != null && idleClip != null)
    //    {
    //        if(animH.IsPlaying(spawnClip.name) == false)
    //        {
    //            animH.CrossFade(idleClip.name);
    //        }
    //    }
    //}

    IEnumerator Spawn()
    {
        while(this != null)
        {
            GameObject objToSpawn = null;
            for(int i = 0; i < objPool.Count; i++)
            {
                if(objPool[i].gameObject.activeSelf == false)
                {
                    objToSpawn = objPool[i].gameObject;
                    break;
                }
            }

            if (objToSpawn != null)
            {
                if (animH != null)
                {
                    animH.CrossFade(spawnClip.name);
                    yield return new WaitForSeconds((spawnClip.length * animSpeed) * 0.8f);
                }

                objToSpawn.transform.position = spawnLocation.position;
                objToSpawn.SetActive(true);

                BreakerObject breakObj = objToSpawn.GetComponent<BreakerObject>();
                if (breakObj != null)
                {
                    breakObj.ReturnInInstant();
                }

                Rigidbody o_Rigidbody = objToSpawn.GetComponent<Rigidbody>();
                if(o_Rigidbody != null)
                {
                    o_Rigidbody.velocity = Vector3.zero;
                    o_Rigidbody.angularVelocity = Vector3.zero;
                }
            }

            if (animH != null && idleClip != null)
            {
                animH.CrossFade(idleClip.name);
            }

            yield return new WaitForSeconds(spawnInterval);
        }
    }
}
