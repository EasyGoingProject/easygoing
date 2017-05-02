// 적 데이터 구조체

using UnityEngine;
using System;

[Serializable]
public class EnemyData
{
    // 적 명칭
    public string enemyName;
    // 적 타입
    public EnemyType enemyType;
    // 공격 타입
    public WeaponType weaponType;
    // 이동속도
    [Range(0, 2.5f)]
    public float moveSpeed;
    // 방향회전속도
    public float rotationSpeed;
    // 사정거리
    public float range;
    // 데미지
    public float power;
    // 체력
    public float health;
    // 공격 쿨타임
    public float coolTime;
    // 공격 오브젝트
    public GameObject attackObject;
    // 공격 오브젝트 생성 시간
    public float attackActiveDelay;
    // 공격 오브젝트 유지 시간
    public float attackActiveDuration;
    // 공격 오브젝트 속도
    public float attackObjectSpeed;
    // 적 프리팹
    public GameObject enemyPrefab;
}
