using System.Collections.Generic;
using UnityEngine;
using Unity.Profiling;

public class GPSManager : MonoBehaviour
{
    public static GPSManager Instance;

    private static readonly ProfilerMarker UpdateGPSMarker = new ProfilerMarker("GPS.UpdateGPS");

    [SerializeField] private Transform firstDestination;
    [SerializeField] private Transform hospitalDestination;

    private Transform waypointMarker;

    public Transform playerCar;

    public RouteDrawer routeDrawer;

    public Vector3 Destination { get; private set; }
    public bool HasDestination { get; private set; }

    // For optimization, only recalculate path if player has moved a certain distance since last path calculation
    private Vector3 lastPathPosition;
    private float repathDistance = 10f;

    void Awake()
    {
        Instance = this;

        if (waypointMarker != null)
            waypointMarker.gameObject.SetActive(false);
    }

    void Start()
    {
        SetDestination(firstDestination.position);
    }

    void Update()
    {
        if (HasDestination)
        {
            // For optimization, only recalculate path if player has moved a certain distance since last path calculation
            if (Vector3.Distance(playerCar.position, lastPathPosition) >= repathDistance)
            {
                lastPathPosition = playerCar.position;
                UpdateGPS();
            }
        }
    }

    public void SetDestination(Vector3 worldPos)
    {
        Destination = worldPos;
        HasDestination = true;
        UpdateGPS();
    }

    public void SetDefaultDestination()
    {
        Destination = hospitalDestination.position;
        HasDestination = true;
        UpdateGPS();
    }

    private void UpdateGPS()
    {
        using (UpdateGPSMarker.Auto())
        {
            // Find closest graph nodes
            RoadGraphNode start = NavigationManager.Instance.GetClosestNode(playerCar.position);

            RoadGraphNode end = NavigationManager.Instance.GetClosestNode(Destination);

            // Calculate path
            List<RoadGraphNode> path = NavigationManager.Instance.FindPath(start, end);

            // Draw it
            routeDrawer.DrawRoute(path);
        }
    }

    public void ClearDestination()
    {
        HasDestination = false;

        if (waypointMarker != null)
            waypointMarker.gameObject.SetActive(false);
    }
}
