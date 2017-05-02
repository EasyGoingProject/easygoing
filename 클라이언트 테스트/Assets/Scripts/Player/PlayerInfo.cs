using UnityEngine;
using System.Collections;

public class PlayerInfo : MonoBehaviour {

    public UILabel lbPlayerName;
    public UITexture sprCharacterFace;
    public UISprite sprHealth;
    public UISprite sprHost;

    public void SetPlayer(CharacterData characterData, ClientData clientData)
    {
        lbPlayerName.text = clientData.clientName.ToString();
        sprCharacterFace.mainTexture = characterData.texCharacter;
        sprHealth.fillAmount = 1.0f;
        sprHost.gameObject.SetActive(clientData.isHost);
    }

    public void SetHealth(float amount)
    {
        sprHealth.fillAmount = amount;
    }
}
