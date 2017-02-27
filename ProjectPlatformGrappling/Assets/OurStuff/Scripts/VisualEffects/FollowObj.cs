using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowObj : MonoBehaviour {
    public Transform followObj;
    public Vector3 offsetV = Vector3.zero;
    public float followSpeed = 10;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        transform.position = Vector3.Lerp(transform.position, followObj.position + offsetV, followSpeed * Time.deltaTime);
	}
}
