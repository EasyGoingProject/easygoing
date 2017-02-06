using UnityEngine;
using System.Collections;

public class InputControl : MonoBehaviour
{
    public static float MoveY;
    public static float MoveX;
    public static bool isMoving = false;

    void FixedUpdate()
    {
        MoveY = Input.GetAxis("Vertical");
        MoveX = Input.GetAxis("Horizontal");
        
        isMoving = (Mathf.Abs(MoveX) > 0 || Mathf.Abs(MoveY) > 0);
    }
}
