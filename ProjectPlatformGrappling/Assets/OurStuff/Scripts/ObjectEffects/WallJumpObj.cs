using UnityEngine;
using System.Collections;

public class WallJumpObj : BaseClass {
    StagMovement stagMovement;
    bool isConnected = false;

    public GameObject particleHit;

    protected float maxMomentumPer = 0.8f;
    protected float stickTimer = 0.0f;
    protected float stickForce = 20;
    protected float stickArea = 15;
	// Use this for initialization
	void Start () {
        Init();
	}

    void Update() //dennas update kallas före stagmovements update
    {
        if(stickTimer > Time.time)
        {
            Vector3 mom = new Vector3(stagMovement.currMomentum.x, 0, stagMovement.currMomentum.z);
            if(mom.magnitude > (stagMovement.currLimitSpeed * maxMomentumPer))
            {
                mom = mom.normalized * (stagMovement.currLimitSpeed * maxMomentumPer);
                stagMovement.currMomentum = mom;
            }

            stagMovement.ySpeed = Mathf.Max(stagMovement.minimumGravity, stagMovement.ySpeed);
        }
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

    void OnTriggerStay(Collider col)
    {
        if (col.gameObject.tag == "Player")
        {
            stickTimer = Time.time + 0.5f;
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
