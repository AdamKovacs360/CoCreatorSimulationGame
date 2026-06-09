using UnityEngine;
using System.Collections.Generic;

public class RouteDrawer : MonoBehaviour
{
    private LineRenderer line;

    void Awake()
    {
        line = GetComponent<LineRenderer>();
    }

    public void DrawRoute(List<RoadGraphNode> path)
    {
        if (path == null || path.Count == 0)
        {
            line.positionCount = 0;
            return;
        }

        line.positionCount = path.Count;

        for (int i = 0; i < path.Count; i++)
        {
            line.SetPosition(i, path[i].position + Vector3.up * 0.5f);
        }
    }
}
