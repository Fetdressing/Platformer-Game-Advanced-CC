using UnityEngine;
using System.Collections;

public abstract class AIState {
    public abstract void Enter(AIEntity entity);
    public abstract void Execute(AIEntity entity);
    public abstract void Exit(AIEntity entity); //säger till AIn att byta state
    public virtual void ForceExit(AIEntity entity)
    {

    }

}
