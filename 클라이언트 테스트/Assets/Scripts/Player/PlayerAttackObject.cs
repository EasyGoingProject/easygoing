using UnityEngine;
using System.Collections;

public class PlayerAttackObject : MonoBehaviour
{
    private float duration;
    private float speed;
    private float damage;

    private Transform attackTrans;
    private Rigidbody attackRigid;
    private bool isNonMove = false;
    private bool isHit = false;

    private float lifeTimer = 0;

    private int createPlayerId;

    public void SetAttack(int senderId, float _duration, float _speed, float _damage, float _upAmount)
    {
        attackTrans = transform;
        attackRigid = GetComponent<Rigidbody>();

        createPlayerId = senderId;
        duration = _duration;
        speed = _speed;
        damage = _damage;

        isNonMove = !(speed > 0);

        if (!isNonMove)
        {
            Vector3 attackDirect = attackTrans.forward;
            attackDirect.y += _upAmount;
            attackRigid.AddForce(attackDirect * speed, ForceMode.Impulse);
        }
    }

    void Update()
    {
        lifeTimer += Time.deltaTime;

        if (lifeTimer > duration)
            Dispose();

        //if (isNonMove)
        //    return;

        //attackTrans.Translate(attackTrans.forward * Time.deltaTime * speed, Space.Self);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isHit || !IOCPManager.connectionData.isHost)
            return;

        //col.gameObject.CompareTag(GlobalData.TAG_ENEMY)
        if (other.gameObject.CompareTag(GlobalData.TAG_PLAYER))
        {
            PlayerControl pControl = other.gameObject.GetComponent<PlayerControl>();
            if (pControl == null || createPlayerId == pControl.clientData.clientNumber)
                return;

            isHit = true;

            IOCPManager.GetInstance.SendToServerMessage(new NetworkData()
            {
                senderId = IOCPManager.senderId,
                sendType = SendType.HIT,
                targetId = pControl.clientData.clientNumber,
                power = damage,
            });

            Dispose();
        }
    }

    private void Dispose()
    {
        //Destroy(gameObject);

        IOCPManager.GetInstance.SendToServerMessage(new NetworkData()
        {
            senderId = IOCPManager.senderId,
            targetId = GetComponent<NetworkSyncTransform>().objectNetworkId,
            sendType = SendType.DESTORY_OBJECT
        });
    }
}
