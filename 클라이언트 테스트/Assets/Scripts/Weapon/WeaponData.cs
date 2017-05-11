// 무기 각각의 데이터 구조체

using UnityEngine;
using System;

[Serializable]
public struct WeaponData
{
    // 무기명 - 데이터베이스내에서 쉽게 확인을 위함
    public string weaponName;
    // 무기 타입
    public WeaponType weaponType;
    // 무기 공격력
    public float damage;
    // 무기 쿨타임
    public float cooltime;
    // 공격 오브젝트
    public GameObject attackObject;
    // 공격 오브젝트 생성 시간
    public float attackActiveDelay;
    // 공격 오브젝트 유지 시간
    public float attackActiveDuration;
    // 공격 오브젝트 속도
    public float attackObjectSpeed;

    public float attackUpAmount;
}
