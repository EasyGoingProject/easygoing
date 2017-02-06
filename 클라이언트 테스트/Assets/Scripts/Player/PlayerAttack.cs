using UnityEngine;
using System.Collections;

public class PlayerAttack : MonoBehaviour
{
    private WeaponData weaponData;
    [SerializeField]
    private WeaponDatabase weaponDB;

    private float cooltimer = .0f;
    private bool canAttackCool = false;

    public void InitWeapon()
    {
        GetWeapon(WeaponType.NONE);

        InvokeRepeating("UpdateWeaponCool", 0.0f, GlobalData.TICKTOCK);
    }

    public void GetWeapon(WeaponType _weapon)
    {
        weaponData = weaponDB.Get(_weapon);
        cooltimer = .0f;
    }

    private void UpdateWeaponCool()
    {
        cooltimer += GlobalData.TICKTOCK;
        canAttackCool = cooltimer > weaponData.cooltime;
    }

    public void Attack()
    {
        ResetAttack();
    }

    private void ResetAttack()
    {
        cooltimer = .0f;
        canAttackCool = false;
    }

    public bool CanAttack { get { return canAttackCool; } }
    public WeaponType GetWeaponType() { return weaponData.weaponType; }
}


