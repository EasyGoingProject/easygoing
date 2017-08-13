using UnityEngine;
using System.Collections;

public class ItemControl : MonoBehaviour
{
    public ItemType itemType;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag(GlobalData.TAG_PLAYER))
        {
            PlayerControl pControl = collision.gameObject.GetComponent<PlayerControl>();
            if (pControl == null)
                return;

            if (pControl.clientData.isLocalPlayer)
            {
                switch (itemType)
                {
                    case ItemType.HEALTH:
                        IOCPManager.GetInstance.SendToServerMessage(new NetworkData()
                        {
                            senderId = IOCPManager.senderId,
                            sendType = SendType.ADDHEALTH,
                        });
                        //보낸클라이언트가 직접 처리
                        IOCPManager.myPlayerControl.AddHealth(GlobalData.ITEM_HEALTH_HEAL_AMOUNT);

                        break;

                    case ItemType.WEAPON_BOW:
                    case ItemType.WEAPON_SPEAR:
                    case ItemType.WEAPON_THROW:
                    case ItemType.WEAPON_SWORD:

                        WeaponType weaponType = WeaponType.HAND;
                        if (itemType == ItemType.WEAPON_BOW)
                            weaponType = WeaponType.BOW;
                        else if (itemType == ItemType.WEAPON_SPEAR)
                            weaponType = WeaponType.SPEAR;
                        else if (itemType == ItemType.WEAPON_SWORD)
                            weaponType = WeaponType.SWORD;
                        else if (itemType == ItemType.WEAPON_THROW)
                            weaponType = WeaponType.THROW;
                        else
                            weaponType = WeaponType.HAND;

                        IOCPManager.GetInstance.SendToServerMessage(new NetworkData()
                        {
                            senderId = IOCPManager.senderId,
                            sendType = SendType.EQUIPWEAPON,
                            weaponType = weaponType
                        });

                        //보낸 클라이언트가 직접 처리
                        IOCPManager.myPlayerControl.SetWeapon(weaponType);
                        break;
                }
            }

            Dispose();
        }
    }

    private void Dispose()
    {
        //Destroy(gameObject);
        if(IOCPManager.connectionData.isHost)
        {
            IOCPManager.GetInstance.SendToServerMessage(new NetworkData()
            {
                senderId = IOCPManager.senderId,
                targetId = GetComponent<NetworkSyncTransform>().objectNetworkId,
                sendType = SendType.DESTROY_OBJECT
            });
        }
    }
}
