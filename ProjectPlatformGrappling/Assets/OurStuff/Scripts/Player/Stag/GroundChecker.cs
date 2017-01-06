using UnityEngine;
using System.Collections;

public class GroundChecker : BaseClass {
    public Transform stagObject;
    private CharacterController cController;
    private CameraShaker cameraShaker;

    private Transform lastGroundedObj;
    private float groundedTimePoint = 0.0f;
    private float maxValueAirTime = 3; //hur många sekunder som ger max camerashake

    public ParticleTimed touchGroundPS;

    void Start()
    {
        Init();
    }

    public override void Init()
    {
        base.Init();
        cController = stagObject.GetComponent<CharacterController>();
        cameraShaker = GameObject.FindGameObjectWithTag("MainCamera").GetComponentsInChildren<Transform>()[1].transform.GetComponent<CameraShaker>();
    }

    public virtual void Reset(float timePointAirBourne)
    {
        base.Reset();
        lastGroundedObj = null;
        groundedTimePoint = timePointAirBourne; //kan man avgöra hur länga han varit i luften
    }

    //void LateUpdate()
    //{
    //    return;
    //    if(activePlatform != null)
    //    {
    //        Vector3 newGlobalPlatformPoint = activePlatform.TransformPoint(activeLocalPlatformPoint);
    //        Vector3 moveDistance = (newGlobalPlatformPoint - activeGlobalPlatformPoint);
    //        stagObject.position = stagObject.position + moveDistance;

    //        platformVelocity = moveDistance / Time.deltaTime;

    //        //if (moveDistance != Vector3.zero)
    //        //{
    //        //    stagObject.GetComponent<CharacterController>().Move(moveDistance);
    //        //}

    //    }
    //    else
    //    {
    //        platformVelocity = Vector3.zero;
    //    }

    //    activeGlobalPlatformPoint = stagObject.position;
    //    activeLocalPlatformPoint = activePlatform.InverseTransformPoint(stagObject.position);

    //}

    //void OnTriggerStay(Collider col)
    //{

    //    if (col.gameObject.tag == "MovingPlatform")
    //    {
    //        activePlatform = col.transform;
    //    }
    //}

    void OnTriggerEnter(Collider col)
    {
        if (Time.timeScale == 0) return;
        if (LayerMask.LayerToName(col.gameObject.layer) != "Player")
        {
            if (lastGroundedObj == null || lastGroundedObj != col.transform)
            {
                lastGroundedObj = col.transform;

                float airBourneTimeValue = Mathf.Min((Time.time - groundedTimePoint), maxValueAirTime) / maxValueAirTime; //går mellan 0 och 1
                cameraShaker.ShakeCamera(0.1f, 0.8f * airBourneTimeValue, true);

                if (airBourneTimeValue > 0.5f) //procent av max tiden
                {
                    if (touchGroundPS != null)
                    {
                        touchGroundPS.StartParticleSystem();
                    }
                }
            }

            if (col.tag == "BreakerObject")
            {
                col.GetComponent<BreakerObject>().Break();
            }
        }
    }

}
