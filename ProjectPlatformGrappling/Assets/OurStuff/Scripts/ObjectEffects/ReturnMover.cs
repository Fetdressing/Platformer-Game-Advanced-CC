using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReturnMover : BaseClass { //används till tex våger
    public Transform startPosT;
    Vector3 startPos;
    public Transform movePos;
    public Transform moveObject;

    public float moveSpeed = 10;

    public bool disableOnLap = false;
	// Use this for initialization
	void Start () {
        if (startPosT == null)
        {
            startPos = moveObject.position;
        }
        else
        {
            startPos = startPosT.position;
        }
        initTimes++;
	}
	
	// Update is called once per frame
	void Update () {
        if (gameObject.activeSelf == false) return;

        moveObject.position = Vector3.MoveTowards(moveObject.position, movePos.position, deltaTime * moveSpeed);

        if(Vector3.Distance(movePos.position, moveObject.position) < 5)
        {
            if(disableOnLap)
            {
                gameObject.SetActive(false);
            }
            moveObject.position = startPos;
        }
	}

    void OnEnable()
    {
        if (initTimes == 0) return;
        moveObject.position = startPos;
    }

    void OnDisable()
    {
        if (initTimes == 0) return;
        moveObject.position = startPos;
    }
}
