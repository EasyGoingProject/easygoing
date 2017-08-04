using UnityEngine;
using System;

[Serializable]
public class Area
{
    public int areaNumber;
    public bool isAlarmed = false;
    public bool isActivate = true;
    public GameObject areaObj;
    public Transform[] winnerPoints;

    public void Reset()
    {
        isAlarmed = false;
        isActivate = true;
        areaObj.SetActive(false);
    }

    public void Alarm()
    {
        isAlarmed = true;
    }

    public void Deactivate()
    {
        if (!isActivate)
            return;
        isActivate = false;
        areaObj.SetActive(true);
    }
}
