using UnityEngine;
using System.Collections;

public class StatePatternAI {
    protected AIEntity entity;
    protected AIState currState;
	// Use this for initialization

    public StatePatternAI(AIEntity entityAI)
    {
        entity = entityAI;
    }

    public void UpdateLoop()
    {
        if (entity == null) return;
        if (currState != null)
        {
            currState.Execute(entity);
        }
    }

    public void ChangeState(AIState newState)
    {
        if (currState != null)
        {
            currState.ForceExit(entity); //ForceExit för annars blir det en endless loop om de inte endar sig själva
        }
        currState = newState;
        currState.Enter(entity);
    }
}
