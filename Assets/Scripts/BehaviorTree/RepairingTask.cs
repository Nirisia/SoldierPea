using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorTree;

public class RepairingTask : Node
{
    Unit unit;

    public RepairingTask(Unit _unit)
    {
        unit = _unit;
    }
    public override NodeState Evaluate()
    {
        if (unit.CanRepair(unit.EntityTarget) == true)
        {
            unit.ComputeRepairing();
            state = NodeState.SUCCESS;
            return state;
        }
        state = NodeState.FAILURE;
        return state;
    }
}
