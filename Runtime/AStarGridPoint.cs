using System.Collections.Generic;
using UnityEngine;

namespace SimplePathfinding
{
    public class AStarGridPoint : IAStarPoint
    {
        bool isWalkable = true;
        List<IAStarPoint> neighbors = new();
        Vector3 pos;

        public AStarGridPoint(Vector3 position)
        {
            pos = position;
        }

        public Vector3 Position => pos;

        public bool Walkable { get => isWalkable; set => isWalkable = value; }

        public List<IAStarPoint> Neighbors => neighbors;
    }
}