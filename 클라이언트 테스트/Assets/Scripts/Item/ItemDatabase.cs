// ItemData를 에셋데이터베이스로 변환, Load에 사용될 클래스
// Assets/Data/ItemDB에 저장되어 있음

using UnityEngine;
using System;

[CreateAssetMenu]
public class ItemDatabase : ScriptableObject
{
    // 시리얼라이즈된 구조체 배열
    // 에셋데이터베이스에서 확인 가능
    public ItemData[] itemDatas;

    // 인덱스를 통해서 구조체를 받아옴
    public ItemData Get(int _index)
    {
        return itemDatas[_index];
    }

    // 아이템타입을 통해서 구조체를 받아옴
    public ItemData Get(ItemType itemType)
    {
        return Array.Find(itemDatas, x => x.itemType == itemType);
    }
}
