using UnityEngine;
using TMPro;

public class MissionManager : MonoBehaviour
{
    public static MissionManager Instance;

    public TMP_Text missionTitle;
    public TMP_Text missionDescription;
    public TMP_Text objectiveText;
    public TMP_Text timerText;

    public int currentMission = 0;

    private float missionTimer;
    private bool timerRunning;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        StartMission(0);
    }

    private void Update()
    {
        if (timerRunning)
        {
            missionTimer -= Time.deltaTime;

            timerText.text =
                "Time: " + Mathf.Ceil(missionTimer);

            if (missionTimer <= 0)
            {
                timerRunning = false;
                Debug.Log("Mission Failed");
            }
        }
    }

    public void StartMission(int missionIndex)
    {
        currentMission = missionIndex;

        switch (missionIndex)
        {
            case 0:
                missionTitle.text = "Mission 1";
                missionDescription.text = "Minor Injury Reported";
                objectiveText.text = "Drive to patient";
                timerText.text = "";
                break;

            case 1:
                missionTitle.text = "Mission 2";
                missionDescription.text = "Major Car Crash";
                objectiveText.text = "Drive to crash site";
                timerText.text = "";
                break;

            case 2:
                missionTitle.text = "Mission 3";
                missionDescription.text = "Cardiac Emergency";
                objectiveText.text = "Reach patient before time runs out";
                missionTimer = 90f;
                timerRunning = true;
                break;
        }
    }

    public void CompleteMission()
    {
        currentMission++;

        if (currentMission < 3)
        {
            StartMission(currentMission);
        }
        else
        {
            objectiveText.text = "All Missions Complete!";
        }
    }
}