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
            if (unit.isCapturing == false)
            {
                unit.StartCapture(unit.CaptureTarget);
                state = NodeState.SUCCESS;
                return state;
            }
            else if (unit.CaptureTarget.GetTeam() == unit.GetTeam())
            {
                unit.isCapturing = false;
            }
        }
        state = NodeState.FAILURE;
        return state;
    }
}
