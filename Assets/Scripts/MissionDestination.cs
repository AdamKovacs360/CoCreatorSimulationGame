using UnityEngine;

public class MissionDestination : MonoBehaviour
{
    private GPSManager gpsManager;
    private UIManager uiManager;

    private void Start()
    {
        uiManager = FindAnyObjectByType<UIManager>();
        gpsManager = FindAnyObjectByType<GPSManager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            ActivateMission();
            gpsManager.IncreaseMissionNum();
            Debug.Log("Patienent picked up! Destination set to Hospital");
        }
    }

    public void ActivateMission()
    {
        GPSManager.Instance.SetDefaultDestination();
        uiManager.EndMission();

        // Destroy the mission destination object after activating the mission prevent multiple activations
        gameObject.SetActive(false);
    }
}
