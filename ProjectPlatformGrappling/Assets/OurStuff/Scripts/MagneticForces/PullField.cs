using UnityEngine;
using System.Collections;

public class PullField : BaseRigidbody {
    [Header("Pull Field")]
    public LayerMask layerMaskNormal;
    public LayerMask layerMaskSpecific; //används för endast spelaren o sånt förmodligen

    public bool massIndependent = false;
    public float force = 20000;
    public float range = 140;
    public float pushThreshholdDistance = 10;

    public float upForce = 8000; //skapa en cool effekt mest o lyfta upp objekten lite
	// Use this for initialization
	void Start () {
	
	}

    void FixedUpdate()
    {
        ApplyForce();
    }

    public void ApplyForce()
    {
        Collider[] colliders;
        colliders = Physics.OverlapSphere(transform.position, range, layerMaskNormal);
        foreach (Collider col in colliders)
        {
            Transform tr = col.transform;
            if (tr == transform) continue;

            if (tr.GetComponent<Rigidbody>() != null)
            {
                Rigidbody rigidbodyTemp = tr.GetComponent<Rigidbody>();
                Vector3 dir;
                float distanceMultiplier = Mathf.Max(Vector3.Distance(transform.position, tr.position), 0.4f);
                float conditionMultiplier = 1.0f;
                if (massIndependent)
                {
                    conditionMultiplier = rigidbodyTemp.mass;
                }

                AddForceFastDrag((upForce * conditionMultiplier * Vector3.up * Time.deltaTime) * (1 - distanceMultiplier / range), ForceMode.Force, rigidbodyTemp);
                if (Vector3.Distance(tr.position, transform.position) < pushThreshholdDistance) //push
                {
                    dir = (tr.transform.position - transform.position).normalized;
                    //rigidbodyTemp.AddForce((force * dir * Time.deltaTime) * (1 - distanceMultiplier / range), ForceMode.Force);
                    AddForceFastDrag((force * 1.5f * conditionMultiplier * dir * Time.deltaTime) * (1 - distanceMultiplier / range), ForceMode.Force, rigidbodyTemp);
                }
                else //pull
                {
                    dir = (transform.position - tr.transform.position).normalized;
                    //rigidbodyTemp.AddForce((force * dir * Time.deltaTime) * (1 - distanceMultiplier / range), ForceMode.Force);
                    AddForceFastDrag((force * conditionMultiplier * dir * Time.deltaTime) * (1 - distanceMultiplier / range), ForceMode.Force, rigidbodyTemp);
                }
               
            }
        }
    }
}
