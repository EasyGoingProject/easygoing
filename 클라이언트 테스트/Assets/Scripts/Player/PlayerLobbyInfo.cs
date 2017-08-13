using UnityEngine;
using System.Collections;

public class PlayerLobbyInfo : MonoBehaviour
{
    public UITexture texCharacter;
    public UILabel lbPlayerName;
    public GameObject objHost;
    public UIButton btnReady;
    public UIButton btnStart;
    public UISprite sprBorder;

    public Color[] borderColor;

    public ClientData clientData;

    private bool isReadySet = false;
    private bool isAllReady = false;

    public void SetLobbyInfo(ClientData _clientData)
    {
        clientData = _clientData;

        NGUITools.SetActive(objHost, clientData.isHost);
        NGUITools.SetActive(btnStart.gameObject, false);
        NGUITools.SetActive(btnReady.gameObject, clientData.isLocalPlayer);

        lbPlayerName.text = clientData.clientName;
        texCharacter.mainTexture = UIManager.GetInstance.GetCharacterTexture(clientData.characterType);
        sprBorder.color = borderColor[0];

        isReadySet = false;
        isAllReady = false;

        EventDelegate.Add(btnReady.onClick, OnReady);
        EventDelegate.Add(btnStart.onClick, OnGameStart);
    }

    public void UpdateLobbyInfo(ClientData _clientData)
    {
        NGUITools.SetActive(objHost, clientData.isHost);
    }

    private void OnReady()
    {
        //NetworkData readyNetData = new NetworkData()
        //{
        //    senderId = IOCPManager.senderId,
        //    sendType = SendType.READY
        //};

        //IOCPManager.GetInstance.SendToServerMessage(readyNetData);

        IOCPManager.GetInstance.client.SendClientReady(true);
        ClientReady();
    }

    public void ClientReady()
    {
        clientData.isReady = true;
    }

    public void AllReady(bool _isAllReady)
    {
        if (clientData.isHost)
            isAllReady = _isAllReady;
    }

    private void OnGameStart()
    {
        IOCPManager.GetInstance.SendToServerMessage(new NetworkData()
        {
            senderId = IOCPManager.senderId,
            sendType = SendType.GAMESTART
        });
        GameManager.GetInstance.GamePlay();
    }

    private void FixedUpdate()
    {
        if (clientData == null || !gameObject.activeSelf)
            return;

        if (clientData.isReady)
        {
            if(!isReadySet)
            {
                isReadySet = true;
                sprBorder.color = borderColor[1];
                btnReady.gameObject.SetActive(false);

                IOCPManager.GetInstance.AllReadyCheck(true);
            }
        }

        if (isAllReady && !btnStart.gameObject.activeSelf)
            btnStart.gameObject.SetActive(true);
        else if (!isAllReady && btnStart.gameObject.activeSelf)
            btnStart.gameObject.SetActive(false);
    }
}
