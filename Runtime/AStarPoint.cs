using System.Collections.Generic;
using UnityEngine;


namespace SimplePathfinding
{
    public class AStarPoint : MonoBehaviour, IAStarPoint
    {
        bool isWalkable = true;
        [HideInInspector] public List<Vector3> serializedNeighbors = new();
        [System.NonSerialized] List<IAStarPoint> neighbors = new();

        public Vector3 Position => transform.position;

        public bool Walkable { get => isWalkable; set => isWalkable = value; }

        public List<IAStarPoint> Neighbors => neighbors;

        public float Weight { get; set; }

    }
}
