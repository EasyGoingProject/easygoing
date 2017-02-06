using UnityEngine;
using System;

[CreateAssetMenu]
public class ItemDatabase : ScriptableObject
{
    public ItemData[] itemDatas;

    public ItemData Get(int _index)
    {
        return itemDatas[_index];
    }

    public ItemData Get(ItemType itemType)
    {
        return Array.Find(itemDatas, x => x.itemType == itemType);
    }
}
