using System.Collections.Generic;
using UnityEngine;

namespace SimplePathfinding
{
    public interface IAStarPoint
    {
        Vector3 Position { get; }
        List<IAStarPoint> Neighbors { get; }
        bool Walkable { get; set; }
        float Weight { get ; set; }
        bool[] AllowedAgentTypes { get; }
    }
}