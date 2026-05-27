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
        [HideInInspector] public bool[] allowedAgentTypes = new bool[30];

        public Vector3 Position => transform.position;

        public bool Walkable { get => isWalkable; set => isWalkable = value; }

        public List<IAStarPoint> Neighbors => neighbors;

        public float Weight { get => weight; set => weight = Mathf.Clamp(value, 0.5f, 2f); }

        public bool[] AllowedAgentTypes => allowedAgentTypes;

        private void OnValidate()
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
