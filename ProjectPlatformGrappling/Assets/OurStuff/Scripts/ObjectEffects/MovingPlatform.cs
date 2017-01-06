using UnityEngine;
using System.Collections;

public class MovingPlatform : MonoBehaviour {
    private Rigidbody thisRigidbody;
    private Transform thisTransform;
    public Transform rotateMesh;
    public float rotationSpeed = 2;
    public float multRot = -1; //vänder rotationen helt tex
    
    public enum MoveType
    {
        Force, MovePos, Translate
    }

    public MoveType moveType = 0;

    private Transform currTarget;
    private int currIndex = 0;
    public Transform[] keyPoints;
    public float force = 20000;

    private Vector3 vecToTarget = Vector3.zero;
    [HideInInspector]
    public Vector3 moveDirection = Vector3.zero;
    // Use this for initialization
    void Start () {
        thisTransform = this.transform;
        thisRigidbody = thisTransform.GetComponent<Rigidbody>();

        GetCurrentTarget();
    }
	
	// Update is called once per frame
	void Update () {
        CheckReached();

        vecToTarget = (currTarget.position - thisTransform.position).normalized;
        moveDirection = vecToTarget; //kan hämtas av tex playermovmentet
        //if (useForce == true)
        //{
        //    thisRigidbody.AddForce(vecToTarget * force * Time.deltaTime);
        //}
        //else
        //{
        //    thisRigidbody.MovePosition(thisTransform.position + vecToTarget * force * Time.deltaTime);
        //}

        switch (moveType)
        {
            case MoveType.Force:
                thisRigidbody.AddForce(vecToTarget * force * Time.deltaTime);
                break;
            case MoveType.MovePos:
                thisRigidbody.MovePosition(thisTransform.position + vecToTarget * force * Time.deltaTime);
                break;
            case MoveType.Translate:
                transform.position = Vector3.MoveTowards(transform.position, currTarget.position, force * Time.deltaTime);
                //transform.Translate(vecToTarget * force * Time.deltaTime);
                break;
        }
	}

    void LateUpdate()
    {
        if (rotateMesh != null)
        {
            float step = rotationSpeed * Time.deltaTime;
            Vector3 newDir = Vector3.RotateTowards(rotateMesh.forward, vecToTarget * multRot, step, 0.0F);
            rotateMesh.rotation = Quaternion.LookRotation(newDir);

        }
    }

    void CheckReached()
    {
        if(Vector3.Distance(currTarget.position, thisTransform.position) < 2)
        {
            GetNextIndex();
            GetCurrentTarget();
        }
    }

    void GetNextIndex()
    {
        currIndex++;
        if(currIndex >= keyPoints.Length)
        {
            currIndex = 0;
        }
    }

    void GetCurrentTarget()
    {
        currTarget = keyPoints[currIndex];
    }
}
