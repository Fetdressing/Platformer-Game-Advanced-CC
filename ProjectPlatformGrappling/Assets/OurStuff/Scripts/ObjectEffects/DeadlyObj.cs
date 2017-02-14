using UnityEngine;
using System.Collections;

public class DeadlyObj : BaseClass {
    public bool canBeBlocked = false;
    public bool killsEnemies = true;
	// Use this for initialization
	void Start () {
        Init();
	}

    public override void Init()
    {
        base.Init();
        Reset();
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
            return;
        }

        if (killsEnemies)
        {
            HealthSpirit hs = col.GetComponent<HealthSpirit>();
            if (hs != null)
            {
                hs.Die();
            }
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
            return;
        }

        if (killsEnemies)
        {
            HealthSpirit hs = col.gameObject.GetComponent<HealthSpirit>();
            if (hs != null)
            {
                hs.Die();
            }
        }
    }
}
