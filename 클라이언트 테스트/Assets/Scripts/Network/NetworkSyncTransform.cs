using UnityEngine;
using System.Collections;

public class NetworkSyncTransform : MonoBehaviour {

    [SerializeField]
    private float positionLerpRate = 15;
    [SerializeField]
    private float rotationLerpRate = 15;
    [SerializeField]
    private float positionThreshold = 0.1f;
    [SerializeField]
    private float rotationThreshold = 1f;

    private Vector3 lastPosition;
    private Vector3 lastRotation;

    private Transform thisTrans;

    public bool isLocalPlayer;

    void Awake()
    {
        thisTrans = transform;
    }

    void Update()
    {
        if (isLocalPlayer)
            return;

        InterpolatePosition();
        InterpolateRotation();
    }

    private void InterpolatePosition()
    {
        thisTrans.position = Vector3.Lerp(thisTrans.position, lastPosition, Time.deltaTime * positionLerpRate);
    }

    private void InterpolateRotation()
    {
        thisTrans.rotation = Quaternion.Lerp(thisTrans.rotation, Quaternion.Euler(lastRotation), Time.deltaTime * rotationLerpRate);
    }

    void FixedUpdate()
    {
        if (!isLocalPlayer)
            return;

        if (IsPositionChanged() || IsRotationChanged())
        {
            lastPosition = thisTrans.position;
            lastRotation = thisTrans.eulerAngles;
            IOCPManager.GetInstance.SendTransform(thisTrans.position, thisTrans.rotation);
        }
    }

    private bool IsPositionChanged()
    {
        return Vector3.Distance(thisTrans.position, lastPosition) > positionThreshold;
    }

    private bool IsRotationChanged()
    {
        return Vector3.Distance(thisTrans.eulerAngles, lastRotation) > rotationThreshold;
    }
}
