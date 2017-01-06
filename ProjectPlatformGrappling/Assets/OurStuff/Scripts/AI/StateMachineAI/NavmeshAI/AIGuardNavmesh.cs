using UnityEngine;
using System.Collections;

public class AIGuardNavmesh : AINavmesh {


    public override void StateEnded(AIState endedState)
    {
        if (endedState is GuardPState)
        {
            statePattern.ChangeState(chaseState);
        }
        else if (endedState is ChaseState)
        {
            statePattern.ChangeState(guardPState);
        }
    }
}
