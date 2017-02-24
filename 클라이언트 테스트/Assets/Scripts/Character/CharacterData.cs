// 캐릭터 각각의 데이터 구조체

using System;
using UnityEngine;

[Serializable]
public struct CharacterData
{
    // 해당 캐릭터 명 - 모델링마다 캐릭터네임을 달리 설정
    public string playerName;
    // 한개의 캐릭터 모델링마다 지정될 캐릭터 타입
    public CharacterType characterType;
    // 해당 캐릭터의 이동속도
    public float moveSpeed;
    // 해당 캐릭터의 회전속도
    public float rotateSpeed;
    // 해당 캐릭터의 기본데미지
    public float power;
    // 해당 캐릭터의 체력
    public float health;
    // 해당 캐릭터 얼굴
    public Texture2D texCharacter;
}
