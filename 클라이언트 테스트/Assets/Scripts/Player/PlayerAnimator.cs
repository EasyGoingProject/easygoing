using UnityEngine;
using System.Collections;

public class PlayerAnimator : MonoBehaviour
{
    private bool isInit = false;

    public Animator playerAnimator;

    public void InitAnimator()
    {
        isInit = true;
    }

    void Update()
    {
        if (!isInit)
            return;

        playerAnimator.SetFloat(GlobalData.ANIMATOR_PARAM_MOVE, InputControl.isMoving ? 0.3f : 0);
    }
}
