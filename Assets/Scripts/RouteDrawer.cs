using System.Collections.Generic;
using UnityEngine;

public class RouteDrawer : MonoBehaviour
{
    public LineRenderer line;

    public void DrawRoute(List<RoadNode> path)
    {
        if (path == null)
        {
            line.positionCount = 0;
            return;
        }

        line.positionCount = path.Count;

        for (int i = 0; i < path.Count; i++)
        {
            line.SetPosition(i, path[i].transform.position + Vector3.up * 1f);
        }
    }
}
