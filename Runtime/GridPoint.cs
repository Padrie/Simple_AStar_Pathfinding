using System;
using System.Collections.Generic;
using UnityEngine;

namespace SimplePathfinding
{
    [Serializable]
    public class GridPoint : IAStarPoint
    {
        bool isWalkable = true;
        float weight = 1f;
        [HideInInspector] public List<Vector3> serializedNeighbors = new();
        [System.NonSerialized] List<IAStarPoint> neighbors = new();
        [SerializeField, HideInInspector] Vector3 pos;

        public Vector3 Position => pos;

        public bool Walkable { get => isWalkable; set => isWalkable = value; }

        public List<IAStarPoint> Neighbors => neighbors;

        public float Weight { get => weight; set => weight = Mathf.Clamp(value, 0.5f, 2f); }

        public GridPoint()
        {
            neighbors = new List<IAStarPoint>();
        }

        public GridPoint(Vector3 position)
        {
            pos = position;
            neighbors = new List<IAStarPoint>();
        }
    }
}