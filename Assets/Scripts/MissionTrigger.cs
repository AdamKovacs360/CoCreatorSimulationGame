using UnityEngine;

public class MissionTrigger : MonoBehaviour
{
    private bool reached;

    private void OnTriggerEnter(Collider other)
    {
        if (reached)
            return;

        if (other.CompareTag("Player"))
        {
            reached = true;

            MissionManager.Instance.objectiveText.text =
                "Patient Loaded. Drive to Hospital";

            HospitalTrigger.Instance.EnableHospital();
        }
    }
}