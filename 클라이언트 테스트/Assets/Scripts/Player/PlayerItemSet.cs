using UnityEngine;
using System;
using System.Collections;

public class PlayerItemSet : MonoBehaviour
{
    public WeaponSet[] weaponSetList;
    private WeaponType currentWeaponType = WeaponType.HAND;
    private WeaponType targetWeapon = WeaponType.HAND;

    public void ActiveWeapon(WeaponType weaponType)
    {
        targetWeapon = weaponType;
    }

    private void ActiveWeaponObject(WeaponType weaponType)
    {
        for (int i = 0; i < weaponSetList.Length; i++)
        {
            weaponSetList[i].weaponObj.SetActive(weaponSetList[i].weaponType == weaponType);
        }
    }

    private void FixedUpdate()
    {
        if (currentWeaponType == targetWeapon)
            return;

        ActiveWeaponObject(targetWeapon);
        currentWeaponType = targetWeapon;
    }
}

[Serializable]
public class WeaponSet
{
    public WeaponType weaponType;
    public GameObject weaponObj;
}
