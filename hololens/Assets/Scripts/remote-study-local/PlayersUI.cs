using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class PlayersUI : MonoBehaviour
{
    public Text localPlayersTextUI;
    public Text remotePlayersTextUI;

    public GameObject localPlayersGO;
    public GameObject remotePlayersGO;

    void UpdateLocalPlayersTextUI()
    {
        localPlayersTextUI.text = "";

        List<string> doneIp = new List<string>();

        for (int i = 0; i < localPlayersGO.transform.childCount; ++i)
        {
            GameObject go = localPlayersGO.transform.GetChild(i).gameObject;
            string childIP = go.name.Split('-')[0];

            if(!doneIp.Contains(childIP))
            {
                localPlayersTextUI.text += "\n";
                localPlayersTextUI.text += childIP;

                for (int j = 0; j < localPlayersGO.transform.childCount; ++j)
                {
                    GameObject child = localPlayersGO.transform.GetChild(j).gameObject;

                    if (child.name.Contains("Head") && child.name.Contains(childIP))
                    {
                        //::ffff:192.168.0.100-Head
                        localPlayersTextUI.text += "\n";
                        localPlayersTextUI.text += "\t> head loaded";
                    }

                    if (child.name.Contains("RightHand-wrist") && child.name.Contains(childIP))
                    {
                        localPlayersTextUI.text += "\n";
                        localPlayersTextUI.text += "\t> right hand loaded";
                    }

                    if (child.name.Contains("LeftHand-wrist") && child.name.Contains(childIP))
                    {
                        localPlayersTextUI.text += "\n";
                        localPlayersTextUI.text += "\t> left hand loaded";
                    }
                }

                doneIp.Add(childIP);
            }
        }
    }

    void UpdateRemotePlayersTextUI()
    {
        remotePlayersTextUI.text = "";

        for (int i = 0; i < remotePlayersGO.transform.childCount; ++i)
        {
            GameObject go = remotePlayersGO.transform.GetChild(i).gameObject;
            remotePlayersTextUI.text += "\n";
            remotePlayersTextUI.text += go.transform.name;
        }
    }

    void Update()
    {
        UpdateLocalPlayersTextUI();
        UpdateRemotePlayersTextUI();
    }
}
