using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System;
using System.Threading;

public class SendPool
{
    /*private bool activated = true;
    private Queue<NetworkData> sendpacketorder = new Queue<NetworkData>();
    public static SendPool instance = new SendPool();

    ThreadStart th;
    Thread t;

    public void activate()
    {
        th = new ThreadStart(pool);
        t = new Thread(th);
        t.Start();
    }

    public void addSendingData(NetworkData netData)
    {
        netData.seq = sendpacketorder.Count;
        sendpacketorder.Enqueue(netData);
    }

    public void pool()
    {
        while (activated)
        {
            if (sendpacketorder.Count > 0)
            {
                IOCPManager.GetInstance.nativeSendMessageToServer(sendpacketorder.Dequeue());
            }
        }
    }

    public void OnDisable()
    {
        activated = false;
    }*/
}
