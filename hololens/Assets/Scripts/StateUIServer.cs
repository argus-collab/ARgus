using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StateUIServer : MonoBehaviour
{
    [Header("Logs")]
    public LogManager log;
    public Text console;

    [Header("State")]
    public NetworkChangeCondition conditions;
    public string condition1Text;
    public string condition2Text;
    public Text conditionState;

    [Header("Buttons")]
    public Button toCondition1Btn;
    public Button toCondition2Btn;

    void Update()
    {
        console.text = log.GetLogsAsString();

        if (conditions.GetIndex() == 0)
        {
            conditionState.text = condition1Text;
            toCondition1Btn.interactable = false;
            toCondition2Btn.interactable = true;
        }
        else if (conditions.GetIndex() == 1)
        {
            conditionState.text = condition2Text;
            toCondition1Btn.interactable = true;
            toCondition2Btn.interactable = false;
        }


    }
}
