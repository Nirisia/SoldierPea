using UnityEngine;

public class EntityDataScriptable : ScriptableObject
{
    [Header("Build Data")]
    public int TypeId = 0;
    public string Caption = "Unknown Unit";
    public int Cost = 1;
    public float BuildDuration = 1f;

    [Header("Health Points")]
    public int MaxHP = 100;

	[Header("Squad")]
	public float SquadAlignement		= 0.5f;
	public float SquadCohesion			= 0.5f;
	public float SquadSeparation		= 0.5f;
	public float SquadSeparationDist	= 5.0f;
	public float UnitViewAngle			= 45.0f;
	public float UnitRangeOfSight		= 100.0f;
}
