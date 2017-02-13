using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallMovement : MonoBehaviour {
    public LayerMask groundLM;
    public Transform rotater;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        Move(Vector3.right, Time.deltaTime * 20);
	}

    public void Move(Vector3 dir, float speed) //keep me on wall
    {
        RaycastHit rHit;
        float checkDistance = 25;
        //if(!Physics.Raycast(transform.position, Vector3.down, out rHit, checkDistance, groundLM))
        //{
        //    dir = Vector3.down;
        //}
        if (Physics.Raycast(transform.position, dir, out rHit, checkDistance, groundLM))
        {
            Vector3 cross1 = Vector3.Cross(rHit.normal, Vector3.up);
            Vector3 cross2 = Vector3.Cross(cross1, rHit.normal); //kommer ge mig en vector upp/ned längs med normalens face

            dir = cross2;
        }

        if(rotater != null)
        {
            Vector3 c = Vector3.Cross(Vector3.right, dir);
            //rotater.up = c;
            Vector3 upDir = Vector3.up;
            if(rHit.collider != null)
            {
                upDir = rHit.normal;
            }

            Quaternion newQuat = Quaternion.LookRotation(dir, upDir);
            if (newQuat != rotater.rotation)
            {
                rotater.rotation = Quaternion.Lerp(rotater.rotation, newQuat, speed);
            }
        }

        transform.Translate(dir * speed);
    }
}
