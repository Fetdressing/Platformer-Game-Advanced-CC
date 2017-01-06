using UnityEngine;
using System.Collections;

public class MagneticForce : BaseRigidbody
{
    public enum ForceType { Push, Pull, Directional };
    [Header("MagneticForce")]
    public ForceType forceType;

    public LayerMask layerMaskNormal;
    public LayerMask layerMaskSpecific; //används för endast spelaren o sånt förmodligen

    public bool massIndependent = false;
    public float force = 20;
    public float range = 40;

    public Vector3 directionVector = Vector3.up;
    // Use this for initialization
    void Start()
    {
        Init();
    }

    public override void Init()
    {
        base.Init();
    }

    // Update is called once per frame
    public virtual void FixedUpdate()
    {
        FixedUpdateLoop();
    }

    public virtual void FixedUpdateLoop()
    {
        ApplyForce();
    }

    public virtual void ApplyForce()
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
                if(massIndependent)
                {
                    conditionMultiplier = rigidbodyTemp.mass;
                }
                switch (forceType)
                {
                    case ForceType.Push:
                        dir = (tr.transform.position - transform.position).normalized;
                        //rigidbodyTemp.AddForce((force * dir * Time.deltaTime) * (1 - distanceMultiplier / range), ForceMode.Force);
                        AddForceFastDrag((force * conditionMultiplier * dir * Time.deltaTime) * (1 - distanceMultiplier / range), ForceMode.Force, rigidbodyTemp);
                        break;
                    case ForceType.Pull:
                        dir = (transform.position - tr.transform.position).normalized;
                        //rigidbodyTemp.AddForce((force * dir * Time.deltaTime) * (1 - distanceMultiplier / range), ForceMode.Force);
                        AddForceFastDrag((force * conditionMultiplier * dir * Time.deltaTime) * (1 - distanceMultiplier / range), ForceMode.Force, rigidbodyTemp);
                        break;
                    case ForceType.Directional:
                        dir = directionVector;
                        AddForceFastDrag((force * conditionMultiplier * dir * Time.deltaTime) * (1 - distanceMultiplier / range), ForceMode.Force, rigidbodyTemp);
                        break;
                }
            }
        }
    }

    public virtual void ApplyForceTarget(Transform tr, float forceT)
    {
        if (tr.GetComponent<Rigidbody>() != null)
        {
            Rigidbody rigidbodyTemp = tr.GetComponent<Rigidbody>();
            Vector3 dir;
            float distanceMultiplier = Vector3.Distance(transform.position, tr.position);
            switch (forceType)
            {
                case ForceType.Push:
                    dir = (tr.transform.position - transform.position).normalized;
                    //rigidbodyTemp.AddForce((forceT * dir * Time.deltaTime) , ForceMode.Force);
                    AddForceFastDrag((forceT * dir * Time.deltaTime), ForceMode.Force, rigidbodyTemp);
                    break;
                case ForceType.Pull:
                    dir = (transform.position - tr.transform.position).normalized;
                    //rigidbodyTemp.AddForce((forceT * dir * Time.deltaTime) , ForceMode.Force);
                    AddForceFastDrag((forceT * dir * Time.deltaTime), ForceMode.Force, rigidbodyTemp);
                    break;
            }
        }
    }

}
