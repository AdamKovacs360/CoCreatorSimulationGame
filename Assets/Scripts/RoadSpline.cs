using UnityEngine;
using UnityEngine.Splines;

public class RoadSpline : MonoBehaviour
{
    [HideInInspector] public SplineContainer spline;

    [HideInInspector] public RoadGraphNode startNode;
    [HideInInspector] public RoadGraphNode endNode;

    void Awake()
    {
        spline = GetComponent<SplineContainer>();
    }
}
