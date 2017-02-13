// 플레이어의 움직임 정보 컴포넌트

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


    [Header("[ Transform Status ]")]
    // 점프할 힘
    public float jumpForce = 10.0f;


    // 속도값이 포함된 플레이어 정보 할당
    private CharacterData characterData;

    private RaycastHit hit;


    // 컴포넌트 초기화
    public void InitTransform(CharacterData _characterData)
    {
        characterData = _characterData;

        isInit = true;
    }

    // 플레이어 이동 정보 업데이트
    public void UpdateTransform()
    {
        if (!isInit)
            return;

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
    }

    // 땅인지 확인 -> 점프 가능 확인
    public bool IsGround
    {
        get
        {
            // 바닥을 향해 레이캐스트를 쏴서 땅인지 확인
            Debug.DrawRay(playerTrans.position, Vector3.down * 0.9f, Color.red, 3.0f);

            if (Physics.Raycast(playerTrans.position, Vector3.down, out hit, 0.9f))
                return hit.transform.CompareTag(GlobalData.TAG_GROUND);
            else return false;
        }
    }

    // 점프
    public void Jump()
    {
        playerRigid.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }
}
