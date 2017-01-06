using UnityEngine;
using System.Collections;

public class VelocityMultiplier : MonoBehaviour {
    Rigidbody o_Rigidbody;
    public float multiplierValue = 2.0f;
    public float maxVelocity = 60;

    public bool stickToGround = false;
    public float stickToGroundAmount = 5;
	// Use this for initialization
	void Start () {
        o_Rigidbody = GetComponent<Rigidbody>();
	}
	
	// Update is called once per frame
	void Update () {
        Vector3 addVel = o_Rigidbody.velocity * multiplierValue * Time.deltaTime;
        if (stickToGround)
        {
            addVel.y = 0;
        }
        o_Rigidbody.velocity += addVel;

        if(o_Rigidbody.velocity.magnitude > maxVelocity)
        {
            
            if(stickToGround)
            {
                float yTemp = o_Rigidbody.velocity.y + -(stickToGroundAmount * Time.deltaTime);
                yTemp = Mathf.Min(yTemp, -stickToGroundAmount * 3);
                o_Rigidbody.velocity = o_Rigidbody.velocity.normalized * maxVelocity;
                o_Rigidbody.velocity = new Vector3(o_Rigidbody.velocity.x, yTemp, o_Rigidbody.velocity.z);
            }
            else
            {
                o_Rigidbody.velocity = o_Rigidbody.velocity.normalized * maxVelocity;
            }
        }
	}
}
