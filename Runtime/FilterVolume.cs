using System.Collections.Generic;
using UnityEngine;

namespace SimplePathfinding
{
    public class FilterVolume : MonoBehaviour
    {
        [SerializeField] Vector3 size = new Vector3(10, 10, 10);
        [SerializeField] FilterVolumeMode mode = FilterVolumeMode.Once;
        [SerializeField] float updateInterval = 0.1f;

        [SerializeField] bool overrideWalkable = false;
        [SerializeField] bool walkableValue = true;

        [SerializeField] bool overrideWeight = false;
        [SerializeField, Range(0.1f, 3f)] float weightValue = 1f;

        [SerializeField] bool overrideTypes = false;
        [SerializeField] public bool[] allowedAgentTypes = new bool[30];

        Dictionary<IAStarPoint, PointSnapshot> affectedPoints = new();

        AStar aStar;

        Vector3 minWorldBounds;
        Vector3 maxWorldBounds;

        Bounds volumeBounds;

        private void Awake()
        {
            aStar = FindAnyObjectByType<AStar>();
        }

        public void ApplyToPoints()
        {
            volumeBounds = new Bounds(transform.position, size);

            minWorldBounds = transform.position - size / 2;
            maxWorldBounds = transform.position + size / 2;

            GetWaypoints();
            RemoveExitedPoints();
        }

        public void GetWaypoints()
        {
            Vector3Int minChunkVec = aStar.ConvertToChunkCoord(minWorldBounds);
            Vector3Int maxChunkVec = aStar.ConvertToChunkCoord(maxWorldBounds);

            for (int x = minChunkVec.x; x <= maxChunkVec.x; x++)
            {
                for (int y = minChunkVec.y; y <= maxChunkVec.y; y++)
                {
                    for (int z = minChunkVec.z; z <= maxChunkVec.z; z++)
                    {
                        Vector3Int coord = new Vector3Int(x, y, z);

                        if (aStar.waypointChunks.TryGetValue(coord, out var chunk))
                        {
                            for (int i = 0; i < chunk.pointList.Count; i++)
                            {
                                if (IsWaypointInBounds(chunk.pointList[i].Position))
                                {
                                    ApplyPoint(chunk.pointList[i]);
                                }
                            }
                        }
                    }
                }
            }
        }

        public void RemoveExitedPoints()
        {
            List<IAStarPoint> toRemove = new List<IAStarPoint>();
            foreach (var kvp in affectedPoints)
            {
                if (!volumeBounds.Contains(kvp.Key.Position))
                {
                    kvp.Value.Restore();
                    toRemove.Add(kvp.Key);
                }
            }

            foreach (var p in toRemove)
            {
                affectedPoints.Remove(p);
            }
        }

        public void ApplyPoint(IAStarPoint point)
        {
            if (affectedPoints.ContainsKey(point)) return;
            affectedPoints.Add(point, new PointSnapshot(point));

            if (overrideWalkable) point.Walkable = walkableValue;
            if (overrideWeight) point.Weight = weightValue;
            if (overrideTypes) point.AllowedAgentTypes = (bool[])allowedAgentTypes.Clone();
        }

        public bool IsWaypointInBounds(Vector3 pos)
        {
            return volumeBounds.Contains(pos);
        }
    }
}