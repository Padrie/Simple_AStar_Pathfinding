using System;
using System.Collections.Generic;
using UnityEngine;

namespace SimplePathfinding
{
    [Serializable]
    public class AStarGridPoint : IAStarPoint
    {
        bool isWalkable = true;
        [HideInInspector] public List<Vector3> serializedNeighbors = new();
        [System.NonSerialized] List<IAStarPoint> neighbors = new();
        [SerializeField, HideInInspector] Vector3 pos;

        public Vector3 Position => pos;

        public bool Walkable { get => isWalkable; set => isWalkable = value; }

        public List<IAStarPoint> Neighbors => neighbors;

        public float Weight { get; set; }

        public AStarGridPoint()
        {
            neighbors = new List<IAStarPoint>();
        }

        public AStarGridPoint(Vector3 position)
        {
            pos = position;
        }
    }
}