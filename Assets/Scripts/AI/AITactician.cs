using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tactician : MonoBehaviour
{
    
    [Range(0,1)]
    float Personality = 0.5f;
    
    private List<AIAction> Actions;
    private List<AIAction> Tactic;
    private UtilitySystem US;
    
}
