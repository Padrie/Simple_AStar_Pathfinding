using System.Collections.Generic;
using UnityEngine;

namespace SimplePathfinding
{
    public interface IAStarPoint
    {
        Vector3 Position { get; }
        bool Walkable { get; set; }
        List<IAStarPoint> Neighbors { get; }
    }
}