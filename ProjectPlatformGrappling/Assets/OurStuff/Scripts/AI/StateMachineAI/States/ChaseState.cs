using UnityEngine;
using System.Collections;

public class ChaseState : AIState {
    AIMoveable _AIMoveable;
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
        if (_AIMoveable.target != null && _AIMoveable.target.gameObject.activeSelf == true && _AIMoveable.DistanceToT(_AIMoveable.target) < _AIMoveable.maxChaseDistance)
        {
            _AIMoveable.RotateTowards(_AIMoveable.target.position);

            if (_AIMoveable.IsValidMove(_AIMoveable.target.position) && _AIMoveable.DistanceToT2D(_AIMoveable.target) > 2)
            {
                _AIMoveable.Move(_AIMoveable.target.position, _AIMoveable.currMovementSpeed);
            }
        }
        else
        {
            Exit(entity);
        }
    }
    public override void Exit(AIEntity entity)
    {

        entity.StateEnded(ChaseState.Instance);
    }

    // singleton
    private static ChaseState instance;

    private static ChaseState Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new ChaseState();
            }

            return instance;
        }
    }
}
