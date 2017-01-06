using UnityEngine;
using System.Collections;

public class BaseRagdoll : BaseRigidbody {
    bool lastState; //vad den senast var, så man inte sätter den till samma sak hela tiden

    public Rigidbody baseJointRigidbody;
    [HideInInspector]
    public Vector3 baseJointStartPos;

    public float toggleCooldown = 1.0f;
    private float toggleCooldownTimer = 0.0f;

    public override void Init()
    {
        base.Init();
        baseJointStartPos = baseJointRigidbody.transform.localPosition;

        lastState = true; //viktigt
        ToggleRagdoll(false);
        baseJointRigidbody.constraints = RigidbodyConstraints.FreezePosition;
    }

    public override void Reset()
    {
        base.Reset();
        toggleCooldownTimer = 0.0f;
    }

    //public override void UpdateLoop()
    //{
    //    if (initTimes == 0) return;
    //    base.UpdateLoop();
    //    if (currRigidbody == baseJointRigidbody)
    //    {
    //        //Debug.Log("Den flyttar ju baseJointRigidbodyn oxå");
    //        //thisRigidbody.transform.position = baseJointRigidbody.transform.position + new Vector3(0, 3, 0);
    //    }
    //}

    public virtual bool ToggleRagdoll(bool b) //returnerar ifall den kunde gör detta nu
    {
        if (lastState == b) return false; //ska inte kunna toggla till samma state igen

        lastState = b;

        if (Time.time < toggleCooldownTimer) return false;
        else
        {
            toggleCooldownTimer = Time.time + toggleCooldown;
            //Debug.Log(toggleCooldownTimer.ToString());
        }

        foreach (Rigidbody rb in this.transform.GetComponentsInChildren<Rigidbody>())
        {
            if (rb != thisRigidbody)
            {
                rb.isKinematic = !b;
                rb.transform.GetComponent<Collider>().enabled = false; //kanske jag vill ha igång dem
                rb.useGravity = b;
            }
        }

        //if (b == true) //sätt igång ragdoll!
        //{
        //    currRigidbody = thisRigidbody;
        //    thisCollider.isTrigger = false; //så testar vi skada mot ontriggerenter istället
        //    //thisRigidbody.isKinematic = true;
        //    //thisRigidbody.useGravity = false;
        //}
        //else //stäng av ragdoll, flytta tillbaks childet
        //{
        //    currRigidbody = thisRigidbody; //kan behöva flytta upp transformen så den inte buggar genom marken
        //    thisCollider.isTrigger = false;
            
        //    //thisRigidbody.isKinematic = false;

        //    //baseJointRigidbody.transform.localPosition = baseJointStartPos;
        //    //thisRigidbody.useGravity = true;
        //}

        return true;
    }
}
