using UnityEngine;
using System.Collections;

public class AreaDeactivateTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (!IOCPManager.connectionData.isHost)
            return;

        if (other.gameObject.CompareTag(GlobalData.TAG_PLAYER))
        {
            PlayerControl pControl = other.gameObject.GetComponent<PlayerControl>();
            if (pControl == null)
                return;

            IOCPManager.GetInstance.SendToServerMessage(new NetworkData()
            {
                senderId = -1,
                sendType = SendType.HIT,
                targetId = pControl.clientData.clientNumber,
                power = 100000,
            });
        }
    }
}
