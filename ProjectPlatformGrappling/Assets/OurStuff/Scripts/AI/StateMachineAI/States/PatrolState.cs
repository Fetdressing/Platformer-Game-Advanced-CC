using UnityEngine;
using System.Collections;
using System;

public class PatrolState : AIState {
    protected AIMoveable _AIMoveable;

    public override void Enter(AIEntity entity)
    {
        if (entity is AIMoveable)
            _AIMoveable = entity.GetComponent<AIMoveable>();
        else
        {
            Debug.Log("Entity inte kapabel att röra på sig");
        }
    }
    public override void Execute(AIEntity entity)
    {
        if(_AIMoveable.currPatrolPoint == Vector3.zero || _AIMoveable.HasReached(_AIMoveable.transform.position, _AIMoveable.currPatrolPoint, 5, false))
        {
           _AIMoveable.GetPatrolPoint(ref _AIMoveable.currPatrolPoint);
        }
        _AIMoveable.Move(_AIMoveable.currPatrolPoint, _AIMoveable.currMovementSpeed);
    }
    public override void Exit(AIEntity entity)
    {

        entity.StateEnded(PatrolState.Instance);
    }

    // singleton
    private static PatrolState instance;

    private static PatrolState Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new PatrolState();
            }

            return instance;
        }
    }
}
