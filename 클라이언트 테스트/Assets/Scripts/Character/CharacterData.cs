using UnityEngine;
using System;

[Serializable]
public struct CharacterData
{
    public string playerName;
    public CharacterType characterType;
    public float moveSpeed;
    public float rotateSpeed;
    public float power;
}
