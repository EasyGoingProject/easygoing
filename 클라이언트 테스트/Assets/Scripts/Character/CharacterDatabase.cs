// CharacterData를 에셋데이터베이스로 변환, Load에 사용될 클래스
// Assets/Data/CharacterDB에 저장되어 있음

using UnityEngine;
using System;

[CreateAssetMenu]
public class CharacterDatabase : ScriptableObject
{
    // 시리얼라이즈된 구조체 배열
    // 에셋데이터베이스에서 확인 가능
    public CharacterData[] playerDatas;

    // 인덱스를 통해서 구조체를 받아옴
    public CharacterData Get(int _index)
    {
        return playerDatas[_index];
    }

    // 캐릭터타입을 통해서 구조체를 받아옴
    public CharacterData Get(CharacterType characterType)
    {
        return Array.Find(playerDatas, x => x.characterType == characterType);
    }
}
