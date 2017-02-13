// 아이템 루트 오브젝트에 부착할 컴포넌트

using UnityEngine;

public class Item : MonoBehaviour
{
    [SerializeField]
    private ItemDatabase itemDatabase;

    public ItemType itemType;
    private ItemData itemData;

    void Start()
    {
        // 아이템 데이터베이스를 통해 아이템 정보를 가져옴
        itemData = itemDatabase.Get(itemType);
    }
}
