using UnityEngine;

public class CrashDetection : MonoBehaviour
{
    [SerializeField] private int MaxCollisions = 3; // Maximum number of collisions allowed
    private int collisionCount = 0;
    private float timeRemaining = 5f;
    private bool isTimerRunning = false;
    private UIManager uiManager;

    void Start()
    {
        uiManager = FindAnyObjectByType<UIManager>();
        collisionCount = 0;
    }

    private void Update()
    {
        if (isTimerRunning)
        {
            timeRemaining -= Time.deltaTime;
            if (timeRemaining <= 0f)
            {
                isTimerRunning = false;
            }
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Obstacle"))
        {
            if (isTimerRunning)
            {
                return; // Ignore further collisions while the timer is running
            }

            collisionCount++;

            if (collisionCount >= MaxCollisions)
            {
                Debug.Log("Player has crashed too many times!");
                uiManager.MissionFailed();
                return;
            }

            uiManager.Crashed(MaxCollisions - collisionCount);
            timeRemaining = 5f; // Reset the timer to 5 seconds
            isTimerRunning = true;
            Debug.Log("Player has crashed!");
        }
    }
}
