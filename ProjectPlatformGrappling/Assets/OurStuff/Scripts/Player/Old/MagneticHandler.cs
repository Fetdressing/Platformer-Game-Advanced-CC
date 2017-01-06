using UnityEngine;
using System.Collections;

public class MagneticHandler : MonoBehaviour {
    private Transform thisTransform;
    private Rigidbody thisRigidbody;
    public Camera thisCamera;

    private RaycastHit raycastHit;
    private bool validTarget;
    private Rigidbody connectRigidbody;
    private Vector3 connectedPos;

    public Transform lineRendererOffset;
    private LineRenderer forceLineRenderer;

    public LayerMask targetLayerMask;
    public float continiousStartForce = 20000;
    public float instantStartForce = 10; //använder annan forcemode så därför mindre

    private Vector3 vecToThis;
    private Vector3 vecToConnect;
    // Use this for initialization
    void Start () {
        thisTransform = this.transform;
        thisRigidbody = thisTransform.GetComponent<Rigidbody>();
        validTarget = false;

        forceLineRenderer = thisTransform.GetComponent<LineRenderer>();
    }
	
	// Update is called once per frame
	void Update () {
        GetTarget();

        if (validTarget)
        {
            if (Input.GetKey(KeyCode.Mouse1))
            {
                forceLineRenderer.enabled = true;
                Vector3[] positionArray = new[] { lineRendererOffset.position, connectedPos };
                forceLineRenderer.SetPositions(positionArray);

                UsePull(continiousStartForce * Time.deltaTime, ForceMode.Force);
            }
            else if (Input.GetKey(KeyCode.Mouse0))
            {
                forceLineRenderer.enabled = true;
                Vector3[] positionArray = new[] { lineRendererOffset.position, connectedPos };
                forceLineRenderer.SetPositions(positionArray);

                UsePush(continiousStartForce * Time.deltaTime, ForceMode.Force);
            }
            else
            {
                forceLineRenderer.enabled = false;
            }
        }
        else
        {
            forceLineRenderer.enabled = false;
        }
	}

    void GetTarget()
    {
        if (Physics.Raycast(thisCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0.5f)), out raycastHit, 1000, targetLayerMask)) //kasta från mitten av skärmen!
        {
            if (raycastHit.transform.GetComponent<Rigidbody>() != null)
            {
                connectRigidbody = raycastHit.transform.GetComponent<Rigidbody>();
                connectedPos = raycastHit.point;
                validTarget = true;
            }
            else
            {
                connectRigidbody = null;
                connectedPos = raycastHit.point;
                validTarget = true;
            }
            vecToThis = (connectedPos - thisTransform.position).normalized;
            vecToConnect = (thisTransform.position - connectedPos).normalized;
        }
        else
        {
            validTarget = false;
        }
    }

    void UsePush(float force, ForceMode fm)
    {
        if (validTarget == false) return;
        if (connectRigidbody != null)
        {
            connectRigidbody.AddForce(force * 100 * vecToThis, fm);
        }

        thisRigidbody.AddForce(force * vecToConnect, fm);
    }

    void UsePull(float force, ForceMode fm)
    {
        if (validTarget == false) return;
        if (connectRigidbody != null)
        {
            connectRigidbody.AddForce(force * 100 * vecToConnect, fm);
        }

        thisRigidbody.AddForce(force * vecToThis, fm);
    }
}
