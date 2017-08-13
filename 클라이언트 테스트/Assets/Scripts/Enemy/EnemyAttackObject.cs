using UnityEngine;

public class EnemyAttackObject : MonoBehaviour
{
    private float duration;
    private float speed;
    private float damage;

    private Transform attackTrans;
    private Rigidbody attackRigid;
    private bool isNonMove = false;
    private bool isHit = false;

    private float lifeTimer = 0;

    public void SetAttack(float _duration, float _speed, float _damage)
    {
        attackTrans = transform;
        attackRigid = GetComponent<Rigidbody>();

        duration = _duration;
        speed = _speed;
        damage = _damage;

        isNonMove = !(speed > 0);

        if (!isNonMove)
        {
            Vector3 attackDirect = attackTrans.forward;
            attackRigid.AddForce(attackDirect * speed, ForceMode.Impulse);
        }
    }

    void Update()
    {
        lifeTimer += Time.deltaTime;

        if (lifeTimer > duration)
            Dispose();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isHit || !IOCPManager.connectionData.isHost)
            return;

        if (other.gameObject.CompareTag(GlobalData.TAG_PLAYER))
        {
            PlayerControl pControl = other.gameObject.GetComponent<PlayerControl>();
            if (pControl == null)
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
        IOCPManager.GetInstance.SendToServerMessage(new NetworkData()
        {
            senderId = IOCPManager.senderId,
            targetId = GetComponent<NetworkSyncTransform>().objectNetworkId,
            sendType = SendType.DESTROY_OBJECT
        });
    }
}

