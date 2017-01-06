using UnityEngine;
using System.Collections;

public class AIPlatformer : AICharacterController
{

    protected float upVelocity = 0;
    protected float forwardDistanceCheck = 10;

    public override void Init()
    {
        patrolState = new PatrolState(); //dessa behöver vara innan base.init, för där kallas reset
        chaseState = new ChaseState();
        guardPState = new GuardPState();

        base.Init();

    }

    public override void Reset()
    {
        base.Reset();
        statePattern.ChangeState(guardPState);
        upVelocity = -gravity * 20; //ett högt värde så denna faller ned dirr
    }

    public override void Update()
    {
        base.Update();
        upVelocity -= gravity * Time.deltaTime;

        if (cController.isGrounded)
        {
            if (upVelocity < -gravity)
            {
                upVelocity = -gravity; //noll ställ den inte helt, så den hela tiden åker ned sakta
            }
        }

        Vector3 upVector = new Vector3(0, upVelocity * Time.deltaTime, 0);

        cController.Move(upVector);
    }
    //funktioner som kan användas

    public override void Move(Vector3 pos, float speed)
    {
        Vector3 modPos = new Vector3(pos.x, 0, pos.z);
        Vector3 modTPos = new Vector3(transform.position.x, 0, transform.position.z);

        Vector3 dir = (modPos - modTPos).normalized; //vill inte den ska röra sig upp o ned genom dessa vektorer

        RotateTowards(pos);

        cController.Move((dir * speed) * Time.deltaTime);
    }

    public override void StateEnded(AIState endedState)
    {
        if(endedState is GuardPState)
        {
            statePattern.ChangeState(chaseState);
        }
        else if (endedState is ChaseState)
        {
            statePattern.ChangeState(guardPState);
        }
    }
}
