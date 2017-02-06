using UnityEngine;

public static class GlobalData
{
    #region [ System ]

    public const float TICKTOCK = 0.1f;

    #endregion


    #region [ Scene ]

    public static string SCENE_BATTLE = "Battle";

    #endregion


    #region [ Layer ]

    public static string LAYER_PLAYER = "Player";
    public static string LAYER_ENEMY = "Enemy";
    public static string LAYER_FIELD = "Field";
    public static string LAYER_WEAPON = "Weapon";
    public static string LAYER_BULLET = "Bullet";

    #endregion


    #region [ Tag ]

    public static string TAG_PLAYER = "Player";
    public static string TAG_GROUND = "Ground";
    public static string TAG_WALL = "Wall";
    public static string TAG_ITEM = "Item";

    #endregion


    #region [ Animator ]

    public const string TRIGGER_ATTACK_NONE = "AttackNormal";
    public const string TRIGGER_ATTACK_SPEAR = "AttackSpear";
    public const string TRIGGER_ATTACK_BOW = "AttackBow";
    public const string TRIGGER_ATTACK_THROW = "AttackThrow";

    public const string TRIGGER_JUMP = "Jump";

    public const string TRIGGER_DIE = "Die";

    public static string ANIMATOR_PARAM_MOVE = "Move";
    public static string ANIMATOR_TAG_ATTACK = "Attack";

    #endregion


    #region [ Button ]

    public static string BUTTON_FIRE = "Fire1";
    public static string BUTTON_JUMP = "Jump";

    #endregion
}

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

public enum WeaponType
{
    NONE = 0,
    SPEAR,
    BOW,
    THROW
}

public enum ItemType
{
    HEALTH = 0,
    WEAPON_SPEAR,
    WEAPON_BOW,
    WEAPON_THROW
}

