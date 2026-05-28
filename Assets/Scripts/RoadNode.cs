using System.Collections.Generic;
using UnityEngine;

public class RoadNode : MonoBehaviour
{
    public List<RoadNode> connectedNodes = new List<RoadNode>();

    public float connectDistance = 30f;

    void Awake()
    {
        RoadNode[] all = FindObjectsOfType<RoadNode>();

        foreach (var node in all)
        {
            if (node == this) continue;

            if (Vector3.Distance(transform.position, node.transform.position) < connectDistance)
            {
                connectedNodes.Add(node);
            }
        }
    }
}
