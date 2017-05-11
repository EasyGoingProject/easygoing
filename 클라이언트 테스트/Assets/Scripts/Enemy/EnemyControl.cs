using UnityEngine;
using System.Collections;

public class EnemyControl : MonoBehaviour
{
    [Header("[ Enemy Type ]")]
    // 적 데이터베이스 : Assets/Data/EnemyDB
    public EnemyDatabase enemyDB;
    // 적 데이터베이스에서 가져올 타입
    public EnemyType enemyType;
    // 적 데이터베이스에서 가져올 Data를 할당할 변수
    private EnemyData enemyData;

    private Transform enemyBodyTrans;

    [HideInInspector]
    public NetworkSyncTransform netSyncTrans;
    [HideInInspector]
    public NetworkSyncAnimator netSyncAnimator;

    [Header("[ Enemy State ]")]
    public EnemyState enemyState = EnemyState.NONE;
    public bool isLive = false;
    public float currentHealth = .0f;

    private bool isLocalPlayer = false;

    public void Init(bool isLocal, int targetId)
    {
        enemyBodyTrans = transform;
        isLocalPlayer = isLocal;

        netSyncTrans = GetComponent<NetworkSyncTransform>();
        netSyncAnimator = GetComponent<NetworkSyncAnimator>();

        netSyncTrans.isLocalPlayer = isLocalPlayer;
        netSyncAnimator.Init(enemyAnimator, targetId, isLocalPlayer);

        InitEnemyData();

        InitLife();
        InitNavMesh();

        InitHUDText();
    }

    void Update()
    {
        if (!isLive || !isLocalPlayer)
            return;

        UpdateTrackingPlayer();
        UpdateRotation();
        UpdateAttackTimer();
        UpdateAnimator();
        UpdateHUDText();
    }


    #region [ EnemyData ] 

    private void InitEnemyData()
    {
        enemyData = enemyDB.Get(enemyType);
    }

    #endregion


    #region [ Health ]
    
    private void InitLife()
    {
        currentHealth = enemyData.health;
        isLive = true;
    }

    public void LossHealth(float amount)
    {
        currentHealth = Mathf.Clamp(currentHealth - amount, 0, currentHealth);
        isLive = !(currentHealth == 0);

        if (!isLive)
            Die();
    }

    private void Die()
    {
        UpdateHUDText();

        GetComponent<Collider>().enabled = false;
        GetComponent<Rigidbody>().isKinematic = true;
        DieAnimation();
    }

    #endregion


    #region [ Tracking Player ]

    private NavMeshAgent navMesh;
    private Transform currentTarget;
    private float distanceWithTarget;
    
    
    private void InitNavMesh()
    {
        navMesh = GetComponent<NavMeshAgent>();
        navMesh.speed = enemyData.moveSpeed;
        navMesh.stoppingDistance = enemyData.range;

        currentTarget = GameObject.FindObjectOfType<PlayerControl>().transform;
    }

    //가까운 플레이어 검색하여 타게팅하기
    private void UpdateTrackingPlayer()
    {
        if (!CanMove)
            return;

        navMesh.SetDestination(currentTarget.position);
        distanceWithTarget = Vector3.Distance(enemyBodyTrans.position, currentTarget.position);
    }

    #endregion


    #region [ Rotation ]

    private Vector3 trackingTarget;
    private Quaternion lookRotation;

    private void UpdateRotation()
    {
        if (!CanMove)
            return;

        trackingTarget = (currentTarget.position - enemyBodyTrans.position).normalized;
        lookRotation = Quaternion.LookRotation(trackingTarget);
        enemyBodyTrans.rotation = Quaternion.Lerp(enemyBodyTrans.rotation, lookRotation, Time.deltaTime * enemyData.rotationSpeed);
    }

    #endregion


    #region [ Attack ]

    [Header("[ Attack ]")]
    public Transform attackPoint;
    private bool canAttack = false;
    private float attackTimer = .0f;

    private void UpdateAttackTimer()
    {
        if (isMotionAttacking || !currentTarget)
            return;

        if (distanceWithTarget > enemyData.range)
            return;

        attackTimer += Time.deltaTime;
        if(attackTimer > enemyData.coolTime)
        {
            StartCoroutine(Attack());
            attackTimer = .0f;
        }
    }

    private IEnumerator Attack()
    {
        string attackAnimatorTrigger = GlobalData.GetAttackTrigger(enemyData.weaponType);
        enemyAnimator.SetTrigger(attackAnimatorTrigger);
        netSyncAnimator.AttackAnimation(WeaponType.HAND);

        yield return new WaitForSeconds(enemyData.attackActiveDelay);

        GameManager.GetInstance.SendEnemyAttack(enemyType, attackPoint);

        //GameObject attackObj = Instantiate(enemyData.attackObject, attackPoint.position, attackPoint.rotation) as GameObject;
        //attackObj.GetComponent<EnemyAttackObject>().SetAttack(
        //    enemyData.attackActiveDuration,
        //    enemyData.attackObjectSpeed,
        //    enemyData.power);
    }

    #endregion


    #region [ Animator ]

    [Header("[ Animator ]")]
    public Animator enemyAnimator;
    private bool isMotionAttacking = false;
    private float animatorMoveParamValue = .0f;

    private void UpdateAnimator()
    {
        isMotionAttacking = enemyAnimator.GetCurrentAnimatorStateInfo(0).IsTag(GlobalData.ANIMATOR_TAG_ATTACK);

        if (!isMotionAttacking)
            animatorMoveParamValue = Mathf.Clamp(navMesh.remainingDistance - enemyData.range, 0, 1.0f);
        else
            animatorMoveParamValue = 0;

        enemyAnimator.SetFloat(GlobalData.ANIMATOR_PARAM_MOVE, animatorMoveParamValue);
    }

    private void DieAnimation()
    {
        enemyAnimator.SetTrigger(GlobalData.TRIGGER_DIE);
        netSyncAnimator.DieAnimation();
    }

    #endregion


    #region [ Global ]

    private bool CanMove
    {
        get { return (currentTarget && !isMotionAttacking); }
    }

    #endregion


    #region [ HUD Text ]

    [Header("[ HUD ]")]
    public Transform HUDTarget;
    private HUDText enemyHUDText;
    private UISprite sprHealth;
    
    private void InitHUDText()
    {
        GameObject enemyHUDTextObj = Instantiate(
                                        UIManager.GetInstance.GetHUDTextPrefab.gameObject,
                                        UIManager.GetInstance.GetHUDRootTransform) as GameObject;

        enemyHUDTextObj.transform.localScale = Vector3.one;

        enemyHUDText = enemyHUDTextObj.GetComponent<HUDText>();

        sprHealth = enemyHUDTextObj.GetComponentInChildren<UISprite>();

        UIFollowTarget followTarget = enemyHUDTextObj.GetComponent<UIFollowTarget>();
        followTarget.target = HUDTarget;
        followTarget.uiCamera = UIManager.GetInstance.GetUICamera;
        followTarget.gameCamera = Camera.main;
    }

    private void UpdateHUDText()
    {
        sprHealth.fillAmount = currentHealth / enemyData.health;
    }

    #endregion
}
