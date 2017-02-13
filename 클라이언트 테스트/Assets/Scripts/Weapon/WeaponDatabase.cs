// WeaponData를 에셋데이터베이스로 변환, Load에 사용될 클래스
// Assets/Data/WeaponDB에 저장되어 있음

using UnityEngine;
using System;

[CreateAssetMenu]
public class WeaponDatabase : ScriptableObject
{
    // 시리얼라이즈된 구조체 배열
    // 에셋데이터베이스에서 확인 가능
    public WeaponData[] weaponDatas;

    // 인덱스를 통해서 구조체를 받아옴
    public WeaponData Get(int _index)
    {
        return weaponDatas[_index];
    }

    // 무기타입을 통해서 구조체를 받아옴
    public WeaponData Get(WeaponType weaponType)
    {
        return Array.Find(weaponDatas, x => x.weaponType == weaponType);
    }
}
