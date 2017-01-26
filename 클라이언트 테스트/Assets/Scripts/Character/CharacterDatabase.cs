using UnityEngine;
using System;
using System.Collections;

[CreateAssetMenu]
public class CharacterDatabase : ScriptableObject
{
    public CharacterData[] playerDatas;

    public CharacterData Get(int _index)
    {
        return playerDatas[_index];
    }

    public CharacterData Get(CharacterType characterType)
    {
        return Array.Find(playerDatas, x => x.characterType == characterType);
    }
}
