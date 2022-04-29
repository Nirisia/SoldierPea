using System.Collections;
using System.Collections.Generic;
using BehaviorTree;

public class CaptureTask : Node
{
    Unit unit;

    public CaptureTask(Unit _unit)
    {
        unit = _unit;
    }

    public override NodeState Evaluate()
    {
        if (unit.CanCapture(unit.CaptureTarget))
        {
            unit.StartCapture(unit.CaptureTarget);
            state = NodeState.SUCCESS;
            return state;
        }
        state = NodeState.FAILURE;
        return state;
    }
}
