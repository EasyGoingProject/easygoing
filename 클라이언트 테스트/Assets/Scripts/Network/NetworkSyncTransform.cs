using UnityEngine;
using System.Collections;

public enum ObjectType
{
    Player,
    Object,
    Enemy
}

public class NetworkSyncTransform : MonoBehaviour {

    public ObjectType objectType = ObjectType.Player;
    public int objectNetworkId = 0;

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

    public void SetObject(int _objectNetworkId, bool isLocal)
    {
        objectNetworkId = _objectNetworkId;
        isLocalPlayer = isLocal;
    }

    public void SetPhysicsState()
    {
    }

    public void SetTransform(NetworkVector pos, NetworkVector rot)
    {
        lastPosition = new Vector3(pos.x, pos.y, pos.z);
        lastRotation = new Vector3(rot.x, rot.y, rot.z);
    }

    public void SetTransform(Vector3 pos, Vector3 rot)
    {
        lastPosition = pos;
        lastRotation = rot;

        thisTrans.position = lastPosition;
        thisTrans.eulerAngles = lastPosition;
    }

    void Update()
    {
        if (isLocalPlayer)//로컬 플레이어면 필요없음/호스트면 필요없음
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
        if (!isLocalPlayer)//로컬 플레이어면 처리/호스트면 처리
            return;

        if (IsPositionChanged() || IsRotationChanged())
        {
            lastPosition = thisTrans.position;
            lastRotation = thisTrans.eulerAngles;

            if (objectType == ObjectType.Player)
            {
                IOCPManager.GetInstance.SendToServerMessage(
                    new NetworkData()
                    {
                        senderId = IOCPManager.senderId,
                        sendType = SendType.SYNCTRANSFORM,
                        position = new NetworkVector()
                        {
                            x = thisTrans.position.x,
                            y = thisTrans.position.y,
                            z = thisTrans.position.z,
                        },
                        rotation = new NetworkVector()
                        {
                            x = thisTrans.eulerAngles.x,
                            y = thisTrans.eulerAngles.y,
                            z = thisTrans.eulerAngles.z,

                        }
                    });
            }
            else if (objectType == ObjectType.Enemy)
            {
                IOCPManager.GetInstance.SendToServerMessage(
                   new NetworkData()
                   {
                       senderId = IOCPManager.senderId,
                       targetId = objectNetworkId,
                       sendType = SendType.ENEMY_SYNC_TRANSFORM,
                       position = new NetworkVector()
                       {
                           x = thisTrans.position.x,
                           y = thisTrans.position.y,
                           z = thisTrans.position.z,
                       },
                       rotation = new NetworkVector()
                       {
                           x = thisTrans.eulerAngles.x,
                           y = thisTrans.eulerAngles.y,
                           z = thisTrans.eulerAngles.z,
                       }
                   });
            }
            else if(objectType == ObjectType.Object)
            {
                IOCPManager.GetInstance.SendToServerMessage(
                    new NetworkData()
                    {
                        senderId = IOCPManager.senderId,
                        targetId = objectNetworkId,
                        sendType = SendType.OBJECT_SYNC_TRANSFORM,
                        position = new NetworkVector()
                        {
                            x = thisTrans.position.x,
                            y = thisTrans.position.y,
                            z = thisTrans.position.z,
                        },
                        rotation = new NetworkVector()
                        {
                            x = thisTrans.eulerAngles.x,
                            y = thisTrans.eulerAngles.y,
                            z = thisTrans.eulerAngles.z,
                        }
                    });
            }
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
