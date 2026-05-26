using System.Collections.Generic;
using UnityEngine;


namespace SimplePathfinding
{
    public class WayPoint : MonoBehaviour, IAStarPoint
    {
        public bool isWalkable = true;
        [Range(0.5f, 2f)]public float weight = 1f;
        [HideInInspector] public List<Vector3> serializedNeighbors = new();
        [System.NonSerialized] List<IAStarPoint> neighbors = new();

        public Vector3 Position => transform.position;

        public bool Walkable { get => isWalkable; set => isWalkable = value; }

        public List<IAStarPoint> Neighbors => neighbors;

        public float Weight { get => weight; set => weight = Mathf.Clamp(value, 0.5f, 2f); }

    }
}
