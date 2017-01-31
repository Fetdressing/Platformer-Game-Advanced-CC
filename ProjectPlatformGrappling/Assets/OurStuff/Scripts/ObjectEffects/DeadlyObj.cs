using UnityEngine;
using System.Collections;

public class DeadlyObj : BaseClass {
    public bool canBeBlocked = false;
	// Use this for initialization
	void Start () {
	
	}

    public override void Init()
    {
        base.Init();
        bActivated = true;
    }

    public override void Deactivate()
    {
        base.Deactivate();
        bActivated = false;
    }

    public override void Reset()
    {
        base.Reset();
        bActivated = true;
    }

    void OnTriggerEnter(Collider col)
    {
        if (!bActivated) { return; }
        PowerManager pM = col.GetComponent<PowerManager>();

        if(pM != null)
        {
            StagMovement sM = col.GetComponent<StagMovement>();
            if (canBeBlocked)
            {
                if (sM.speedBreaker.active == true)
                {
                    return;
                }
            }

            pM.Die();
        }
    }

    void OnCollisionEnter(Collision col)
    {
        if (!bActivated) { return; }
        PowerManager pM = col.collider.transform.GetComponent<PowerManager>();

        if (pM != null)
        {
            StagMovement sM = col.gameObject.GetComponent<StagMovement>();
            if (canBeBlocked)
            {
                if (sM.speedBreaker.active == true)
                {
                    return;
                }
            }

            pM.Die();
        }
    }
}
