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
        RotateX = Mathf.Clamp(Input.GetAxis("Mouse X"), -1.0f, 1.0f);
        
        isMoving = (Mathf.Abs(MoveX) > 0 || Mathf.Abs(MoveY) > 0);
    }
}
