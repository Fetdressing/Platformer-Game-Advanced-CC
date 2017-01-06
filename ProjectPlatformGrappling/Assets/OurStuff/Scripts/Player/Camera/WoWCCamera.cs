using UnityEngine;
using System.Collections;

public class WoWCCamera : BaseClass
{
    protected ControlManager controlManager;
    public LayerMask collisionLayerMask; //mot vilka lager ska kameran kolla kollision?

    public Transform target;
    private float currSpeed = 0.0f;
    private Vector3 currFrameTargetPos = new Vector3();
    private Vector3 lastFrameTargetPos = new Vector3();

    public float targetHeight = 12.0f;
    public float distance = 5.0f;
    public float extraDistance = 0.0f;

    public float maxDistance = 20;
    public float minDistance = 2.5f;

    public float xSpeed = 250.0f;
    public float ySpeed = 120.0f;
    [HideInInspector]
    public float speedMultiplier = 1;

    public float yMinLimit = -20;
    public float yMaxLimit = 80;

    protected IEnumerator goToPrefAngleCo;
    protected float yMinSoftLimit = -5;
    protected float yMaxSoftLimit = 20;
    protected float yPreferredAngle = 20; //den anglen som den flyttas till efter man gått över gränsen

    public float zoomRate = 20;

    public float rotationDampening = 3.0f;

    public float theta2 = 0.5f;

    private float x = 0.0f;
    private float y = 0.0f;

    private Vector3 fwd = new Vector3();
    private Vector3 rightVector = new Vector3();
    private Vector3 upVector = new Vector3();
    private Vector3 movingVector = new Vector3();
    private Vector3 collisionVector = new Vector3();
    private bool isColliding = false;

    private Vector3 a1 = new Vector3();
    private Vector3 b1 = new Vector3();
    private Vector3 c1 = new Vector3();
    private Vector3 d1 = new Vector3();
    private Vector3 e1 = new Vector3();
    private Vector3 f1 = new Vector3();
    private Vector3 h1 = new Vector3();
    private Vector3 i1 = new Vector3();

    [HideInInspector]public IEnumerator settingRotation;
    [HideInInspector]
    public bool movingToPos = false;
    //@script AddComponentMenu("Camera-Control/WoW Camera")

    void Start()
    {
        controlManager = GameObject.FindGameObjectWithTag("Manager").GetComponent<ControlManager>();
        var angles = transform.eulerAngles;
        x = angles.y;
        y = angles.x;

        // Make the rigid body not change rotation
        if (GetComponent<Rigidbody>())
            GetComponent<Rigidbody>().freezeRotation = true;

        movingToPos = false;
        settingRotation = null;
    }

    public override void Reset()
    {
        base.Reset();
        movingToPos = false;
        if (settingRotation != null)
        {
            StopCoroutine(settingRotation);
        }
        settingRotation = null;

        StopAllCoroutines();
    }

    public IEnumerator SetRot(Vector3 newDir, bool unlock = true)
    {
        if(settingRotation != null)
        {
            StopCoroutine(settingRotation);
        }
        movingToPos = true;

        float xn = Vector3.Angle(transform.forward, newDir);

        if(Vector3.Dot(transform.right, newDir) < 0.0f)
        {
            xn = -xn;
        }

        settingRotation = SettingRot(xn, unlock);
        yield return StartCoroutine(settingRotation);
    }

    IEnumerator SettingRot(float xn, bool unlock)
    {
        Debug.Log("Den väljer alltid den minsta lutningen, det blir fel för denna som alltid går genom samma håll, FIXA!");

        xn = xn + x;
        float yn = 30;
        while(settingRotation != null && Mathf.Abs(x - xn) + Mathf.Abs(y - yn) > 2f)
        {
            x = Mathf.Lerp(x, xn, Time.deltaTime * 8);
            y = Mathf.Lerp(y, yn, Time.deltaTime * 8);
            
            Quaternion rotation = Quaternion.Euler(y, x, 0);
            Vector3 position = target.position - (rotation * Vector3.forward * (distance) + new Vector3(0, -targetHeight, 0));

            transform.position = position;
            transform.rotation = rotation;

            yield return new WaitForEndOfFrame();
        }

        if (unlock)
        {
            movingToPos = false;
        }
        settingRotation = null;
    }

    void LateUpdate()
    {
        if (movingToPos == true) return;
        if (Time.timeScale == 0) return;

        if (!target)
            return;

        currFrameTargetPos = target.position;
        if (lastFrameTargetPos != Vector3.zero)
        {
            var valueSpeed = Mathf.Abs(Mathf.Abs(lastFrameTargetPos.magnitude) - Mathf.Abs(currFrameTargetPos.magnitude)) * 2;
            extraDistance = Mathf.Lerp(extraDistance, valueSpeed, Time.deltaTime * 10f); 
        }
        else
        {
            extraDistance = Mathf.Lerp(extraDistance, 0, Time.deltaTime * 10f);
        }

        x += controlManager.horAxisView * xSpeed * 0.02f * speedMultiplier;
        y -= controlManager.verAxisView * ySpeed * 0.02f * speedMultiplier;


        distance -= (Input.GetAxis("Mouse ScrollWheel") * Time.deltaTime) * zoomRate * Mathf.Abs(distance);
        distance = Mathf.Clamp(distance, minDistance, maxDistance);

        y = ClampAngle(y, yMinLimit, yMaxLimit);

        ////lerpa tillbaks den om ingen input är på den vertikala axeln
        //if (controlManager.verAxisView < 0.01f && controlManager.verAxisView > -0.01f)
        //{
        //    if (y > yMaxSoftLimit)
        //    {
        //        if(goToPrefAngleCo != null)
        //        {
        //            StopCoroutine(goToPrefAngleCo);
        //        }
        //        goToPrefAngleCo = GoToPreferredAngle(yMaxSoftLimit);
        //        StartCoroutine(goToPrefAngleCo);
        //    }
        //    else if(y < yMinSoftLimit)
        //    {
        //        if (goToPrefAngleCo != null)
        //        {
        //            StopCoroutine(goToPrefAngleCo);
        //        }
        //        goToPrefAngleCo = GoToPreferredAngle(yMinSoftLimit);
        //        StartCoroutine(goToPrefAngleCo);
        //    }
        //}

        Quaternion rotation = Quaternion.Euler(y, x, 0);
        Vector3 position = target.position - (rotation * Vector3.forward * (distance + extraDistance) + new Vector3(0, -targetHeight, 0));

        float newDistance = 0.000f;
        
        Vector3 toTar = (position - target.position);
        //Debug.DrawRay(target.position, toTar, Color.red);

        Vector3 yOffset = new Vector3(0, 1.5f, 0);
        CompensateForWalls(target.position + yOffset, position, ref newDistance);

        if(newDistance > 0.01f)
        {
            position = target.position - (rotation * Vector3.forward * (newDistance) + new Vector3(0, -targetHeight, 0));
        }
        //// Check to see if we have a collision
        //collisionVector = AdjustLineOfSight(transform.position, position);

        //// Check Line Of Sight
        //if (collisionVector != Vector3.zero)
        //{
        //    Debug.Log("Check Line Of Sight");
        //    a1 = transform.position;
        //    b1 = position;
        //    c1 = AdjustLineOfSight(transform.position, position);
        //    d1 = c1 - a1;
        //    e1 = d1.normalized * -1;
        //    f1 = d1 + e1 * 1;
        //    g1 = f1 + a1;
        //    position = g1;

        //    // check distance player to camera
        //    h1 = position - a1;
        //    if (h1.magnitude < 10)
        //    {
        //        position = a1 - fwd * 4;
        //        //position.y = targetPlayer.y;
        //        theta2 = theta2 + .25;
        //    }

        //    // set new camera distance
        //    h1 = position - a1;
        //    distance = h1.magnitude;
        //}

        //// check collision
        //if (Physics.CheckSphere (position, .5) )
        //{
        //    a1 = transform.position;

        //    newPosition = a1 - fwd * 4;
        //    //newPosition.y = targetPlayer.y;
        //    theta2 = theta2 + .25;

        //    // set new camera distance
        //    h1 = position - a1;
        //    distance = h1.magnitude;
        //}  

        //position = Vector3.Slerp(transform.position, position, Time.deltaTime * 100);

        transform.rotation = rotation;
        transform.position = position;

        lastFrameTargetPos = target.position;


    }

    IEnumerator GoToPreferredAngle(float prefAngle)
    {
        while(Mathf.Abs(y - prefAngle) > 1.0f)
        {
            if(controlManager.verAxisView > 0.0f && controlManager.verAxisView < 0.0f) //avbryt om spelaren gör input
            {
                yield break;
            }

            y = Mathf.Lerp(y, prefAngle, Time.deltaTime * 1);
            yield return new WaitForEndOfFrame();
        }
    }

    static float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360)
            angle += 360;
        if (angle > 360)
            angle -= 360;
        return Mathf.Clamp(angle, min, max);
    }

    private void CompensateForWalls(Vector3 fromObject, Vector3 toTar, ref float distance)
    {
        // Compensate for walls between camera
        RaycastHit wallHit = new RaycastHit();
        if (Physics.Linecast(fromObject, toTar, out wallHit, collisionLayerMask))
        {
            distance = (wallHit.point - fromObject).magnitude;
        }

    }


    //Vector3 AdjustLineOfSight(Vector3 vecA, Vector3 vecB)
    //{
    //    RaycastHit hit;

    //    if (Physics.Linecast(vecA, vecB, hit))
    //    {
    //        Debug.Log("I hit something");
    //        return hit.point;
    //    }

    //    return Vector3.zero;
    //}
}