using UnityEngine;
using System.Collections;

public class WallJumpObj : BaseClass {
    StagMovement stagMovement;
    bool isConnected = false;

    public GameObject particleHit;

    protected float stickForce = 20;
    protected float stickArea = 15;
	// Use this for initialization
	void Start () {
        Init();
	}

    //void FixedUpdate()
    //{
    //    if(Vector3.Distance(stagMovement.transform.position, transform.position) < stickArea)
    //    {
    //        Vector3 dir = (transform.position - stagMovement.transform.position).normalized;
    //        stagMovement.ApplyExternalForce(stickForce * dir);
    //    }
    //}

    public override void Init()
    {
        base.Init();
        stagMovement = GameObject.FindGameObjectWithTag("Player").GetComponent<StagMovement>();
    }

    void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.tag == "Player")
        {
            stagMovement.AddJumpsAvaible(stagMovement.jumpAmount, stagMovement.jumpAmount);
            if (particleHit != null)
            {
                GameObject tempPar = Instantiate(particleHit.gameObject);
                tempPar.transform.position = col.transform.position;
                Destroy(tempPar.gameObject, 3);
            }
        }
    }

    void OnTriggerExit(Collider col)
    {
        if (col.gameObject.tag == "Player")
        {
            if (particleHit != null)
            {
                GameObject tempPar = Instantiate(particleHit.gameObject);
                tempPar.transform.position = col.transform.position;
                Destroy(tempPar.gameObject, 3);
            }
        }
    }
}
