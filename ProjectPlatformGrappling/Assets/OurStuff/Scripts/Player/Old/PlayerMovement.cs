using UnityEngine;
using System.Collections;

public class PlayerMovement : MonoBehaviour {
    private Transform thisTransform;
    private Rigidbody thisRigidbody;
    public Camera thisCamera;
    private Transform cameraParent; //rotates around y
    public Transform cameraFollowObject;


    public Transform lineRendererOffset;
    private SpringJoint thisCjointSwing;
    private LineRenderer hookLineRenderer;
    private RaycastHit raycastHit;
    private Rigidbody connectedRigidbody;
    private Vector3 connectedPos;
    public float finalDistanceToHook = 5.0f;
    private bool isAttached;

    private bool isGrounded;

    [Header("MouseStuff")]
    public float XSensitivity = 8;
    public float YSensitivity = 8;
    public float smoothTime = 10;

    public bool clampVerticalRotation;
    public float MinimumX = -90F;
    public float MaximumX = 90F;
    private Quaternion thisTargetRot;
    private Quaternion thisCameraTargetRot;
    // Use this for initialization
    void Start () {
        thisTransform = this.transform;
        thisRigidbody = thisTransform.GetComponent<Rigidbody>();
        //thisCamera = thisTransform.GetComponentsInChildren<Camera>()[0];
        cameraParent = thisCamera.transform.parent;
        thisCjointSwing = thisTransform.GetComponent<SpringJoint>();
        hookLineRenderer = lineRendererOffset.GetComponent<LineRenderer>();

        //mousestuff
        thisTargetRot = cameraParent.localRotation;
        thisCameraTargetRot = thisCamera.transform.localRotation;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        isGrounded = false;
        isAttached = false;
    }
	
    void Update()
    {
        GetGrounded();
        //if(!isGrounded)
        //{
        //    thisRigidbody.AddForce(Vector3.down * 10);
        //}
        //if(isGrounded)
        //{
        //    thisTransform.localRotation = Quaternion.identity;
        //}

        if (Input.GetKeyDown(KeyCode.F))
        {
            FireHook();
        }
        UpdateHook();
        RotateView();
    }

    void LateUpdate()
    {
        //float z = thisCamera.transform.eulerAngles.z; //se till så ingen rotation är i z på kameran
        //thisCamera.transform.Rotate(0, 0, -z);

        cameraParent.transform.position = Vector3.Slerp(thisCamera.transform.position, cameraFollowObject.position, smoothTime * Time.deltaTime * 10); //förflytta kameran till spelar objektet
    }

	// Update is called once per frame
	void FixedUpdate () {
	    if(Input.GetKey(KeyCode.A))
        {
            thisRigidbody.AddForce(thisCamera.transform.right * -2000 * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.D))
        {
            thisRigidbody.AddForce(thisCamera.transform.right * 2000 * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.W))
        {
            thisRigidbody.AddForce(thisCamera.transform.forward * 2000 * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.S))
        {
            thisRigidbody.AddForce(thisCamera.transform.forward * -2000 * Time.deltaTime);
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            thisRigidbody.AddForce(thisCamera.transform.up * 1000);
        }
    }

    void FireHook()
    {
        if(Physics.Raycast(thisCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0.5f)), out raycastHit)) //kasta från mitten av skärmen!
        {
            if (raycastHit.transform.GetComponent<Rigidbody>() != null)
            {
                AttachHook(raycastHit.point, raycastHit.normal, raycastHit.transform.GetComponent<Rigidbody>());
            }
            else
            {
                DetachHook();
            }
        }
        else
        {
            DetachHook();
        }
    }

    void UpdateHook()
    {
        if (isAttached)
        {
            //if(Input.GetKey(KeyCode.F))
            //{
            //    float distanceSpeedMult = Mathf.Min(Vector3.Distance(connectedRigidbody.transform.position, thisTransform.position) * 0.02f, 2.0f);
            //    thisCjointSwing.anchor = Vector3.MoveTowards(thisCjointSwing.anchor, new Vector3(finalDistanceToHook, finalDistanceToHook, finalDistanceToHook), 35.3f * Time.deltaTime);
            //}
            //else if(Input.GetKey(KeyCode.G)) //går ej backa?
            //{
            //    float distanceSpeedMult = Mathf.Min(Vector3.Distance(connectedRigidbody.transform.position, thisTransform.position) * 0.02f, 2.0f);
            //    thisCjointSwing.anchor = Vector3.Slerp(thisCjointSwing.anchor, new Vector3(finalDistanceToHook, finalDistanceToHook, finalDistanceToHook), Time.deltaTime * 0.2f * -distanceSpeedMult);
            //}
            //Debug.Log(thisCjointSwing.anchor.ToString());
            //thisCjointSwing.connectedAnchor = thisTransform.position - hookAlongVector;
            //thisCjointSwing.axis = (connectedRigidbody.transform.position - thisTransform.position).normalized;
            hookLineRenderer.enabled = true;
            Vector3[] positionArray = new[] { lineRendererOffset.position, connectedPos };
            hookLineRenderer.SetPositions(positionArray);

            //thisCjointSwing.axis = (thisTransform.position - connectedRigidbody.transform.position).normalized;
        }
        else
        {
            hookLineRenderer.enabled = false;
        }
    }

    void AttachHook(Vector3 pos, Vector3 nor, Rigidbody rigidbodyHit)
    {
        if (rigidbodyHit.isKinematic == false) return;
        DetachHook();
        isAttached = true;
        thisCjointSwing = thisTransform.gameObject.AddComponent<SpringJoint>() as SpringJoint;
        connectedRigidbody = rigidbodyHit;
        //thisCjointSwing.axis = (pos - thisTransform.position).normalized;
        //Debug.Log(pos.ToString());
        connectedPos = pos;
        thisCjointSwing.anchor = thisTransform.InverseTransformPoint(pos);
        thisCjointSwing.connectedAnchor = thisTransform.position;

        //thisCjointSwing.xMotion = ConfigurableJointMotion.Limited;
        //thisCjointSwing.yMotion = ConfigurableJointMotion.Limited;
        //thisCjointSwing.zMotion = ConfigurableJointMotion.Limited;

        //SoftJointLimitSpring sjL = new SoftJointLimitSpring();
        //sjL.damper = 150;
        //sjL.spring = 0;
        //thisCjointSwing.linearLimitSpring = sjL;

        thisCjointSwing.breakForce = 800;
        thisCjointSwing.breakTorque = 10;
        thisCjointSwing.spring = 2;
        thisCjointSwing.damper = 0;
        //thisCjointSwing.useLimits = false;

        thisCjointSwing.connectedBody = rigidbodyHit;

        thisCjointSwing.autoConfigureConnectedAnchor = true;
        //thisCjointSwing.enableProjection = false;
        thisCjointSwing.enablePreprocessing = false;
        thisCjointSwing.enableCollision = true;
        //thisCjointSwing.minDistance = 2;
        //thisCjointSwing.minDistance = Vector3.Distance(thisTransform.position, pos) / 10;
        //thisCjointSwing.spring = 80;
        //SoftJointLimit sjL = new SoftJointLimit();
        //sjL.limit = 180;
        //thisCjointSwing.swing1Limit = sjL;
        //thisCjointSwing.swing2Limit = sjL;
        //SoftJointLimit sjL2 = new SoftJointLimit();
        //sjL2.limit = 0;
        //thisCjointSwing.lowTwistLimit = sjL2;
        //thisCjointSwing.highTwistLimit = sjL2;

        //SoftJointLimitSpring sjS = new SoftJointLimitSpring();
        //sjS.spring = 10;
        //thisCjointSwing.swingLimitSpring = sjS;

    }

    void DetachHook()
    {
        if (!isAttached) return;

        if (thisCjointSwing != null)
        {
            Destroy(thisCjointSwing);
        }
        isAttached = false;
        
    }

    void OnJointBreak(float breakForce)
    {
        DetachHook();
    }
    void GetGrounded()
    {
        Ray ray = new Ray(thisTransform.position, Vector3.down);
        if(Physics.Raycast(ray, 1.2f))
        {
            isGrounded = true;
        }
        else
        {
            isGrounded = false;
        }
    }

    //kamera stuff
    private void RotateView()
    {
        //avoids the mouse looking if the game is effectively paused
        if (Mathf.Abs(Time.timeScale) < float.Epsilon) return;

        // get the rotation before it's changed
        float oldYRotation = thisTransform.eulerAngles.y;

        LookRotation();

        if (isGrounded)
        {
            // Rotate the rigidbody velocity to match the new direction that the character is looking
            Quaternion velRotation = Quaternion.AngleAxis(thisTransform.eulerAngles.y - oldYRotation, Vector3.up);
            //thisRigidbody.velocity = velRotation * thisRigidbody.velocity;
        }
    }

    public void LookRotation()
    {
        float yRot = Input.GetAxis("Mouse X") * XSensitivity;
        float xRot = Input.GetAxis("Mouse Y") * YSensitivity;

        thisTargetRot *= Quaternion.Euler(0f, yRot, 0f);
        thisCameraTargetRot *= Quaternion.Euler(-xRot, 0f, 0f);

        if (clampVerticalRotation)
            thisCameraTargetRot = ClampRotationAroundXAxis(thisCameraTargetRot);

        cameraParent.localRotation = Quaternion.Slerp(cameraParent.localRotation, thisTargetRot,
            smoothTime * Time.deltaTime);
        thisCamera.transform.localRotation = Quaternion.Slerp(thisCamera.transform.localRotation, thisCameraTargetRot,
            smoothTime * Time.deltaTime);

        //float z = thisCamera.transform.eulerAngles.z;
        //thisCamera.transform.Rotate(0, 0, -z);
        //thisCamera.transform.localRotation = Quaternion.Euler(tempQ.x, tempQ.y, 0);
        //UpdateCursorLock();
    }

    Quaternion ClampRotationAroundXAxis(Quaternion q)
    {
        q.x /= q.w;
        q.y /= q.w;
        q.z /= q.w;
        q.w = 1.0f;

        float angleX = 2.0f * Mathf.Rad2Deg * Mathf.Atan(q.x);

        angleX = Mathf.Clamp(angleX, MinimumX, MaximumX);

        q.x = Mathf.Tan(0.5f * Mathf.Deg2Rad * angleX);

        return q;
    }
}
