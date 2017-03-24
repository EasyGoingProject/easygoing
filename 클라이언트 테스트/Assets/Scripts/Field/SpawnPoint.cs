using UnityEngine;
using System.Collections;

public class SpawnPoint : MonoBehaviour
{
    public int playerIndex;

    public Vector3 GetPosition()
    {
        return transform.position;
    }
}
