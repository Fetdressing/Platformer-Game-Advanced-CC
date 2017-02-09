using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReturnMover : BaseClass { //används till tex våger
    public Transform startPosT;
    Vector3 startPos;
    public Transform movePos;
    public Transform moveObject;
    public GameObject disableRootObj; //ifall man har en root som oxå ska disablas

    public float moveSpeed = 10;

    public bool disableOnLap = false;

	// Use this for initialization
	void Start () {
        if (startPosT == null)
        {
            startPos = moveObject.localPosition;
        }
        else
        {
            startPos = startPosT.localPosition;
        }

        moveObject.localPosition = startPos;

        initTimes++;
	}
	
	// Update is called once per frame
	void Update () {
        //if (gameObject.activeSelf == false) return;

        moveObject.localPosition = Vector3.MoveTowards(moveObject.localPosition, movePos.localPosition, deltaTime * moveSpeed);

        if(Vector3.Distance(movePos.localPosition, moveObject.localPosition) < 5)
        {
            if(disableOnLap)
            {
                if(disableRootObj != null)
                {
                    disableRootObj.SetActive(false);
                }
                gameObject.SetActive(false);
            }
            moveObject.localPosition = startPos;
        }
	}

    void OnEnable()
    {
        if (initTimes == 0) return;
        moveObject.localPosition = startPos;
    }

    void OnDisable()
    {
        if (initTimes == 0) return;
        //moveObject.localPosition = startPos;
    }
}
