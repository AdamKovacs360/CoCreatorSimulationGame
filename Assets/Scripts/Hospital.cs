using UnityEngine;

public class Hospital : MonoBehaviour
{
    private bool doHaveMoreMission = true;
    private bool doWeHavePatientInTheCar = false;
    private UIManager uiManager;
    private GPSManager gpsManager;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        uiManager = FindAnyObjectByType<UIManager>();
        gpsManager = FindAnyObjectByType<GPSManager>();
        doHaveMoreMission = true;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (!doHaveMoreMission)
            {
                Debug.Log("Player entered the hospital. No more missions available."); 
            }
            else
            {
                gpsManager.SetDestination();
                Debug.Log("Player entered the hospital. More missions are available.");

                if (doWeHavePatientInTheCar && doHaveMoreMission)
                {
                    uiManager.ShowMissionText();
                }
            }

            //Reset Patient to false
            doWeHavePatientInTheCar = false;
        }
    }

    public void SetMissionBoolToFalse()
    {
        doHaveMoreMission = false; // Reset the mission flag when the player enters the hospital
        uiManager.MissionSuccess();
    }

    public void SetPatientBoolToTrue()
    {
        doWeHavePatientInTheCar = true;
    }
}
