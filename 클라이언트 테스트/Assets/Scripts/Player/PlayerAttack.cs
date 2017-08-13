// 플레이어의 공격 정보 컴포넌트

using UnityEngine;
using System.Collections;

public class PlayerAttack : MonoBehaviour
{
    // 무기 정보를 가진 데이터베이스 할당
    [SerializeField]
    private WeaponDatabase weaponDB;

    // 현재 사용중인 무기의 정보
    private WeaponData currentWeaponData;
    
    // 공격력 배율
    private float power;

    // 공격위치
    public Transform attackPoint;


    #region [ Init ]

    // 컴포넌트 초기화
    public void InitAttack(float _power)
    {
        power = _power;

        // 무기가 없는 상태의 무기정보를 받아옴
        GetWeapon(WeaponType.HAND);

        // 쿨타입 업데이트
        InvokeRepeating("UpdateWeaponCool", 0.0f, GlobalData.TICKTOCK);
    }

    #endregion


    #region [ Weapon ]

    // 무기 획득
    public void GetWeapon(WeaponType _weapon)
    {
        // 무기 획득시 해당 무기의 무기정보를 받아옴
        currentWeaponData = weaponDB.Get(_weapon);
        // 무기 획득시 쿨타임 초기화
        cooltimer = .0f;
    }

    // 현재 무기의 타입 확인
    public WeaponType GetWeaponType() { return currentWeaponData.weaponType; }

    #endregion


    #region [ Update ]

    // 쿨타임
    private float cooltimer = .0f;
    // 공격가능 유무 확인 - 쿨타임파트
    private bool canAttackCool = false;

    // 쿨타입 업데이트
    private void UpdateWeaponCool()
    {
        cooltimer += GlobalData.TICKTOCK;
        // 쿨타임이 됐는지 확인
        canAttackCool = cooltimer > currentWeaponData.cooltime;
    }

    #endregion


    #region [ Attack ]

    // 공격처리
    public IEnumerator Attack()
    {
        ResetAttack();

        yield return new WaitForSeconds(currentWeaponData.attackActiveDelay);

        /* 
        // SinglePlay
        GameObject attackObj = Instantiate(currentWeaponData.attackObject) as GameObject;
        attackObj.transform.position = attackPoint.position;
        attackObj.transform.rotation = attackPoint.rotation;

        attackObj.GetComponent<PlayerAttackObject>().SetAttack(
            currentWeaponData.attackActiveDuration,
            currentWeaponData.attackObjectSpeed,
            currentWeaponData.damage * power);
        */

        GameManager.GetInstance.SendAttack(currentWeaponData.weaponType, attackPoint, power);
    }

    // 공격 초기화
    private void ResetAttack()
    {
        // 쿨타임 초기화
        cooltimer = .0f;
        canAttackCool = false;
    }

    // 공격 가능 유무 확인
    public bool CanAttack
    {
        get { return canAttackCool; }
    }

    #endregion
}


