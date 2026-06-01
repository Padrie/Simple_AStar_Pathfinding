using System.Collections.Generic;
using System.Linq;
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

        [Header("Chunk Settings")]
        public int chunkSize = 10;
        public Dictionary<Vector3Int, PointChunk> waypointChunks = new();
        public bool drawChunkGizmos = true;

        [Header("K Nearest")]
        [SerializeField] LayerMask obstacleMask;
        [SerializeField] int maxNeighbors = 6;
        [SerializeField] float initialRadius = 2f;
        [SerializeField] float radiusStep = 2f;
        [SerializeField] float maxRadius = 8f;
        [SerializeField] bool useRayCast = true;

        [HideInInspector] public string[] agentTypes = new string[30];

        public static AStar Instance;

        [HideInInspector] public List<WayPoint> waypointList = new List<WayPoint>();
        [HideInInspector] public List<GridPoint> gridpointList = new List<GridPoint>();
        [HideInInspector] public List<AStarGrid> gridList = new List<AStarGrid>();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
            }
            else
            {
                Instance = this;
            }

            PopulatePointList();
            PopulateWaypointChunks();

            foreach (var grid in gridList)
                grid.storedGridPoints.Clear();

            foreach (var point in gridpointList)
                foreach (var grid in gridList)
                    if (grid.IsAgentInGridVolume(point.Position))
                    {
                        grid.storedGridPoints[Vector3Int.RoundToInt(point.Position)] = point;
                        break;
                    }

            aStarPoints.RemoveAll(x => x == null);
        }

        private void Start()
        {
            PopulatePointNeighbors();
        }

        public void SetupPoints()
        {
            ClearPoints();

            RefreshPatrolPoints();
            RefreshGridPoints();

            GetWaypointNeighbors();
            GetGridNeighbors();

            ConnectNeighbors();
            ConnectGridBoundries();

            SaveNeighborKeys();

            PopulateWaypointChunks();
        }

        private void RefreshPatrolPoints()
        {
            var points = FindObjectsByType<WayPoint>();
            waypointList.Clear();

            for (int i = 0; i < points.Length; i++)
            {
                waypointList.Add(points[i]);
            }
        }

        private void RefreshGridPoints()
        {
            var grids = FindObjectsByType<AStarGrid>();
            gridpointList.Clear();
            gridList.Clear();

            for (int i = 0; i < grids.Length; i++)
            {
                gridList.Add(grids[i]);
                foreach (var point in grids[i].GetGridPoints())
                {
                    point.allowedAgentTypes = (bool[])grids[i].allowedAgentTypes.Clone();
                    gridpointList.Add(point);
                }
            }
        }

        public void PopulatePointList()
        {
            aStarPoints.Clear();
            aStarPoints.AddRange(waypointList);
            aStarPoints.AddRange(gridpointList);
        }

        public void PopulateWaypointChunks()
        {
            waypointChunks.Clear();

            foreach (var waypoint in waypointList)
            {
                Vector3Int chunkPos = ConvertToChunkCoord(waypoint.Position);

                if (waypointChunks.TryGetValue(chunkPos, out var chunk))
                {
                    chunk.pointList.Add(waypoint);
                }
                else
                {
                    PointChunk newChunk = new PointChunk(chunkPos);
                    waypointChunks.Add(chunkPos, newChunk);
                    newChunk.pointList.Add(waypoint);
                }
            }
        }

        public void PopulatePointNeighbors()
        {
            Dictionary<Vector3, IAStarPoint> allPoints = new();

            foreach (var point in gridpointList)
            {
                allPoints[Vector3Int.RoundToInt(point.Position)] = point;
            }

            foreach (var waypoint in waypointList)
            {
                allPoints[waypoint.Position] = waypoint;
            }

            foreach (var point in gridpointList)
            {
                if (point == null) continue;

                foreach (var neighborPos in point.serializedNeighbors)
                {
                    if (allPoints.TryGetValue(neighborPos, out var neighbor))
                    {
                        point.Neighbors.Add(neighbor);
                        continue;
                    }

                    Vector3Int vec = Vector3Int.RoundToInt(neighborPos);
                    if (allPoints.TryGetValue(vec, out var roundedNeighbor))
                        point.Neighbors.Add(roundedNeighbor);
                }
            }

            foreach (var waypoint in waypointList)
            {
                foreach (var neighborPos in waypoint.serializedNeighbors)
                {
                    if (allPoints.TryGetValue(neighborPos, out var neighbor))
                    {
                        waypoint.Neighbors.Add(neighbor);
                        continue;
                    }

                    Vector3Int vec = Vector3Int.RoundToInt(neighborPos);
                    if (allPoints.TryGetValue(vec, out var roundedNeighbor))
                        waypoint.Neighbors.Add(roundedNeighbor);
                }
            }
        }

        private void SaveNeighborKeys()
        {
            foreach (var point in waypointList)
            {
                point.serializedNeighbors.Clear();
                foreach (var neighbor in point.Neighbors)
                {
                    point.serializedNeighbors.Add(neighbor.Position);
                }
            }

            foreach (var point in gridpointList)
            {
                point.serializedNeighbors.Clear();
                foreach (var neighbor in point.Neighbors)
                {
                    point.serializedNeighbors.Add(neighbor.Position);
                }
            }
        }

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

        public void CalculatePath(PathRequest aStarPathRequest)
        {
            if (aStarPathRequest.startPoint == null || aStarPathRequest.endPoint == null)
            {
                Debug.LogWarning("Path request has null start or end point");
                return;
            }

            bool calculatingPath = true;

            aStarPathRequest.SetCosts(aStarPathRequest.startPoint, 0,
                    SetH(aStarPathRequest.startPoint.Position, aStarPathRequest.endPoint.Position), null);

            aStarPathRequest.openAStarPoints.Enqueue(
                aStarPathRequest.startPoint, aStarPathRequest.GetF(aStarPathRequest.startPoint));

            while (calculatingPath)
            {
                IAStarPoint currentAStarPoint = aStarPathRequest.openAStarPoints.Dequeue();
                if (currentAStarPoint == null) break;
                aStarPathRequest.openSet.Remove(currentAStarPoint);

                if (aStarPathRequest.closedAStarPoints.Contains(currentAStarPoint))
                    continue;

                aStarPathRequest.closedAStarPoints.Add(currentAStarPoint);

                if (currentAStarPoint == aStarPathRequest.endPoint)
                {
                    aStarPathRequest.aStarPointPath = ReconstructPath(currentAStarPoint, aStarPathRequest);
                    calculatingPath = false;
                    break;
                }

                if (currentAStarPoint.Neighbors != null)
                {
                    foreach (IAStarPoint neighbor in currentAStarPoint.Neighbors)
                    {
                        if (neighbor == null) continue;

                        if (!neighbor.Walkable) continue;

                        if (aStarPathRequest.closedAStarPoints.Contains(neighbor)) continue;
                        
                        bool allowed = false;

                        for (int i = 0; i < 30; i++)
                        {
                            if (aStarPathRequest.agentTypes[i] && neighbor.AllowedAgentTypes[i])
                            {
                                allowed = true;
                                break;
                            }
                        }
                        if (!allowed) continue;

                        float crossSystemMultiplier = 1f;
                        if ((currentAStarPoint is WayPoint) != (neighbor is WayPoint))
                            crossSystemMultiplier = 1.5f;

                        float tentativeG = aStarPathRequest.GetG(currentAStarPoint) +
                            Vector3.Distance(currentAStarPoint.Position, neighbor.Position) * neighbor.Weight * crossSystemMultiplier;

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

        private List<IAStarPoint> ReconstructPath(IAStarPoint current, PathRequest aStarPathRequest)
        {
            List<IAStarPoint> path = new List<IAStarPoint>();

            while (current != null)
            {
                path.Add(current);
                current = aStarPathRequest.GetCameFrom(current);
            }

            path.Reverse();
            return path;
        }

        private void GetWaypointNeighbors()
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

        private void GetGridNeighbors()
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
                    new Vector3Int(0, 0, -grid.cellSize),
                    new Vector3Int(grid.cellSize, 0, grid.cellSize),
                    new Vector3Int(grid.cellSize, 0, -grid.cellSize),
                    new Vector3Int(-grid.cellSize, 0, grid.cellSize),
                    new Vector3Int(-grid.cellSize, 0, -grid.cellSize)
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

            int diagonalConnections = 0;
            foreach (var point in gridpointList)
                foreach (var neighbor in point.Neighbors)
                {
                    var diff = neighbor.Position - point.Position;
                    int axisCount = 0;
                    if (Mathf.Abs(diff.x) > 0.01f) axisCount++;
                    if (Mathf.Abs(diff.y) > 0.01f) axisCount++;
                    if (Mathf.Abs(diff.z) > 0.01f) axisCount++;
                    if (axisCount >= 2) diagonalConnections++;
                }
        }

        private void ConnectGridBoundries()
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
                    new Vector3Int(0, 0, -gridA.cellSize),
                    new Vector3Int(gridA.cellSize, 0, gridA.cellSize),
                    new Vector3Int(gridA.cellSize, 0, -gridA.cellSize),
                    new Vector3Int(-gridA.cellSize, 0, gridA.cellSize),
                    new Vector3Int(-gridA.cellSize, 0, -gridA.cellSize)
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

        private void AddGridNeighbor(GridPoint currentPoint, GridPoint neighbor, Vector3 dir, int length)
        {
            Vector3 from = currentPoint.Position + dir.normalized * 0.1f;

            bool blocked = false;
            if (useRayCast)
            {
                blocked = Physics.Raycast(from, dir.normalized, dir.magnitude, obstacleMask);
            }

            if (!blocked)
            {
                if (!currentPoint.Neighbors.Contains(neighbor))
                    currentPoint.Neighbors.Add(neighbor);
                if (!neighbor.Neighbors.Contains(currentPoint))
                    neighbor.Neighbors.Add(currentPoint);
            }
        }

        private void ConnectNeighbors()
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

            IAStarPoint smallestDistanceObject = null;
            float smallestDistance = float.MaxValue;
            Vector3Int baseCoord = ConvertToChunkCoord(pos);

            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    for (int z = -1; z <= 1; z++)
                    {
                        Vector3Int agentChunkCoord = baseCoord + new Vector3Int(x, y, z);
                        if (waypointChunks.TryGetValue(agentChunkCoord, out var chunk))
                        {
                            for (int i = 0; i < chunk.pointList.Count; i++)
                            {
                                float d = (pos - chunk.pointList[i].Position).sqrMagnitude;

                                if (d < smallestDistance)
                                {
                                    smallestDistance = d;
                                    smallestDistanceObject = chunk.pointList[i];
                                }
                            }
                        }
                    }
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

        public IAStarPoint SelectRandomPoint(Vector3 pos)
        {
            List<IAStarPoint> pointsInRadius = new List<IAStarPoint>();

            foreach (var point in aStarPoints)
            {
                if (Vector3.Distance(point.Position, pos) <= selectRandomPointAroundPlayer)
                    pointsInRadius.Add(point);
            }

            if (pointsInRadius.Count == 0)
                return aStarPoints[Random.Range(0, aStarPoints.Count)];

            return pointsInRadius[Random.Range(0, pointsInRadius.Count)];
        }

        private float SetH(Vector3 a, Vector3 b)
        {
            switch (pathfindingStyle)
            {
                case PathFindingStyle.Weight1:
                    return (a - b).magnitude;
                case PathFindingStyle.Weight2:
                    return (a - b).magnitude * 2;
                case PathFindingStyle.Weight3:
                    return (a - b).magnitude * 3;
                default:
                    return (a - b).magnitude;
            }
        }

        public Vector3Int ConvertToChunkCoord(Vector3 pos)
        {
            int x = Mathf.FloorToInt(pos.x / chunkSize);
            int y = Mathf.FloorToInt(pos.y / chunkSize);
            int z = Mathf.FloorToInt(pos.z / chunkSize);

            return new Vector3Int(x, y, z);
        }
    }
}
