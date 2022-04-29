using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AITactician : MonoBehaviour
{
    

    [Range(0,1)]
    float Personality = 0.5f;
    
    public List<AIAction> Actions;
    public Factory factory;
    
    private List<AIAction> Tactic;
    private UtilitySystem US;


    private void Start()
    {
        if (factory)
        {
            foreach (var prefab in factory.Prefabs)
            {
                Debug.Log(prefab.name);
            }
        }
    }
}
