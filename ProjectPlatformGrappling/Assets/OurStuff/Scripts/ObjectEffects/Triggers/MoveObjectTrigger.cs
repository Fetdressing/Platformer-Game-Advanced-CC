using UnityEngine;
using System.Collections;

public class MoveObjectTrigger : Trigger {

    public Transform moveObject;
    private Rigidbody moveRigidbody;
    public float moveSpeedHome = 9.0f;
    public float moveSpeedTrigger = 15.0f;
    private float currMoveSpeed;

    private Vector3 currMoveposition;
    public Transform idlePosition;
    public Transform triggerPosition;

    public override void Init()
    {
        base.Init();
        moveRigidbody = moveObject.GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        if (initTimes == 0) return;

        ToggleTrigger(GetTriggered());
        if(Vector3.Distance(moveObject.position, currMoveposition) > 3.0f)
        {
            Vector3 toTarget = (currMoveposition - moveObject.position).normalized;
            moveRigidbody.MovePosition(moveObject.position + toTarget * Time.deltaTime * currMoveSpeed);
        }
    }

    public override void StartTrigger()
    {
        base.StartTrigger();
        currMoveSpeed = moveSpeedTrigger;
        currMoveposition = triggerPosition.position;
    }

    public override void ExitTrigger()
    {
        base.ExitTrigger();
        currMoveSpeed = moveSpeedHome;
        currMoveposition = idlePosition.position;
    }
}
