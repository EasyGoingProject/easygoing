using UnityEngine;
using System.Collections;

public class PlayerInfo : MonoBehaviour {

    public UILabel lbPlayerName;
    public UITexture sprCharacterFace;
    public UISprite sprHealth;

    public void SetPlayer(CharacterData characterData)
    {
        lbPlayerName.text = characterData.characterType.ToString();
        sprCharacterFace.mainTexture = characterData.texCharacter;
        sprHealth.fillAmount = 1.0f;
    }

    public void SetHealth(float amount)
    {
        sprHealth.fillAmount = amount;
    }
}
