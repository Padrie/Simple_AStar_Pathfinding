using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using UnityEngine;

namespace SimplePathfinding
{
    public class AStar : MonoBehaviour
    {
        static readonly Vector3 RAYCAST_OFFSET = new Vector3(0, 0.25f, 0);

        List<IAStarPoint> aStarPoints = new();
        [SerializeField] float selectRandomPointAroundPlayer = 20f;
        [SerializeField] bool drawGizmos = true;
        [SerializeField] PathFindingStyle pathfindingStyle;

        [Header("K Nearest")]
        [SerializeField] LayerMask obstacleMask;
        [SerializeField] int maxNeighbors = 6;
        [SerializeField] float initialRadius = 2f;
        [SerializeField] float radiusStep = 2f;
        [SerializeField] float maxRadius = 8f;
        [SerializeField] bool useRayCast = true;

        public static AStar Instance;

        [SerializeField, HideInInspector] List<AStarPoint> waypointList = new List<AStarPoint>();
        [SerializeField, HideInInspector] List<AStarGridPoint> gridpointList = new List<AStarGridPoint>();
        [SerializeField, HideInInspector] List<AStarGrid> gridList = new List<AStarGrid>();

        private void Awake()
        {
            Instance = this;

            RefreshPoints();
            PopulatePointList();

            aStarPoints.RemoveAll(x => x == null);
        }

        private void Start()
        {
            GetWaypointNeighbors();
            GetGridNeighbors();

            ConnectNeighbors();
            ConnectGridBoundries();
        }

        [ContextMenu("Start Setup")]
        public void SetupPoints()
        {
            RefreshPatrolPoints();
            RefreshGridPoints();

            GetWaypointNeighbors();
            GetGridNeighbors();

            ConnectNeighbors();
            ConnectGridBoundries();
        }

        [ContextMenu("Get Points")]
        public void RefreshPoints()
        {
            RefreshPatrolPoints();
            RefreshGridPoints();
            Debug.Log("Points refreshed. Total points: " + aStarPoints.Count);
        }

        public void RefreshPatrolPoints()
        {
            var points = FindObjectsByType<AStarPoint>();
            waypointList.Clear();

            for (int i = 0; i < points.Length; i++)
            {
                waypointList.Add(points[i]);
            }

            Debug.Log("Way Points refreshed. Total points: " + waypointList.Count);
        }

        public void RefreshGridPoints()
        {
            var grids = FindObjectsByType<AStarGrid>();
            gridpointList.Clear();
            gridList.Clear();

            for (int i = 0; i < grids.Length; i++)
            {
                var points = grids[i].GetGridPoints();
                gridList.Add(grids[i]);

                for (int j = 0; j < points.Count; j++)
                {
                    gridpointList.Add(points[j]);
                }
            }

            print("Grid Points refreshed. Total points: " + gridpointList.Count);
        }

        public void PopulatePointList()
        {
            aStarPoints.Clear();
            aStarPoints.AddRange(waypointList);
            aStarPoints.AddRange(gridpointList);
        }

        [ContextMenu("Clear point lists")]
        public void ClearPoints()
        {
            for (int i = 0; i < aStarPoints.Count; i++)
            {
                aStarPoints[i].Neighbors.Clear();
            }

            gridpointList.Clear();
            waypointList.Clear();
            aStarPoints.Clear();
            gridList.Clear();
        }

        public void CalculatePath(AStarPathRequest aStarPathRequest, Color randomColor)
        {
            bool calculatingPath = true;

            aStarPathRequest.SetCosts(aStarPathRequest.startPoint, 0,
                    SetH(aStarPathRequest.startPoint.Position, aStarPathRequest.endPoint.Position), null);

            aStarPathRequest.openAStarPoints.Enqueue(
                aStarPathRequest.startPoint, aStarPathRequest.GetF(aStarPathRequest.startPoint));

            while (calculatingPath)
            {
                if (aStarPathRequest.openAStarPoints.Count == 0)
                {
                    Debug.LogWarning("Open list is empty");
                    break;
                }

                IAStarPoint currentAStarPoint = aStarPathRequest.openAStarPoints.Dequeue();
                aStarPathRequest.openSet.Remove(currentAStarPoint);

                if (aStarPathRequest.closedAStarPoints.Contains(currentAStarPoint))
                    continue;

                aStarPathRequest.closedAStarPoints.Add(currentAStarPoint);

                if (currentAStarPoint == aStarPathRequest.endPoint)
                {
                    aStarPathRequest.aStarPointPath = ReconstructPath(currentAStarPoint, aStarPathRequest, randomColor);
                    calculatingPath = false;
                    break;
                }

                if (currentAStarPoint.Neighbors != null)
                {
                    foreach (IAStarPoint neighbor in currentAStarPoint.Neighbors)
                    {
                        if (neighbor == null) continue;
                        if (aStarPathRequest.closedAStarPoints.Contains(neighbor))
                        {
                            continue;
                        }

                        float tentativeG = aStarPathRequest.GetG(currentAStarPoint) + Vector3.Distance(currentAStarPoint.Position, neighbor.Position);

                        bool isNewNode = !aStarPathRequest.openSet.Contains(neighbor);

                        if (isNewNode || tentativeG < aStarPathRequest.GetG(neighbor))
                        {
                            aStarPathRequest.SetCosts(neighbor, tentativeG, SetH(neighbor.Position, aStarPathRequest.endPoint.Position), currentAStarPoint);
                            aStarPathRequest.openAStarPoints.Enqueue(neighbor, aStarPathRequest.GetF(neighbor));

                            if (isNewNode)
                                aStarPathRequest.openSet.Add(neighbor);
                        }
                    }
                }
            }
        }

        public List<IAStarPoint> ReconstructPath(IAStarPoint current, AStarPathRequest aStarPathRequest, Color randomColor)
        {
            List<IAStarPoint> path = new List<IAStarPoint>();

            while (current != null)
            {
                path.Add(current);
                current = aStarPathRequest.GetCameFrom(current);
            }

            path.Reverse();

            for (int i = 0; i < path.Count - 1; i++)
            {
                if (path[i] != null)
                {
                    Debug.DrawLine(path[i].Position, path[i + 1].Position, randomColor, .1f);
                }
            }

            return path;
        }

        public void GetWaypointNeighbors()
        {
            int n = waypointList.Count;
            Vector3[] positions = new Vector3[n];

            for (int i = 0; i < n; i++)
                positions[i] = waypointList[i].Position;

            for (int i = 0; i < n; i++)
            {
                IAStarPoint a = waypointList[i];
                a.Neighbors.Clear();

                float searchRadius = initialRadius;
                List<(IAStarPoint p, float dist)> validCandidates = new List<(IAStarPoint p, float dist)>();

                while (searchRadius <= maxRadius)
                {
                    validCandidates.Clear();
                    for (int j = 0; j < n; j++)
                    {
                        if (i == j) continue;

                        float sqrD = (positions[i] - positions[j]).sqrMagnitude;
                        if (sqrD <= searchRadius * searchRadius)
                            validCandidates.Add((waypointList[j], sqrD));
                    }

                    if (validCandidates.Count > 0)
                    {
                        validCandidates = validCandidates.OrderBy(x => x.dist).ToList();

                        List<(IAStarPoint p, float dist)> visible = new List<(IAStarPoint p, float dist)>();
                        foreach (var c in validCandidates)
                        {
                            if (visible.Count >= maxNeighbors) break;

                            Vector3 from = positions[i] + RAYCAST_OFFSET;
                            Vector3 to = c.p.Position + RAYCAST_OFFSET;
                            Vector3 dir = to - from;
                            float dist = dir.magnitude;

                            bool blocked = false;
                            if (useRayCast)
                            {
                                blocked = Physics.Raycast(from, dir.normalized, dist, obstacleMask);
                            }

                            if (!blocked)
                                visible.Add(c);
                        }

                        if (visible.Count > 0)
                        {
                            foreach (var v in visible.Take(maxNeighbors))
                            {
                                a.Neighbors.Add(v.p);
                            }
                            break;
                        }
                    }

                    searchRadius += radiusStep;
                }
            }

            for (int i = 0; i < n; i++)
            {
                IAStarPoint a = waypointList[i];
                foreach (var b in a.Neighbors)
                {
                    if (!b.Neighbors.Contains(a))
                        b.Neighbors.Add(a);
                }
            }
        }

        public void GetGridNeighbors()
        {
            foreach (var grid in gridList)
            {
                Vector3Int[] directions = new Vector3Int[]
                {
                    new Vector3Int(grid.cellSize, 0, 0),
                    new Vector3Int(-grid.cellSize, 0, 0),
                    new Vector3Int(0, grid.cellSize, 0),
                    new Vector3Int(0, -grid.cellSize, 0),
                    new Vector3Int(0, 0, grid.cellSize),
                    new Vector3Int(0, 0, -grid.cellSize)
                };

                foreach (var point in grid.storedGridPoints)
                {
                    for (int i = 0; i < directions.Length; i++)
                    {
                        if (grid.storedGridPoints.TryGetValue(point.Key + directions[i], out var neighbor))
                        {
                            AddGridNeighbor(point.Value, neighbor, directions[i], grid.cellSize);
                        }
                    }
                }

                int totalNeighbors = 0;
                foreach (var point in grid.storedGridPoints)
                    totalNeighbors += point.Value.Neighbors.Count;
            }
        }

        void ConnectGridBoundries()
        {
            foreach (var gridA in gridList)
            {
                Vector3Int[] directions = new Vector3Int[]
                {
                    new Vector3Int(gridA.cellSize, 0, 0),
                    new Vector3Int(-gridA.cellSize, 0, 0),
                    new Vector3Int(0, gridA.cellSize, 0),
                    new Vector3Int(0, -gridA.cellSize, 0),
                    new Vector3Int(0, 0, gridA.cellSize),
                    new Vector3Int(0, 0, -gridA.cellSize)
                };

                foreach (var point in gridA.storedGridPoints)
                {
                    foreach (var gridB in gridList)
                    {
                        if (gridA == gridB) continue;

                        if (gridA.cellSize != gridB.cellSize)
                        {
                            Debug.LogWarning("Grid " + gridA.name + " and " + gridB.name + 
                                " have different cell sizes. This may cause connection issues.");
                        }

                        for (int i = 0; i < directions.Length; i++)
                        {
                            if (gridB.storedGridPoints.TryGetValue(point.Key + directions[i], out var neighbor))
                            {
                                AddGridNeighbor(point.Value, neighbor, directions[i], gridB.cellSize);
                            }
                        }

                    }
                }
            }
        }

        void AddGridNeighbor(AStarGridPoint currentPoint, AStarGridPoint neighbor, Vector3 dir, int length)
        {
            Vector3 from = currentPoint.Position + dir.normalized * 0.1f;

            bool blocked = false;
            if (useRayCast)
            {
                blocked = Physics.Raycast(from, dir.normalized, length, obstacleMask);
            }

            if (!blocked)
            {
                if (!currentPoint.Neighbors.Contains(neighbor))
                    currentPoint.Neighbors.Add(neighbor);
                if (!neighbor.Neighbors.Contains(currentPoint))
                    neighbor.Neighbors.Add(currentPoint);
            }
        }

        public void ConnectNeighbors()
        {
            foreach (var waypoint in waypointList)
            {
                foreach (var gridpoint in gridpointList)
                {
                    Vector3 from = waypoint.Position + RAYCAST_OFFSET;
                    Vector3 to = gridpoint.Position + RAYCAST_OFFSET;
                    Vector3 dir = to - from;
                    float dist = dir.magnitude;
                    float sqrDist = (waypoint.Position - gridpoint.Position).sqrMagnitude;

                    if (sqrDist > initialRadius * initialRadius) continue;

                    bool blocked = false;
                    if (useRayCast)
                    {
                        blocked = Physics.Raycast(from, dir.normalized, dist, obstacleMask);
                    }

                    if (!blocked)
                    {
                        if (!waypoint.Neighbors.Contains(gridpoint))
                            waypoint.Neighbors.Add(gridpoint);
                        if (!gridpoint.Neighbors.Contains(waypoint))
                            gridpoint.Neighbors.Add(waypoint);
                    }
                }
            }
        }

        public IAStarPoint GetNearestPoint(Vector3 pos)
        {
            foreach (var grid in gridList)
            {
                if (grid.IsAgentInGridVolume(pos))
                {
                    var gridPoint = GetNearestGridPoint(pos, grid);
                    if (gridPoint != null)
                    {
                        return gridPoint;
                    }
                }
            }

            return GetNearestWayPoint(pos);
        }

        public IAStarPoint GetNearestWayPoint(Vector3 pos)
        {
            if (aStarPoints == null || aStarPoints.Count == 0) return null;

            IAStarPoint smallestDistanceObject = aStarPoints[0];
            float smallestDistance = (pos - aStarPoints[0].Position).sqrMagnitude;

            for (int i = 1; i < aStarPoints.Count; i++)
            {
                float d = (pos - aStarPoints[i].Position).sqrMagnitude;
                if (d < smallestDistance)
                {
                    smallestDistance = d;
                    smallestDistanceObject = aStarPoints[i];
                }
            }

            return smallestDistanceObject;
        }

        public IAStarPoint GetNearestGridPoint(Vector3 pos, AStarGrid grid)
        {
            Vector3Int origin = grid.GetOrigin();
            Vector3Int nearestKey = new Vector3Int(
                Mathf.RoundToInt((pos.x - origin.x) / grid.cellSize) * grid.cellSize + origin.x,
                Mathf.RoundToInt((pos.y - origin.y) / grid.cellSize) * grid.cellSize + origin.y,
                Mathf.RoundToInt((pos.z - origin.z) / grid.cellSize) * grid.cellSize + origin.z);

            grid.storedGridPoints.TryGetValue(nearestKey, out var nearestPoint);

            if (nearestPoint != null) return nearestPoint;
            else return null;
        }

        public IAStarPoint SelectRandomPatrolPoint(Vector3 pos)
        {
            return aStarPoints[Random.Range(0, aStarPoints.Count - 1)];
        }

        private float SetH(Vector3 a, Vector3 b)
        {
            switch (pathfindingStyle)
            {
                case PathFindingStyle.Grid:
                    //return ManhattanDistance(a, b, 1);
                    return (a - b).sqrMagnitude;
                case PathFindingStyle.Linear:
                    return (a - b).magnitude;
                case PathFindingStyle.Weight2:
                    return (a - b).magnitude * 2;
                case PathFindingStyle.Weight3:
                    return (a - b).magnitude * 3;
                default:
                    return (a - b).sqrMagnitude;
            }
        }

        private float ManhattanDistance(Vector3 a, Vector3 b, float multiplier)
        {
            float dx = Mathf.Abs(a.x - b.x);
            float dy = Mathf.Abs(a.y - b.y);
            float dz = Mathf.Abs(a.z - b.z);

            return multiplier * (dx + dy + dz);
        }

        private bool IsDiagonal(Vector3 a, Vector3 b)
        {
            int count = 0;
            if (Mathf.Abs(a.x - b.x) > 0.01f) count++;
            if (Mathf.Abs(a.y - b.y) > 0.01f) count++;
            if (Mathf.Abs(a.z - b.z) > 0.01f) count++;

            if (count >= 2)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
