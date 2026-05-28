using UnityEngine;
using UnityEngine.Splines;
using System.Collections.Generic;

public class RoadGraphBuilder : MonoBehaviour
{
    public float nodeSpacing = 10f;

    public List<RoadGraphNode> nodes = new List<RoadGraphNode>();

    void Awake()
    {
        BuildGraph();
    }

    void BuildGraph()
    {
        nodes.Clear();

        RoadSpline[] roads = FindObjectsOfType<RoadSpline>();

        foreach (var road in roads)
        {
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

                if (previous != null)
                {
                    previous.neighbours.Add(node);
                    node.neighbours.Add(previous);
                }

                previous = node;
            }
        }

        Debug.Log("Generated nodes: " + nodes.Count);

        Debug.Log("Roads found: " + roads.Length);
    }
}