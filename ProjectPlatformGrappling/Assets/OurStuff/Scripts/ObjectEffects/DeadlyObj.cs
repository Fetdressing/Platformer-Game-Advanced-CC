using UnityEngine;
using System.Collections;

public class DeadlyObj : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	

    void OnTriggerEnter(Collider col)
    {
        PowerManager pM = col.GetComponent<PowerManager>();

        if(pM != null)
        {
            pM.Die();
        }
    }

    void OnCollisionEnter(Collision col)
    {
        PowerManager pM = col.collider.transform.GetComponent<PowerManager>();

        if (pM != null)
        {
            pM.Die();
        }
    }
}
