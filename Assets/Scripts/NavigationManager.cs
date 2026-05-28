using System.Collections.Generic;
using UnityEngine;

public class NavigationManager : MonoBehaviour
{
    public static NavigationManager Instance;

    public List<RoadNode> allNodes = new List<RoadNode>();

    void Awake()
    {
        Instance = this;

        allNodes.AddRange(FindObjectsOfType<RoadNode>());
    }

    public RoadNode GetClosestNode(Vector3 pos)
    {
        RoadNode closest = null;
        float minDist = Mathf.Infinity;

        foreach (RoadNode node in allNodes)
        {
            float dist = Vector3.Distance(pos, node.transform.position);

            if (dist < minDist)
            {
                minDist = dist;
                closest = node;
            }
        }

        return closest;
    }

    public List<RoadNode> FindPath(RoadNode start, RoadNode goal)
    {
        List<RoadNode> open = new List<RoadNode>();

        List<RoadNode> closed = new List<RoadNode>();

        Dictionary<RoadNode, RoadNode> cameFrom = new Dictionary<RoadNode, RoadNode>();

        Dictionary<RoadNode, float> gScore = new Dictionary<RoadNode, float>();

        open.Add(start);
        gScore[start] = 0;

        while (open.Count > 0)
        {
            RoadNode current = open[0];

            foreach (var node in open)
            {
                float nodeScore = gScore[node] + Vector3.Distance(node.transform.position, goal.transform.position);

                float currentScore = gScore[current] + Vector3.Distance(current.transform.position, goal.transform.position);

                if (nodeScore < currentScore)
                    current = node;
            }

            if (current == goal)
            {
                return ReconstructPath(cameFrom, current);
            }

            open.Remove(current);
            closed.Add(current);

            foreach (var neighbour in current.connectedNodes)
            {
                if (closed.Contains(neighbour))
                    continue;

                float tentative = gScore[current] + Vector3.Distance(current.transform.position, neighbour.transform.position);

                if (!open.Contains(neighbour))
                    open.Add(neighbour);

                if (gScore.ContainsKey(neighbour) && tentative >= gScore[neighbour])
                    continue;

                cameFrom[neighbour] = current;
                gScore[neighbour] = tentative;
            }
        }

        return null;
    }

    List<RoadNode> ReconstructPath(Dictionary<RoadNode, RoadNode> cameFrom, RoadNode current)
    {
        List<RoadNode> path = new List<RoadNode>();

        path.Add(current);

        while (cameFrom.ContainsKey(current))
        {
            current = cameFrom[current];
            path.Add(current);
        }

        path.Reverse();
        return path;
    }
}
