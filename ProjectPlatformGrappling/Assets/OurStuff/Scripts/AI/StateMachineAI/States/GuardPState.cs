using UnityEngine;
using System.Collections;

public class GuardPState : PatrolState { //patrol fast letar efter spelaren
    public override void Execute(AIEntity entity)
    {
        if (!_AIMoveable.IsValidMove(_AIMoveable.currPatrolPoint) || _AIMoveable.currPatrolPoint == Vector3.zero)
        {
            Vector3 newPatrolPoint = Vector3.zero;
            
            if (_AIMoveable.GetPatrolPoint(ref newPatrolPoint))
            {
                _AIMoveable.currPatrolPoint = newPatrolPoint;
            }
        }

        _AIMoveable.Move(_AIMoveable.currPatrolPoint, _AIMoveable.currMovementSpeed);

        if (_AIMoveable.HasReached(_AIMoveable.transform.position, _AIMoveable.currPatrolPoint, 1, false))
        {
            _AIMoveable.currPatrolPoint = Vector3.zero;
        }

        if(_AIMoveable.CheckForTarget(ref _AIMoveable.target))
        {
            Exit(entity);
        }
    }

    public override void Exit(AIEntity entity)
    {

        entity.StateEnded(GuardPState.Instance);
    }

    // singleton
    private static GuardPState instance; //finns över alla GuardPStates

    private static GuardPState Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new GuardPState();
            }

            return instance;
        }
    }

}
