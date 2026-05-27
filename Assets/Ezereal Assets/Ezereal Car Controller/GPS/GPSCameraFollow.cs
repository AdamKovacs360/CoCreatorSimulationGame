using UnityEngine;

public class GPSCameraFollow : MonoBehaviour
{
    public Transform target;

    [Header("Offset relative to car")]
    public Vector3 localOffset = new Vector3(0f, 60f, -40f);

    [Header("Rotation")]
    public float tilt = 55f;

    void LateUpdate()
    {
        if (!target) return;

        // Rotate offset with car
        Vector3 rotatedOffset = Quaternion.Euler(0f, target.eulerAngles.y, 0f) * localOffset;

        // Position camera relative to car
        transform.position = target.position + rotatedOffset;

        // Look with car heading
        Quaternion targetRotation = Quaternion.Euler(tilt, target.eulerAngles.y, 0f);

        transform.rotation = targetRotation;
    }
}
