using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorTree;

public class SeeEnemyTask : Node
{
    Unit unit;

    public SeeEnemyTask(Unit _unit)
    {
        unit = _unit;
    }

    public override NodeState Evaluate()
    {
        if (unit.CanSeeEnemy())
        {
            state = NodeState.SUCCESS;
            return state;
        }

        state = NodeState.FAILURE;
        return state;
    }
}
