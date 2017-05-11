using UnityEngine;
using System.Collections;

public class UIManager : Singleton<UIManager>
{
    private void Awake()
    {
        InitPlayerInfo();
        InitPanel();
    }

    private void Update()
    {
        UpdateCheckTargetPanel();
    }

    #region [ Camera ] 

    public Camera uiCamera;

    public Camera GetUICamera
    {
        get { return uiCamera; }
    }

    #endregion


    #region [ Panel Control ]

    [Header("[Panel Control]")]
    public GameObject[] panelObjs;
    public PanelType currentPanelType;
    private PanelType targetPanelType = PanelType.None;

    private void InitPanel()
    {
        ShowPanel(PanelType.Connection);
    }

    private void UpdateCheckTargetPanel()
    {
        if (targetPanelType != PanelType.None)
        {
            ShowPanel(targetPanelType);
            targetPanelType = PanelType.None;
        }
    }

    public void SetTargetPanel(PanelType _targetPanel)
    {
        targetPanelType = _targetPanel;
    }

    public void ShowPanel(PanelType panelType)
    {
        currentPanelType = panelType;

        for(int i=0; i< panelObjs.Length - 1; i++)
        {
            panelObjs[i].SetActive(i == (int)panelType);
        }
    }

    #endregion


    #region [ Player Info ]

    [Header("[ Player Info ]")]
    [SerializeField]
    private UIGrid gridPlayerInfo;
    [SerializeField]
    private PlayerInfo playerInfoPrefab;
    public PlayerLobbyInfo[] playerLobbyInfos;

    private void InitPlayerInfo()
    {
        for (int i = 0; i < playerLobbyInfos.Length; i++)
            NGUITools.SetActive(playerLobbyInfos[i].gameObject, false);
    }

    public PlayerInfo AddPlayerInfo(CharacterData characterData, ClientData clientData)
    {
        PlayerInfo playerInfo = Instantiate(playerInfoPrefab, gridPlayerInfo.transform) as PlayerInfo;
        playerInfo.transform.localScale = Vector3.one;
        playerInfo.SetPlayer(characterData, clientData);

        gridPlayerInfo.Reposition();

        return playerInfo;
    }

    public PlayerLobbyInfo AddPlayerLobbyInfo(ClientData clientData)
    {
        NGUITools.SetActive(playerLobbyInfos[clientData.clientIndex].gameObject, true);
        playerLobbyInfos[clientData.clientIndex].SetLobbyInfo(clientData);
        return playerLobbyInfos[clientData.clientIndex];
    }

    #endregion


    #region [ Character Texture ]

    [Header("[ Character Texture ]")]
    public CharacterDatabase characterDB;

    public Texture2D GetCharacterTexture(CharacterType _charType)
    {
        return characterDB.Get(_charType).texCharacter;
    }


    #endregion


    #region [ HUD ]

    [Header("[ HUD ]")]
    [SerializeField]
    private HUDRoot HUDRoot;
    [SerializeField]
    private HUDText HUDTextPrefab;


    public HUDText GetHUDTextPrefab
    {
        get { return HUDTextPrefab; }
    }

    public Transform GetHUDRootTransform
    {
        get { return HUDRoot.transform; }
    }

    #endregion
}

public enum PanelType
{
    Connection = 0,
    Lobby,
    Play,
    Result,
    None
}
