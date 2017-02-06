using UnityEngine;

public class Item : MonoBehaviour
{
    [SerializeField]
    private ItemDatabase itemDatabase;

    public ItemType itemType;
    private ItemData itemData;

    void Start()
    {
        itemData = itemDatabase.Get(itemType);
    }
}
