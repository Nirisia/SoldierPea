using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorTree;

public class SeeAllyTask : Node
{
    Unit unit;

    public SeeAllyTask(Unit _unit)
    {
        unit = _unit;
    }

    public override NodeState Evaluate()
    {
        if (unit.CanSeeAlly())
        {
            state = NodeState.SUCCESS;
            return state;
        }

        state = NodeState.FAILURE;
        return state;
    }

}
