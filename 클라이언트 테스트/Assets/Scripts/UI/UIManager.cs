using UnityEngine;
using System.Collections;

public class UIManager : Singleton<UIManager> {

    public Camera uiCamera;

    public Camera GetUICamera
    {
        get { return uiCamera; }
    }


    #region [ Player Info ]

    [Header("[ Player Info ]")]
    [SerializeField]
    private UIGrid gridPlayerInfo;
    [SerializeField]
    private PlayerInfo playerInfoPrefab;

    public PlayerInfo AddPlayerInfo(CharacterData characterData)
    {
        PlayerInfo playerInfo = Instantiate(playerInfoPrefab, gridPlayerInfo.transform) as PlayerInfo;
        playerInfo.transform.localScale = Vector3.one;

        gridPlayerInfo.Reposition();

        return playerInfo;
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
