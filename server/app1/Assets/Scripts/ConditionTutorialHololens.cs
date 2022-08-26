#if !UNITY_WSA
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConditionTutorialHololens : MonoBehaviour, ICondition
{
    private int _index = 3;
    public int Index
    {
        get => _index;
        set => _index = value;
    }
    public UITutorialManager uiman;
    public ConditionHololens conditionHololens;
    public SpotGenerator spots;

    //private SymbolsRandomPlacement symbolsPlacer;

    void Update()
    {
        //if (symbolsPlacer == null)
        //    symbolsPlacer = GameObject.FindObjectOfType<SymbolsRandomPlacement>();
    }

    void ICondition.ApplyCondition()
    {
        conditionHololens.SetHololensCondition();

        Debug.Log("condition tutorial hololens");
        uiman.ResetTutoDone();
        uiman.StartViews();

        //if (symbolsPlacer != null)
        //    symbolsPlacer.Initialize();

        spots.SetTuto(true);
    }

    void ICondition.ResetCondition()
    {
        uiman.SetTutoDone();
        spots.SetTuto(false);
        spots.Change();
    }
}
#endif