using UnityEngine;
using System.Collections;

public class DeadlyObj : MonoBehaviour {
    public bool canBeBlocked = false;
	// Use this for initialization
	void Start () {
	
	}
	

    void OnTriggerEnter(Collider col)
    {
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
