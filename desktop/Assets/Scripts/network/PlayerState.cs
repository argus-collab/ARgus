using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class PlayerState : NetworkBehaviour
{
    [SyncVar]
    public string playerName;
    //private string lastPlayerName;
    public Text playerNameDisplay;

    [SyncVar]
    public int arrowIndex;
    //private int lastArrowIndex;
    public List<Sprite> arrows;
    public SpriteRenderer arrowRend;

    [SyncVar]
    public string cameraMasterName;
    //private string lastCameraMasterName;
    public NetworkPlayer playerCamera;

    private void Awake()
    {
        playerNameDisplay.text = playerName;
        arrowRend.sprite = arrows[arrowIndex];
        playerCamera.mainCameraName = cameraMasterName;
    }

    void Update()
    {
        if (isServer)
            return;
    }

    public void UpdatePlayerData(string playerName, int arrowIndex, string cameraMasterName)
    {
        Init(playerName, arrowIndex, cameraMasterName);

        if(isClient)
            CmdInit(playerName, arrowIndex, cameraMasterName);

        if (isServer)
            RpcInit(playerName, arrowIndex, cameraMasterName);
    }

    void Init(string playerName, int arrowIndex, string cameraMasterName)
    {
        this.playerName = playerName;
        this.arrowIndex = arrowIndex;
        this.cameraMasterName = cameraMasterName;

        playerNameDisplay.text = playerName;
        arrowRend.sprite = arrows[arrowIndex];
        playerCamera.mainCameraName = cameraMasterName;

        //lastPlayerName = playerName;
        //lastArrowIndex = arrowIndex;
        //lastCameraMasterName = cameraMasterName;
    }

    [Command]
    private void CmdInit(string playerName, int arrowIndex, string cameraMasterName)
    {
        Init(playerName, arrowIndex, cameraMasterName);
    }

    [ClientRpc]
    private void RpcInit(string playerName, int arrowIndex, string cameraMasterName)
    {
        Init(playerName, arrowIndex, cameraMasterName);
    }
}
