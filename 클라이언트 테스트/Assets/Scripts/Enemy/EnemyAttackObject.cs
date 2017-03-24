using UnityEngine;

public class EnemyAttackObject : MonoBehaviour
{
    private float duration;
    private float speed;
    private float damage;

    private Transform attackTrans;
    private bool isNonMove = false;
    private bool isHit = false;

    private float lifeTimer = 0;

    public void SetAttack(float _duration, float _speed, float _damage)
    {
        attackTrans = transform;

        duration = _duration;
        speed = _speed;
        damage = _damage;

        isNonMove = !(speed > 0);
    }

    void Update()
    {
        lifeTimer += Time.deltaTime;

        if (lifeTimer > duration)
            Dispose();

        if (isNonMove)
            return;

        attackTrans.Translate(attackTrans.forward * Time.deltaTime * speed, Space.Self);
    }

    private void OnCollisionEnter(Collision col)
    {
        if (isHit)
            return;
        if (col.gameObject.layer == LayerMask.NameToLayer(GlobalData.LAYER_PLAYER)
            && col.gameObject.CompareTag(GlobalData.TAG_PLAYER))
        {
            isHit = true;

            col.gameObject.GetComponent<PlayerControl>().LossHealth(damage);
            Dispose();
        }
    }

    private void Dispose()
    {
        Destroy(gameObject);
    }
}

