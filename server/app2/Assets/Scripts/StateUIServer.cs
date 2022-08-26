using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StateUIServer : MonoBehaviour
{
    public WebRTCNetworkCommunication net;

    [Header("Logs")]
    public LogManager log;
    public Text console;

    [Header("State")]
    public NetworkChangeCondition conditions;
    
    public List<string> conditionTexts;
    public List<int> conditionIndexes;
    //public string condition1Text;
    //public string condition2Text;
    //public string condition3Text;
    //public string condition4Text;
    public Text conditionState;

    [Header("Buttons")]
    public List<Button> toConditionButton;
    //public Button toCondition1Btn;
    //public Button toCondition2Btn;
    //public Button toCondition3Btn;
    //public Button toCondition4Btn;
    public Button startTimer;
    public Button stopTimer;

    void Update()
    {
        console.text = log.GetLogsAsString();

        for (int i = 0; i < toConditionButton.Count; ++i)
        {
            if (conditions.GetIndex() == conditionIndexes[i])
            {
                toConditionButton[i].interactable = false;
                conditionState.text = conditionTexts[i];
            }
            else
                toConditionButton[i].interactable = true;
        }

        //if (conditions.GetIndex() == 0)
        //{
        //    conditionState.text = condition1Text;
        //    toCondition1Btn.interactable = false;
        //    toCondition2Btn.interactable = true;
        //    toCondition3Btn.interactable = true;
        //    toCondition4Btn.interactable = true;
        //}
        //else if (conditions.GetIndex() == 1)
        //{
        //    conditionState.text = condition2Text;
        //    toCondition1Btn.interactable = true;
        //    toCondition2Btn.interactable = false;
        //    toCondition3Btn.interactable = true;
        //    toCondition4Btn.interactable = true;
        //}
        //else if (conditions.GetIndex() == 2)
        //{
        //    conditionState.text = condition3Text;
        //    toCondition1Btn.interactable = true;
        //    toCondition2Btn.interactable = true;
        //    toCondition3Btn.interactable = false;
        //    toCondition4Btn.interactable = true;
        //}
        //else if (conditions.GetIndex() == 3)
        //{
        //    conditionState.text = condition4Text;
        //    toCondition1Btn.interactable = true;
        //    toCondition2Btn.interactable = true;
        //    toCondition3Btn.interactable = true;
        //    toCondition4Btn.interactable = false;
        //}
    }

    public void StartTimer()
    {
        net.SendNetworkEvent("startTimer");
        startTimer.interactable = false;
        stopTimer.interactable = true;
    }

    public void StopTimer()
    {
        net.SendNetworkEvent("stopTimer");
        startTimer.interactable = true;
        stopTimer.interactable = false;
    }
}
