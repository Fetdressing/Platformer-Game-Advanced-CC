using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MagneticPlacer : BaseClass {
    private Transform thisTransform;
    private Camera thisCamera;
    public Transform playerTransform;

    public Transform[] projectilesPull;
    //private List<Transform> projectilesStartPositionsPull = new List<Transform>();
    private int currPullIndex = 0;

    public Transform[] projectilesPush;
    //private List<Transform> projectilesStartPositionsPush = new List<Transform>();
    private int currPushIndex = 0;

    public float shootForce = 1100;
    [HideInInspector]
    public float ballOutTime = 2.5f;
    public LayerMask targetLayerMask;
    // Use this for initialization
    void Start () {
        Init();
    }

    public override void Init()
    {
        base.Init();
        thisTransform = this.transform;
        thisCamera = thisTransform.GetComponentsInChildren<Transform>()[1].GetComponent<Camera>();
        for (int i = 0; i < projectilesPull.Length; i++)
        {
            Transform temp = new GameObject().transform;
            temp.SetParent(playerTransform);
            temp.position = projectilesPull[i].position;
            //projectilesStartPositionsPull.Add(temp);
            projectilesPull[i].SetParent(null);
            //projectilesPull[i].GetComponent<MagneticBall>().SetStartTransform(temp);
            projectilesPull[i].GetComponent<MagneticBall>().SetPlayer(playerTransform);
        }

        //for (int i = 0; i < projectilesPush.Length; i++)
        //{
        //    Transform temp = new GameObject().transform;
        //    temp.SetParent(playerTransform);
        //    temp.position = projectilesPush[i].position;
        //    //projectilesStartPositionsPush.Add(temp);
        //    projectilesPush[i].SetParent(null);
        //    projectilesPush[i].GetComponent<MagneticBall>().SetStartTransform(temp);
        //}
    }

    public override void Reset()
    {
        base.Reset();
        projectilesPull[0].GetComponent<MagneticBall>().Reset();
        projectilesPull[1].GetComponent<MagneticBall>().Reset();
    }

    //void HandleNextPullProjectile()
    //{
    //    MagneticBall mBall = projectilesPull[currPullIndex].GetComponent<MagneticBall>();
    //    MagneticBallState state = mBall.magneticBallState;

    //    MagneticBallGetNextState(mBall);

    //    currPullIndex++;
    //    if(currPullIndex >= projectilesPull.Length)
    //    {
    //        currPullIndex = 0;
    //    }
    //}

    void HandlePullProjectile(int index) //fast ett specifikt index istället
    {
        MagneticBall mBall = projectilesPull[index].GetComponent<MagneticBall>();
        MagneticBallState state = mBall.magneticBallState;

        MagneticBallGetNextState(mBall);

    }

    //void HandleNextPushProjectile()
    //{
    //    MagneticBall mBall = projectilesPush[currPushIndex].GetComponent<MagneticBall>();
    //    MagneticBallState state = mBall.magneticBallState;

    //    MagneticBallGetNextState(mBall);
    //    currPushIndex++;
    //    if (currPushIndex >= projectilesPush.Length)
    //    {
    //        currPushIndex = 0;
    //    }
    //}


    void MagneticBallGetNextState(MagneticBall mBall)
    {
        MagneticBallState state = mBall.magneticBallState;

        switch (state)
        {
            case MagneticBallState.HeadingHome:
                FireMagneticBall(mBall.thisRigidbody);
                break;
            case MagneticBallState.HeadingToTarget:
                mBall.OrderHeadHome();
                break;
            case MagneticBallState.ApplyingGravity:
                mBall.OrderHeadHome();
                break;
        }
    }

    void FireMagneticBall(Rigidbody rb)
    {
        MagneticBall mBall = rb.transform.GetComponent<MagneticBall>();
        RaycastHit raycastHit;
        if (Physics.Raycast(thisCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0.0f)), out raycastHit, Mathf.Infinity,targetLayerMask)) //kasta från mitten av skärmen!
        {
            if (Vector3.Dot(raycastHit.normal, thisCamera.transform.position - raycastHit.point) > 0) //normalen mot eller från sig?
            {
                if (Vector3.Distance(raycastHit.point, thisCamera.transform.position) <= (Vector3.Distance(playerTransform.position, thisCamera.transform.position))) //ligger raycasthit framför spelaren? man vill ju ej skjuta bakåt
                {
                    rb.transform.forward = thisTransform.forward;
                    mBall.OrderFire(shootForce, ballOutTime);
                }
                else
                {
                    rb.transform.LookAt(raycastHit.point);
                    mBall.OrderFire(shootForce, ballOutTime);
                }
            }
            else
            {
                rb.transform.forward = thisTransform.forward;
                mBall.OrderFire(shootForce, ballOutTime);
            }
        }
        else
        {
            rb.transform.forward = thisTransform.forward;
            mBall.OrderFire(shootForce, ballOutTime);
        }
    }

    void FireMagneticBallsToSides()
    {
        MagneticBall mBallRight = projectilesPull[0].GetComponent<MagneticBall>();
        MagneticBall mBallLeft = projectilesPull[1].GetComponent<MagneticBall>();

        if (mBallRight.magneticBallState != MagneticBallState.HeadingHome) { mBallRight.OrderHeadHome(); mBallLeft.OrderHeadHome(); return; }
        if (mBallLeft.magneticBallState != MagneticBallState.HeadingHome) { mBallRight.OrderHeadHome(); mBallLeft.OrderHeadHome(); return; }

        mBallRight.transform.forward = thisTransform.right * -1;
        mBallLeft.transform.forward = thisTransform.right;

        mBallRight.OrderFire(shootForce, ballOutTime);
        mBallLeft.OrderFire(shootForce, ballOutTime);
    }

    // Update is called once per frame
    void Update () {
        projectilesPull[0].GetComponent<MagneticBall>().SetStartPosition(playerTransform.position + thisTransform.right * -4 + Vector3.up * 2);
        projectilesPull[1].GetComponent<MagneticBall>().SetStartPosition(playerTransform.position + thisTransform.right * 4 + Vector3.up * 2);

        if (Input.GetButtonDown("Fire1"))
        {
            HandlePullProjectile(0);
        }
        if (Input.GetButtonDown("Fire2"))
        {
            HandlePullProjectile(1);
        }
        if(Input.GetKeyDown(KeyCode.LeftShift))
        {
            FireMagneticBallsToSides();
        }
    }
}
