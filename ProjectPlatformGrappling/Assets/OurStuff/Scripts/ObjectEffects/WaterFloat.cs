using UnityEngine;
using System.Collections;

public class WaterFloat : BaseRigidbody {
    public Vector3 upFloatVector = Vector3.up;
    public Vector3 streamFloatVector = new Vector3(0,0,0);

    public float streamForce = 1600;

    public GameObject splashParticle;
    private float splashCooldown = 0.5f;
    private float splashTimer = 0.0f;
    // Use this for initialization

    void OnTriggerStay(Collider col)
    {
        Rigidbody colRig = col.GetComponent<Rigidbody>();

        if (colRig != null && colRig.isKinematic == false)
        {

            AddForceSlowDrag(streamFloatVector * streamForce * Time.deltaTime, ForceMode.Force, colRig);
            AddForceSlowDrag(upFloatVector * streamForce * 0.5f * Time.deltaTime, ForceMode.Force, colRig);
        }
        else
        {
            StagMovement stagM = col.GetComponent<StagMovement>();
            if(stagM != null)
            {
                stagM.ApplyYForce(streamForce * Time.deltaTime, streamForce * 0.1f);
            }
        }
    }

    void OnTriggerEnter(Collider col)
    {
        //splash
        if (splashTimer > Time.time) return;
        splashTimer = splashCooldown + Time.time;
        GameObject parTemp = GameObject.Instantiate(splashParticle.gameObject);
        parTemp.transform.position = col.transform.position;
        Destroy(parTemp, 3);
    }
}
