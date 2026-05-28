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

        if (waypointMarker != null)
        {
            waypointMarker.position = worldPos + Vector3.up * 2f;

            waypointMarker.gameObject.SetActive(true);
        }

        RoadNode start = NavigationManager.Instance.GetClosestNode(playerCar.position);

        RoadNode end = NavigationManager.Instance.GetClosestNode(worldPos);

        var path = NavigationManager.Instance.FindPath(start, end);

        routeDrawer.DrawRoute(path);

        Debug.Log($"Destenation Set {Destination}");
    }

    public void ClearDestination()
    {
        HasDestination = false;

        if (waypointMarker != null)
            waypointMarker.gameObject.SetActive(false);
    }
}
