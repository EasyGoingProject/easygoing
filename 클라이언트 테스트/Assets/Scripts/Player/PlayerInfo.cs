using UnityEngine;
using System.Collections;

public class PlayerInfo : MonoBehaviour {

    public UILabel lbPlayerName;
    public UITexture sprCharacterFace;
    public UISprite sprHealth;
    public UISprite sprHost;
    public GameObject objDie;
    public Color[] playerNameColor;

    public void SetPlayer(CharacterData characterData, ClientData clientData)
    {
        lbPlayerName.text = clientData.clientName.ToString();
        lbPlayerName.color = clientData.isLocalPlayer ? playerNameColor[0] : playerNameColor[1];

        sprCharacterFace.mainTexture = characterData.texCharacter;
        sprHealth.fillAmount = 1.0f;
        sprHost.gameObject.SetActive(clientData.isHost);

        objDie.SetActive(false);
    }

    public void HideHost()
    {
        sprHost.gameObject.SetActive(false);
    }

    public void UpdatePlayerInfo(ClientData clientData)
    {
        sprHost.gameObject.SetActive(clientData.isHost);
    }

    public void SetHealth(float amount)
    {
        sprHealth.fillAmount = amount;
    }

    public void SetDie()
    {
        objDie.SetActive(true);
    }
}
