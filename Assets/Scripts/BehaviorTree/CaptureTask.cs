using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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

        if (unit.isCapturing == false 
            && unit.CaptureTarget.GetTeam() != unit.GetTeam()
            && Vector3.Distance(unit.NavMeshAgent.destination, unit.CaptureTarget.transform.position) <= unit.GetUnitData.CaptureDistanceMax)
        {
            unit.StartCapture(unit.CaptureTarget);
            state = NodeState.SUCCESS;
            return state;
        }
        else if (Vector3.Distance(unit.NavMeshAgent.destination, unit.CaptureTarget.transform.position) > unit.GetUnitData.CaptureDistanceMax
                && unit.isCapturing == true)
        {
            unit.StopCapture();
            unit.isCapturing = false;
            state = NodeState.SUCCESS;
            return state;
        }
        else if (unit.CaptureTarget.GetTeam() == unit.GetTeam())
        {
            unit.isCapturing = false;
			unit.StopCapture();
            state = NodeState.SUCCESS;
            return state;
        }

        state = NodeState.FAILURE;
        return state;
    }
}
