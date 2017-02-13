// 플레이어의 Input값을 받아둠

using UnityEngine;

public class InputControl : MonoBehaviour
{
    // 위,아래 이동 수치 확보
    public static float MoveY;
    // 좌,우  이동 수치 확보
    public static float MoveX;
    // 이동 유무 확인
    public static bool isMoving = false;

    void FixedUpdate()
    {
        MoveY = Input.GetAxis("Vertical");
        MoveX = Input.GetAxis("Horizontal");
        
        isMoving = (Mathf.Abs(MoveX) > 0 || Mathf.Abs(MoveY) > 0);
    }
}
