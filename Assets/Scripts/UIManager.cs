using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Receiver.Primitives;

public class UIManager : MonoBehaviour
{
    [SerializeField] private Canvas missionCanvas;
    [SerializeField] private TextMeshProUGUI remainingLifeText;
    [SerializeField] private TextMeshProUGUI missionStartText;
    [SerializeField] private TextMeshProUGUI missionEndText;
    [SerializeField] private TextMeshProUGUI missionSuccessText;
    [SerializeField] private TextMeshProUGUI missionFailedText;
    [SerializeField] private TextMeshProUGUI[] missionTexts;
    [SerializeField] private TextMeshProUGUI pressToContinueText;
    [SerializeField] private TextMeshProUGUI dialogueText;

    //public GameObject GameCompleteUI;
    //public GameObject GameOverUI;

    private int currentMissionIndex = 0;
    private float timer = 5f;
    private float typingSpeed = 0.05f;
    private bool isTimeStopped = false;
    private bool isGameOver = false;
    private bool isTimer = false;

    [TextArea]
    public string dialogue;

    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        isGameOver = false;
        isTimer = false;
        currentMissionIndex = 0;
        DisableAllMissionTexts();
        StartMission();
    }

    // Update is called once per frame
    void Update()
    {
        if (isTimer)
        {
            timer -= Time.deltaTime;
            if (timer <= 0)
            {
                isTimer = false;
                timer = 5f;
                DisableAllMissionTexts();
            }
        }

        if (isTimeStopped && !isGameOver)
        {
            if (AnyInputPressedThisFrame())
            {
                DisableAllMissionTexts();
            }
        }
    }

    private bool AnyInputPressedThisFrame()
    {
        foreach (var device in InputSystem.devices)
        {
            foreach (var control in device.allControls)
            {
                if (control is ButtonControl button && button.wasPressedThisFrame)
                {
                    return true;
                }
            }
        }
        return false;
    }

    public void DisableAllMissionTexts()
    {
        Time.timeScale = 1f;
        isTimeStopped = false;
        missionCanvas.gameObject.SetActive(false);
        missionStartText.gameObject.SetActive(false);
        missionEndText.gameObject.SetActive(false);
        missionFailedText.gameObject.SetActive(false);
        missionSuccessText.gameObject.SetActive(false);
        remainingLifeText.gameObject.SetActive(false);
        dialogueText.gameObject.SetActive(false);

        pressToContinueText.text = "Press any key to continue...";
        pressToContinueText.gameObject.SetActive(false);

        foreach (var text in missionTexts)
        {
            text.gameObject.SetActive(false);
        }
    }

    public void StartMission()
    {
        Time.timeScale = 0f; // Pause the game
        isTimeStopped = true;

        missionCanvas.gameObject.SetActive(true);
        //missionStartText.gameObject.SetActive(true);
        dialogueText.gameObject.SetActive(true);
        StartCoroutine(TypeDialogue());
    }

    public void EndMission()
    {
        Time.timeScale = 0f; // Pause the game
        isTimeStopped = true;

        missionCanvas.gameObject.SetActive(true);
        missionEndText.gameObject.SetActive(true);
    }

    public void MissionFailed()
    {
        Time.timeScale = 0f; // Pause the game
        //GameOverUI.SetActive(true);
        isTimeStopped = true;
        isGameOver = true;

        pressToContinueText.text = "Press R to restart...";
        missionCanvas.gameObject.SetActive(true);
        missionFailedText.gameObject.SetActive(true);
    }

    public void MissionSuccess()
    {
        Time.timeScale = 0f; // Pause the game
        isTimeStopped = true;
        isGameOver = true;

        pressToContinueText.text = "Press R to restart...";
        missionCanvas.gameObject.SetActive(true);
        missionSuccessText.gameObject.SetActive(true);
    }
    public void ShowMissionText()
    {
            Time.timeScale = 0f; // Pause the game
            isTimeStopped = true;
            missionCanvas.gameObject.SetActive(true);
            missionTexts[currentMissionIndex].gameObject.SetActive(true);
            currentMissionIndex++;
    }
    public void Crashed(int remaining)
    {
        missionCanvas.gameObject.SetActive(true);
        remainingLifeText.text = "You have Crashed! Remaining Life: " + remaining.ToString();
        remainingLifeText.gameObject.SetActive(true);
        pressToContinueText.text = "";
        isTimer = true;
    }
    IEnumerator TypeDialogue()
    {
        dialogueText.text = "";

        foreach (char letter in dialogue)
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }
    }

}


