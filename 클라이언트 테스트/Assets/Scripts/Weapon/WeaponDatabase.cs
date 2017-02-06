using UnityEngine;
using System;

[CreateAssetMenu]
public class WeaponDatabase : ScriptableObject
{
    public WeaponData[] weaponDatas;

    public WeaponData Get(int _index)
    {
        return weaponDatas[_index];
    }

    public WeaponData Get(WeaponType weaponType)
    {
        return Array.Find(weaponDatas, x => x.weaponType == weaponType);
    }
}
