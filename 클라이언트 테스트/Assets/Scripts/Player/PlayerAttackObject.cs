using UnityEngine;

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
    }

    void FixedUpdate()
    {
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isHit || !IOCPManager.connectionData.isHost)//호스트가 처리한다.
            return;

        //******if구문 안과 else if 구문 안, 모든 경우에 해당하는 내용*****
        /* 비동기 통신에서 순서는 통제되지 않는다. 이 점이 매우 중요하다.
         * 아래의 경우에서는 히트 처리를 하고 Dispose를 하도록 명령했다.
         * 그러나 비동기는 그러한 순서를 인식하지 않고 보낸다.
         * 즉, 경우에 따라 Dispose가 먼저 보내져 PlayerAttackObject가 지워지고 히트처리를 하게 될 수도 있다.
         * 두 컴퓨터가 접속해서 공격했을 때 문제가 생기는 이유들 중 하나가 되기에 충분하다.
         * 해결 : SendToServerMessage메소드의 동작을 바꾼다. send packet queue라는 대기열을 만들어
         * 요청한 순서대로 데이터가 보내질 수 있도록 한다.
        */

        if (other.gameObject.CompareTag(GlobalData.TAG_PLAYER))
        {
            PlayerControl pControl = other.gameObject.GetComponent<PlayerControl>();
            if (pControl == null || createPlayerId == pControl.clientData.clientNumber)
                return;

            isHit = true;

            NetworkData hitData = new NetworkData()
            {
                senderId = createPlayerId,
                sendType = SendType.HIT,
                targetId = pControl.clientData.clientNumber,
                power = damage,
            };
            IOCPManager.GetInstance.SendToServerMessage(hitData);
            //판단 주체 호스트 클라는 자신이 직접 처리
            pControl.LossHealth(damage, createPlayerId);

            Dispose();
        }
        else if (other.gameObject.CompareTag(GlobalData.TAG_ENEMY))
        {
            EnemyControl eControl = other.gameObject.GetComponent<EnemyControl>();
            if (eControl == null)
                return;

            isHit = true;

            IOCPManager.GetInstance.SendToServerMessage(new NetworkData()
            {
                senderId = createPlayerId,
                sendType = SendType.ENEMY_HIT,
                targetId = eControl.enemyID,
                power = damage,
            });
            //호스트는 자신이 직접 처리
            eControl.LossHealth(damage, createPlayerId);

            Dispose();
        }
    }

    private void Dispose()
    {
        //Destroy(gameObject);
        if (IOCPManager.connectionData.isHost)
        {
            //호스트는 자신이 직접 제거
            Destroy(this);
            IOCPManager.GetInstance.SendToServerMessage(new NetworkData()
            {
                senderId = IOCPManager.senderId,
                targetId = GetComponent<NetworkSyncTransform>().objectNetworkId,
                sendType = SendType.DESTROY_OBJECT,
            });
        }
    }
}
