using UnityEngine;

public class MissionDestination : MonoBehaviour
{
    public Transform destinationPoint;

    public void ActivateMission()
    {
        GPSManager.Instance.SetDestination(destinationPoint.position);
    }
}
