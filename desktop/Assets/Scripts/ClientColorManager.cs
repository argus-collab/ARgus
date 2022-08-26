using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientColorManager : MonoBehaviour
{
    public CustomClientNetworkManager networkManager;

    public void ChangeColorState()
    {
        //Debug.Log("change color state");
        networkManager.SendCommandMessage("change-color-state");
    }
}
