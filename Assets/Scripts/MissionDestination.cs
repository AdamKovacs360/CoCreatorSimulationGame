using UnityEngine;

public class MissionDestination : MonoBehaviour
{
    public Transform destinationPoint;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            ActivateMission();
            Debug.Log("Mission activated! Destination set to: " + (destinationPoint != null ? destinationPoint.position.ToString() : "Default (Hospital)"));
        }
    }

    public void ActivateMission()
    {
        if (destinationPoint == null)
        {
            // if no new destination GPS manager will set it to default position (Hospital)
            GPSManager.Instance.SetDefaultDestination();
        }
        else
        {
            // Set the GPS destination to the mission's destination point
            GPSManager.Instance.SetDestination(destinationPoint.position);
        }

        // Destroy the mission destination object after activating the mission prevent multiple activations
        Destroy(gameObject);
    }
}
