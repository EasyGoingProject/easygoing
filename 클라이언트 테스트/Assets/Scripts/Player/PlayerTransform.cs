using UnityEngine;
using System.Collections;

public class PlayerTransform : MonoBehaviour {

    private bool isInit = false;

    public Transform playerTrans;
    public Rigidbody playerRigid;

    public float jumpForce = 10.0f;
    public bool isJump = false;
    
    private CharacterData characterData;
    

    private RaycastHit hit;

    public void InitTransform(CharacterData _characterData)
    {
        characterData = _characterData;

        isInit = true;
    }

    public void UpdateTransform()
    {
        if (!isInit)
            return;

        Vector3 targetPos = playerTrans.position;
        targetPos.x += InputControl.MoveX;
        targetPos.z += InputControl.MoveY;

        Vector3 lookDirect = targetPos - playerTrans.position;

        playerTrans.position = Vector3.Lerp(playerTrans.position, 
                                            targetPos, 
                                            characterData.moveSpeed * Time.deltaTime);

        if (!lookDirect.Equals(Vector3.zero))
        {
            playerTrans.rotation = Quaternion.Lerp(playerTrans.rotation,
                                                   Quaternion.LookRotation(lookDirect),
                                                   characterData.rotateSpeed * Time.deltaTime);
        }
    }


    public bool IsGround
    {
        get
        {
            Debug.DrawRay(playerTrans.position, Vector3.down * 0.9f, Color.red, 3.0f);

            if (Physics.Raycast(playerTrans.position, Vector3.down, out hit, 0.9f))
                return hit.transform.CompareTag(GlobalData.TAG_GROUND);
            else return false;
        }
    }

    public void Jump()
    {
        playerRigid.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }
}
