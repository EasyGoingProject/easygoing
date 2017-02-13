// 아이템 각각의 데이터 구조체

using System;

[Serializable]
public struct ItemData
{
    // 아이템명 - 데이터베이스내에서 쉽게 확인을 위함
    public string itemName;
    // 아이템 타입
    public ItemType itemType;
}
