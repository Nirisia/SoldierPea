using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorTree;

public class SeePointTask : Node
{
    Unit unit;

    public SeePointTask(Unit _unit)
    {
        unit = _unit;
    }

    public override NodeState Evaluate()
    {
        if (unit.CanSeePoint())
        {
            state = NodeState.SUCCESS;
            return state;
        }

        state = NodeState.FAILURE;
        return state;
    }
}
