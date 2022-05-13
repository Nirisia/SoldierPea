using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorTree;

public class IsMovingTask : Node
{
    Unit unit;

    public IsMovingTask(Unit _unit)
    {
        unit = _unit;
    }

    public override NodeState Evaluate()
    {
        if (unit.NavMeshAgent.isStopped == false)
        {
            state = NodeState.SUCCESS;
            return state;
        }

        state = NodeState.FAILURE;
        return state;
    }
}
