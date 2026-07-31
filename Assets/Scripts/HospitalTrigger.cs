using UnityEngine;

public class HospitalTrigger : MonoBehaviour
{
    public static HospitalTrigger Instance;

    public GameObject hospitalMarker;

    private void Awake()
    {
        Instance = this;
    }

    public void EnableHospital()
    {
        hospitalMarker.SetActive(true);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            hospitalMarker.SetActive(false);

            MissionManager.Instance.CompleteMission();
        }
    }
}