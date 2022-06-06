using System.Collections;
using System.Collections.Generic;
using BehaviorTree;

public class AttackTask : Node
{
    Unit unit;

    public AttackTask(Unit _unit)
    {
        unit = _unit;
    }
    public override NodeState Evaluate()
    {
        if (unit.CanAttack(unit.EntityTarget) == true)
        {
            unit.ComputeAttack();
            state = NodeState.SUCCESS;
            return state;
        }

        state = NodeState.FAILURE;
        return state;

    }
}
