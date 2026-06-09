using UnityEngine;
using UnityEngine.Splines;
using System.Collections.Generic;

public class RoadGraphBuilder : MonoBehaviour
{
    public float nodeSpacing = 10f;
    public float junctionDistance = 8f;

    public List<RoadGraphNode> nodes = new List<RoadGraphNode>();

    void Start()
    {
        BuildGraph();
    }

    void BuildGraph()
    {
        nodes.Clear();

        RoadSpline[] roads = FindObjectsOfType<RoadSpline>();

        foreach (var road in roads)
        {
            if (road == null)
            {
                Debug.LogError("Found null RoadSpline");
                continue;
            }

            if (road.spline == null)
            {
                Debug.LogError($"RoadSpline '{road.name}' has no spline assigned");
                continue;
            }

            RoadGraphNode firstNode = null;
            RoadGraphNode lastNode = null;

            Spline spline = road.spline.Spline;

            RoadGraphNode previous = null;

            float length = spline.GetLength();

            int count = Mathf.CeilToInt(length / nodeSpacing);

            for (int i = 0; i <= count; i++)
            {
                float t = i / (float)count;

                Vector3 pos = road.spline.transform.TransformPoint(spline.EvaluatePosition(t));

                RoadGraphNode node = new RoadGraphNode();

                node.position = pos;
                nodes.Add(node);

                if (i == 0)
                {
                    firstNode = node;
                }

                if (previous != null)
                {
                    previous.neighbours.Add(node);
                }

                previous = node;
                lastNode = node;
            }

            road.startNode = firstNode;
            road.endNode = lastNode;
        }

        ConnectRoadEndpoints(roads);
    }
    void ConnectRoadEndpoints(RoadSpline[] roads)
    {
        for (int i = 0; i < roads.Length; i++)
        {
            for (int j = i + 1; j < roads.Length; j++)
            {
                TryConnect(roads[i].startNode, roads[j].startNode);
                TryConnect(roads[i].startNode, roads[j].endNode);
                TryConnect(roads[i].endNode, roads[j].startNode);
                TryConnect(roads[i].endNode, roads[j].endNode);
            }
        }
    }

    void TryConnect(RoadGraphNode a, RoadGraphNode b)
    {
        if (a == null || b == null) return;

        float dist = Vector3.Distance(a.position, b.position);

        if (dist > junctionDistance) return;

        a.neighbours.Add(b);
        b.neighbours.Add(a);
    }
}