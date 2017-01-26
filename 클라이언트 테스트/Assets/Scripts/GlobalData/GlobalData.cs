using UnityEngine;

public static class GlobalData
{
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

    #endregion


    #region [ Animator ]

    public static string ANIMATOR_PARAM_MOVE = "Move";
    public static string ANIMATOR_TAG_ATTACK = "Attack";

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

