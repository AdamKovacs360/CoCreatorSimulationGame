using UnityEngine;

public class Hospital : MonoBehaviour
{
    private bool doHaveMoreMission = true;
    private UIManager uiManager;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        uiManager = FindAnyObjectByType<UIManager>();
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
                uiManager.MissionSuccess();
            }
            else
            {
                Debug.Log("Player entered the hospital. More missions are available.");
            }
        }
    }

    public void SetMissionBoolToFalse()
    {
        doHaveMoreMission = false; // Reset the mission flag when the player enters the hospital
    }
}
