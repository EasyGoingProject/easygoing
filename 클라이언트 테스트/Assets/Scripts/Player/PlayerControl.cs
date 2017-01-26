using UnityEngine;
using System.Collections;

public class PlayerControl : MonoBehaviour
{
    public CharacterType characterType;
    public CharacterDatabase characterDB;

    private CharacterData characterData;

    private PlayerTransform playerTransform;
    private PlayerAnimator playerAnimator;

    void Awake()
    {
        characterData = characterDB.Get(characterType);

        playerTransform = GetComponent<PlayerTransform>();
        playerAnimator = GetComponent<PlayerAnimator>();
    }

    void Start()
    {
        playerTransform.InitTransform(characterData);
        playerAnimator.InitAnimator();
    }
}
