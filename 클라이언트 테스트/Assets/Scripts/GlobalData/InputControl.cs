// 플레이어의 Input값을 받아둠

using UnityEngine;

public class InputControl : MonoBehaviour
{
    // 위,아래 이동 수치 확보
    public static float MoveY;
    // 좌,우  이동 수치 확보
    public static float MoveX;
    // 좌, 우 회전
    public static float RotateX;
    // 이동 유무 확인
    public static bool isMoving = false;

    void FixedUpdate()
    {
        MoveY = Input.GetAxis("Vertical");
        MoveX = Input.GetAxis("Horizontal");
        //회전 변화율 -1~1 -> -5~5, 이에 맞게 SmoothFollow의 RotationDamper도 5로 수정
        RotateX = Mathf.Clamp(Input.GetAxis("Mouse X"), -5.0f, 5.0f);
        
        isMoving = (Mathf.Abs(MoveX) > 0 || Mathf.Abs(MoveY) > 0);
    }
}
