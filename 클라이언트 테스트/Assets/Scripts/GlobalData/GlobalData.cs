// 게임 시스템 전체에 사용될 변수를 미리 지정

public static class GlobalData
{
    #region [ System ]

    // InvokeRepeat시 사용될 업데이트 프레임 
    public const float TICKTOCK = 0.1f;

    #endregion


    // 씬명 변수로 할당
    #region [ Scene ]

    public static string SCENE_BATTLE = "Battle";

    #endregion


    // 레이어명 변수로 할당
    #region [ Layer ]

    public static string LAYER_PLAYER = "Player";
    public static string LAYER_ENEMY = "Enemy";
    public static string LAYER_FIELD = "Field";
    public static string LAYER_WEAPON = "Weapon";
    public static string LAYER_BULLET = "Bullet";
    public static string LAYER_SPAWNPOINT = "SpawnPoint";
    public static string LAYER_ITEM = "Item";
    public static string LAYER_ENEMYATTACK = "EnemyAttack";
    public static string LAYER_PLAYERATTACK = "PlayerAttack";

    #endregion


    // 태그 변수로 할당
    #region [ Tag ]

    public static string TAG_PLAYER = "Player";
    public static string TAG_GROUND = "Ground";
    public static string TAG_WALL = "Wall";
    public static string TAG_ITEM = "Item";
    public static string TAG_ENEMY = "Enemy";

    #endregion


    // 애니메이터 파라미터명 및 태그 변수로 할당
    #region [ Animator ]

    public const string TRIGGER_ATTACK_HAND = "AttackNormal";
    public const string TRIGGER_ATTACK_SPEAR = "AttackSpear";
    public const string TRIGGER_ATTACK_BOW = "AttackBow";
    public const string TRIGGER_ATTACK_THROW = "AttackThrow";

    public const string TRIGGER_JUMP = "Jump";

    public const string TRIGGER_DIE = "Die";

    public static string ANIMATOR_PARAM_MOVE = "Move";
    public static string ANIMATOR_TAG_ATTACK = "Attack";

    public static string GetAttackTrigger(WeaponType weaponType)
    {
        switch (weaponType)
        {
            case WeaponType.HAND:
                return TRIGGER_ATTACK_HAND;
            case WeaponType.BOW:
                return TRIGGER_ATTACK_BOW;
            case WeaponType.SPEAR:
                return TRIGGER_ATTACK_SPEAR;
            case WeaponType.THROW:
                return TRIGGER_ATTACK_THROW;
            default:
                return TRIGGER_ATTACK_HAND;
        }
    }

    #endregion


    // Input 버튼명 변수로 할당
    #region [ Button ]

    public static string BUTTON_FIRE = "Fire1";
    public static string BUTTON_JUMP = "Jump";

    #endregion


    #region [ ITEM ]

    public const float ITEM_HEALTH_HEAL_AMOUNT = 20.0f;

    #endregion
}


// 캐릭터명 타입
public enum CharacterType
{
    Character001 = 0,
    Character002,
    Character003,
    Character004,
    Character005,
    Character006,
    Character007
}


// 무기 타입
public enum WeaponType
{
    HAND = 0,
    SPEAR,
    BOW,
    THROW,
    SWORD
}


// 아이템 타입
public enum ItemType
{
    HEALTH = 0,
    WEAPON_SPEAR,
    WEAPON_BOW,
    WEAPON_THROW,
    WEAPON_SWORD
}


// 적 타입
public enum EnemyType
{
    NATIVECHASER
}


// 적 상태
public enum EnemyState
{
    NONE = 0,
    TRACKING,
    ATTACK,
    DIE
}
