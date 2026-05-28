using UnityEngine;
using System.Collections.Generic;

public class NavigationManager : MonoBehaviour
{
    public static NavigationManager Instance;

    public RoadGraphBuilder graphBuilder;

    void Awake()
    {
        Instance = this;
    }

    public RoadGraphNode GetClosestNode(Vector3 pos)
    {
        RoadGraphNode closest = null;
        float minDist = Mathf.Infinity;

        foreach (var node in graphBuilder.nodes)
        {
            float dist =
                Vector3.Distance(
                    pos,
                    node.position);

            if (dist < minDist)
            {
                minDist = dist;
                closest = node;
            }
        }

        return closest;
    }

    public List<RoadGraphNode> FindPath(
        RoadGraphNode start,
        RoadGraphNode goal)
    {
        List<RoadGraphNode> open =
            new List<RoadGraphNode>();

        HashSet<RoadGraphNode> closed =
            new HashSet<RoadGraphNode>();

        Dictionary<RoadGraphNode, RoadGraphNode>
            cameFrom =
            new Dictionary<RoadGraphNode, RoadGraphNode>();

        Dictionary<RoadGraphNode, float>
            gScore =
            new Dictionary<RoadGraphNode, float>();

        open.Add(start);
        gScore[start] = 0;

        while (open.Count > 0)
        {
            RoadGraphNode current = open[0];

            foreach (var node in open)
            {
                float nodeScore =
                    gScore[node] +
                    Vector3.Distance(
                        node.position,
                        goal.position);

                float currentScore =
                    gScore[current] +
                    Vector3.Distance(
                        current.position,
                        goal.position);

                if (nodeScore < currentScore)
                    current = node;
            }

            if (current == goal)
                return ReconstructPath(
                    cameFrom,
                    current);

            open.Remove(current);
            closed.Add(current);

            foreach (var neighbour in current.neighbours)
            {
                if (closed.Contains(neighbour))
                    continue;

                float tentative =
                    gScore[current] +
                    Vector3.Distance(
                        current.position,
                        neighbour.position);

                if (!open.Contains(neighbour))
                    open.Add(neighbour);

                if (gScore.ContainsKey(neighbour) &&
                    tentative >= gScore[neighbour])
                    continue;

                cameFrom[neighbour] =
                    current;

                gScore[neighbour] =
                    tentative;
            }
        }

        return null;
    }

    List<RoadGraphNode> ReconstructPath(
        Dictionary<RoadGraphNode,
        RoadGraphNode> cameFrom,
        RoadGraphNode current)
    {
        List<RoadGraphNode> path =
            new List<RoadGraphNode>();

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
