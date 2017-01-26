using UnityEngine;
using System.Collections;

public class InputControl : MonoBehaviour
{
    public static float moveY;
    public static float moveX;
    public static bool isMoving = false;

    void Update()
    {
        moveY = Input.GetAxis("Vertical");
        moveX = Input.GetAxis("Horizontal");

        isMoving = (Mathf.Abs(moveX) > 0 || Mathf.Abs(moveY) > 0);
    }
}
