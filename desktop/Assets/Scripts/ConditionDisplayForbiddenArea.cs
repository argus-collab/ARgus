using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConditionDisplayForbiddenArea : MonoBehaviour, ICondition
{
    private int _index = 4;
    public int Index
    {
        get => _index;
        set => _index = value;
    }
    private SymbolsRandomPlacement symbolsPlacer;

    void Update()
    {
        if (symbolsPlacer == null)
            symbolsPlacer = GameObject.FindObjectOfType<SymbolsRandomPlacement>(); 
    }

    void ICondition.ApplyCondition()
    {
        Debug.Log("condition hide/display forbidden area");
        if (symbolsPlacer != null)
            symbolsPlacer.displayForbiddenArea = true;
    }

    void ICondition.ResetCondition()
    {
        if (symbolsPlacer != null)
            symbolsPlacer.displayForbiddenArea = false;
    }
}
