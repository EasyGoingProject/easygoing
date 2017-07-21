// 플레이어의 움직임 정보 컴포넌트

#define ThirdPerson

using UnityEngine;
using System.Collections;

public class PlayerTransform : MonoBehaviour
{
    // 초기화 유무 확인
    // 초기화는 PlayerControl에서 하게 되므로 
    // Update시 초기화 됐는지 확인할 불린값이 필요
    private bool isInit = false;


    [Header("[ Bound Component ]")]
    // 이동, 회전할 플레이어 오브젝트의 트랜스폼
    public Transform playerTrans;
    // 플레이어의 강체가 포함된 오브젝트 할당
    public Rigidbody playerRigid;

    
    // 속도값이 포함된 플레이어 정보 할당
    private CharacterData characterData;

    private float rotateAngle;
    private Vector3 movement;
    private Quaternion rotation;


    #region [ Init ]

    // 컴포넌트 초기화
    public void InitTransform(CharacterData _characterData)
    {
        characterData = _characterData;

        InitJumpRay();

        isInit = true;
    }

    #endregion


    #region [ Update ]

    // 플레이어 이동 정보 업데이트
    public void UpdateTransform()
    {
        if (!isInit)
            return;

#if !ThirdPerson
        // Input Control에서 받아온 위,아래,좌,우 수치를 통해 플레이어가 이동할 위치 지정
        Vector3 targetPos = playerTrans.position;
        targetPos.x += InputControl.MoveX;
        targetPos.z += InputControl.MoveY;
        
        // 이동할 위치와 현재 위치와의 위치값의 차이로 바라볼 방향 확인
        Vector3 lookDirect = targetPos - playerTrans.position;

        // 플레이어 이동 - Lerp를 통해 해당 위치로 부드럽게 이동
        playerTrans.position = Vector3.Lerp(playerTrans.position,
                                            targetPos,
                                            characterData.moveSpeed * Time.deltaTime);

        // 이동방향이 0,0,0이 될시 오류가 발생하기 때문에 막음
        if (!lookDirect.Equals(Vector3.zero))
        {
            // 플레이어 회전 - Lerp를 통해 해당 위치로 부드럽게 회전
            playerTrans.rotation = Quaternion.Lerp(playerTrans.rotation,
                                                   Quaternion.LookRotation(lookDirect),
                                                   characterData.rotateSpeed * Time.deltaTime);
        }
#else
        movement = (playerTrans.forward * InputControl.MoveY + playerTrans.right * InputControl.MoveX).normalized;

        playerTrans.position += (movement * characterData.moveSpeed * Time.deltaTime);

        rotateAngle = ClampAngle(rotateAngle + (InputControl.RotateX * characterData.rotateSpeed));
        rotation = Quaternion.Euler(0, rotateAngle, 0);

        playerTrans.rotation = rotation;
#endif
    }

    private float ClampAngle(float targetAngle)
    {
        if (targetAngle < -360.0f)
            targetAngle += 360.0f;
        else if (targetAngle > 360.0f)
            targetAngle -= 360.0f;
        return targetAngle;
    }

#endregion


#region [ Jump ]

    [Header("[ Jump ]")]
    // 점프할 힘
    public float jumpForce = 10.0f;
    
    // 땅체크 레이캐스트용
    private RaycastHit jumpRayhit;
    private float jumpGroundRayOffsetY = 0.1f;
    private LayerMask jumpGroundRayLayer;
    private Vector3 jumpGroundPos;

    private void InitJumpRay()
    {
        jumpGroundRayLayer = 1 << LayerMask.NameToLayer(GlobalData.LAYER_FIELD);
    }

    // 땅인지 확인
    public bool IsGround
    {
        get
        {
            jumpGroundPos = playerTrans.position;
            jumpGroundPos.y += jumpGroundRayOffsetY;

            // 바닥을 향해 레이캐스트를 쏴서 땅인지 확인
            Debug.DrawRay(jumpGroundPos, Vector3.down * 0.9f, Color.red, 3.0f);

            if (Physics.Raycast(jumpGroundPos, Vector3.down, out jumpRayhit, 0.9f, jumpGroundRayLayer))
                return jumpRayhit.transform.CompareTag(GlobalData.TAG_GROUND);
            else return false;
        }
    }

    // 점프
    public void Jump()
    {
        playerRigid.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

#endregion
}
