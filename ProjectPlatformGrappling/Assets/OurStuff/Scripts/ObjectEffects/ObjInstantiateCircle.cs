using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ObjInstantiateCircle : BaseClass { //spawnar objekt som sedan tas bort vid en viss position (eller efter en tid?)
    public GameObject o_object;
    public int poolSize = 5;
    protected List<GameObject> objPool = new List<GameObject>();

    public float spawnInterval = 5;
	// Use this for initialization
	void Start () {
        Init();
	}

    public override void Init()
    {
        base.Init();
        for(int i = 0; i < poolSize; i++)
        {
            GameObject tO = Instantiate(o_object.gameObject);
            objPool.Add(tO);
            tO.SetActive(false);
        }

        StartCoroutine(Spawn());
    }

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
                objToSpawn.transform.position = transform.position;
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
                }
            }


            yield return new WaitForSeconds(spawnInterval);
        }
    }
}
