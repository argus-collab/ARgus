#if !UNITY_WSA
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConditionTutorial : MonoBehaviour, ICondition
{
    private int _index = 2;
    public int Index
    {
        get => _index;
        set => _index = value;
    }
    public UITutorialManager uiman;

    void ICondition.ApplyCondition()
    {
        Debug.Log("condition tutorial");
        uiman.ResetTutoDone();
        uiman.StartViews();
    }

    void ICondition.ResetCondition()
    {
        uiman.SetTutoDone();
    }
}
#endif