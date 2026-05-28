using UnityEngine;
using System.Collections.Generic;

public class RoadGraphNode
{
    public Vector3 position;
    public List<RoadGraphNode> neighbours = new List<RoadGraphNode>();
}