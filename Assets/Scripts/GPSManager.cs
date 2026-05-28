using System.Collections.Generic;
using UnityEngine;

public class GPSManager : MonoBehaviour
{
    public static GPSManager Instance;

    public Transform waypointMarker;

    public Transform playerCar;

    public RouteDrawer routeDrawer;

    public Vector3 Destination { get; private set; }
    public bool HasDestination { get; private set; }

    void Awake()
    {
        Instance = this;

        if (waypointMarker != null)
            waypointMarker.gameObject.SetActive(false);
    }

    void Start()
    {
        SetDestination(waypointMarker.position);
    }

    public void SetDestination(Vector3 worldPos)
    {
        Destination = worldPos;
        HasDestination = true;

        // Find closest graph nodes
        RoadGraphNode start =
            NavigationManager.Instance
            .GetClosestNode(playerCar.position);

        RoadGraphNode end =
            NavigationManager.Instance
            .GetClosestNode(worldPos);

        // Calculate path
        List<RoadGraphNode> path =
            NavigationManager.Instance
            .FindPath(start, end);

        // Draw it
        routeDrawer.DrawRoute(path);

        Debug.Log(
            path == null
            ? "No path"
            : "Path nodes: " + path.Count);
    }

    public void ClearDestination()
    {
        HasDestination = false;

        if (waypointMarker != null)
            waypointMarker.gameObject.SetActive(false);
    }
}
