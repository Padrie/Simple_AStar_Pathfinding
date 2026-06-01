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
        [HideInInspector] public bool[] allowedAgentTypes = new bool[30];

        public Vector3 Position => pos;

        public bool Walkable { get => isWalkable; set => isWalkable = value; }

        public List<IAStarPoint> Neighbors => neighbors;

        public float Weight { get => weight; set => weight = Mathf.Max(0.1f, value); }

        public bool[] AllowedAgentTypes { get => allowedAgentTypes; set => allowedAgentTypes = value; }

        public GridPoint()
        {
            neighbors = new List<IAStarPoint>();
            SetAgentTypesLimit();
        }

        public GridPoint(Vector3 position)
        {
            pos = position;
            neighbors = new List<IAStarPoint>();
            SetAgentTypesLimit();
        }

        private void SetAgentTypesLimit()
        {
            if (allowedAgentTypes == null || allowedAgentTypes.Length != 30)
            {
                allowedAgentTypes = new bool[30];
                allowedAgentTypes[0] = true;
            }
            else
            {
                bool anyTrue = false;
                for (int i = 0; i < 30; i++)
                    if (allowedAgentTypes[i]) { anyTrue = true; break; }

                if (!anyTrue) allowedAgentTypes[0] = true;
            }
        }
    }
}