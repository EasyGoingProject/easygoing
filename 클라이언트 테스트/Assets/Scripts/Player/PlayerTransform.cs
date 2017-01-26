using UnityEngine;
using System.Collections;

public class PlayerTransform : MonoBehaviour {

    private bool isInit = false;

    public Transform playerTrans;

    private CharacterData characterData;

    public void InitTransform(CharacterData _characterData)
    {
        characterData = _characterData;

        isInit = true;
    }

    void Update()
    {
        if (!isInit)
            return;

        if (!InputControl.isMoving)
            return;

        Vector3 targetPos = playerTrans.position;
        targetPos.x += InputControl.moveX;
        targetPos.z += InputControl.moveY;

        Vector3 lookDirect = targetPos - playerTrans.position;

        playerTrans.position = Vector3.Lerp(playerTrans.position, 
                                            targetPos, 
                                            characterData.moveSpeed * Time.deltaTime);
        playerTrans.rotation = Quaternion.Lerp(playerTrans.rotation,
                                               Quaternion.LookRotation(lookDirect),
                                               characterData.rotateSpeed * Time.deltaTime);
    }
}
