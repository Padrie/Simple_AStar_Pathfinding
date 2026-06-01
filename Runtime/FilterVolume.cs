using System.Collections;
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
        [Tooltip("Default is 1"), SerializeField, Min(0.1f)] float weightValue = 1f;

        [SerializeField] bool overrideTypes = false;
        [HideInInspector, SerializeField] public bool[] agentTypes = new bool[30];

        Dictionary<IAStarPoint, PointSnapshot> affectedPoints = new();

        AStar aStar;

        Vector3Int origin;

        Vector3 minWorldBounds;
        Vector3 maxWorldBounds;

        Bounds volumeBounds;

        private void Awake()
        {
            aStar = FindAnyObjectByType<AStar>();
        }

        private void Start()
        {
            switch (mode)
            {
                case FilterVolumeMode.Once:
                    ApplyToPoints();
                    break;
                case FilterVolumeMode.Realtime:
                    StartCoroutine(RealtimeLoop());
                    break;
                case FilterVolumeMode.Volume:
                    break;
                default:
                    break;
            }
        }

        IEnumerator RealtimeLoop()
        {
            while (true)
            {
                ApplyToPoints();

                yield return new WaitForSeconds(updateInterval);
            }
        }

        public void ApplyToPoints()
        {
            volumeBounds = new Bounds(transform.position, size);

            minWorldBounds = transform.position - size / 2;
            maxWorldBounds = transform.position + size / 2;

            GetWaypoints();
            GetGridPoints();
            RemoveExitedPoints();
        }

        private void GetWaypoints()
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

        private void GetGridPoints()
        {
            foreach (var grid in aStar.gridList)
            {
                origin = grid.GetOrigin();

                int minX = Mathf.RoundToInt((minWorldBounds.x - origin.x) / grid.cellSize) * grid.cellSize + origin.x;
                int maxX = Mathf.RoundToInt((maxWorldBounds.x - origin.x) / grid.cellSize) * grid.cellSize + origin.x;

                int minY = Mathf.RoundToInt((minWorldBounds.y - origin.y) / grid.cellSize) * grid.cellSize + origin.y;
                int maxY = Mathf.RoundToInt((maxWorldBounds.y - origin.y) / grid.cellSize) * grid.cellSize + origin.y;

                int minZ = Mathf.RoundToInt((minWorldBounds.z - origin.z) / grid.cellSize) * grid.cellSize + origin.z;
                int maxZ = Mathf.RoundToInt((maxWorldBounds.z - origin.z) / grid.cellSize) * grid.cellSize + origin.z;

                for (int x = minX; x <= maxX; x += grid.cellSize)
                {
                    for (int y = minY; y <= maxY; y += grid.cellSize)
                    {
                        for (int z = minZ; z <= maxZ; z += grid.cellSize)
                        {
                            Vector3Int coord = new Vector3Int(x, y, z);

                            if (grid.storedGridPoints.TryGetValue(coord, out var point))
                            {
                                ApplyPoint(point);
                            }
                        }
                    }
                }
            }
        }

        private void RemoveExitedPoints()
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

        private void ApplyPoint(IAStarPoint point)
        {
            if (affectedPoints.ContainsKey(point)) return;
            affectedPoints.Add(point, new PointSnapshot(point));

            if (overrideWalkable) point.Walkable = walkableValue;
            if (overrideWeight) point.Weight = weightValue;
            if (overrideTypes) point.AllowedAgentTypes = (bool[])agentTypes.Clone();
        }

        private bool IsWaypointInBounds(Vector3 pos)
        {
            return volumeBounds.Contains(pos);
        }

        private void OnDestroy()
        {
            foreach (var point in affectedPoints)
                point.Value.Restore();
            affectedPoints.Clear();
        }

        public void OnDrawGizmos()
        {
            Gizmos.DrawWireCube(transform.position, size);
        }
    }
}