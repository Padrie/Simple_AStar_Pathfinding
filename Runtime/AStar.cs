using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SimplePathfinding
{
    public class AStar : MonoBehaviour
    {
        static readonly Vector3 RAYCAST_OFFSET = new Vector3(0, 0.25f, 0);

        [SerializeField] List<IAStarPoint> aStarPoints = new();
        [SerializeField] float selectRandomPointAroundPlayer = 20f;
        [SerializeField] bool drawGizmos = true;
        [SerializeField] PathFindingStyle pathfindingStyle;

        [Header("K Nearest")]
        [SerializeField] LayerMask obstacleMask;
        [SerializeField] int maxNeighbors = 4;
        [SerializeField] float initialRadius = 8f;
        [SerializeField] float radiusStep = 8f;
        [SerializeField] float maxRadius = 64f;
        [SerializeField] bool useRayCast = true;

        HashSet<IAStarPoint> touchedPatrolPoints = new HashSet<IAStarPoint>();

        public static AStar Instance;

        private void Awake()
        {
            Instance = this;

            RefreshPatrolPoints();

            aStarPoints.RemoveAll(x => x == null);
        }

        private void Start()
        {
            GetNeighbors();
        }

        //TODO: Replace FindGameObjectsWithTag with a better solution
        public void RefreshPatrolPoints()
        {
            GameObject[] a = GameObject.FindGameObjectsWithTag("AStarPoints");
            for (int i = 0; i < a.Length; i++)
                aStarPoints.Add(a[i].GetComponent<IAStarPoint>());

            int nullCount = 0;
            foreach (var p in aStarPoints)
                if (p == null) nullCount++;
            Debug.Log("Null points: " + nullCount);

            Debug.Log("Patrol Points refreshed. Total points: " + aStarPoints.Count);
        }

        public void CalculatePath(AStarPathRequest aStarPathRequest, Color randomColor)
        {
            bool calculatingPath = true;

            PriorityQueue<IAStarPoint> openAStarPoints = new PriorityQueue<IAStarPoint>();
            HashSet<IAStarPoint> openSet = new HashSet<IAStarPoint>();
            HashSet<IAStarPoint> closedAStarPoints = new HashSet<IAStarPoint>();

            aStarPathRequest.SetCosts(aStarPathRequest.startPoint, 0,
                SetH(aStarPathRequest.startPoint.Position, aStarPathRequest.endPoint.Position), null);

            openAStarPoints.Enqueue(aStarPathRequest.startPoint, aStarPathRequest.GetF(aStarPathRequest.startPoint));

            while (calculatingPath)
            {
                if (openAStarPoints.Count == 0)
                {
                    print(openAStarPoints.Count);
                    Debug.LogWarning("Open list is empty");
                    break;
                }

                IAStarPoint currentAStarPoint = openAStarPoints.Dequeue();
                openSet.Remove(currentAStarPoint);

                if (closedAStarPoints.Contains(currentAStarPoint))
                    continue;

                closedAStarPoints.Add(currentAStarPoint);

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
                        if (closedAStarPoints.Contains(neighbor))
                        {
                            continue;
                        }

                        float tentativeG = aStarPathRequest.GetG(currentAStarPoint) + Vector3.Distance(currentAStarPoint.Position, neighbor.Position);

                        bool isNewNode = !openSet.Contains(neighbor);

                        if (isNewNode || tentativeG < aStarPathRequest.GetG(neighbor))
                        {
                            aStarPathRequest.SetCosts(neighbor, tentativeG, SetH(neighbor.Position, aStarPathRequest.endPoint.Position), currentAStarPoint);
                            openAStarPoints.Enqueue(neighbor, aStarPathRequest.GetF(neighbor));

                            if (isNewNode)
                                openSet.Add(neighbor);

                            //setupVisuals(neighbor);
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

        [ContextMenu("Rebuild Neighbors")]
        public void GetNeighbors()
        {
            int n = aStarPoints.Count;
            Vector3[] positions = new Vector3[n];

            for (int i = 0; i < n; i++)
                positions[i] = aStarPoints[i].Position;

            for (int i = 0; i < n; i++)
            {
                IAStarPoint a = aStarPoints[i];
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
                            validCandidates.Add((aStarPoints[j], sqrD));
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
                IAStarPoint a = aStarPoints[i];
                foreach (var b in a.Neighbors)
                {
                    if (!b.Neighbors.Contains(a))
                        b.Neighbors.Add(a);
                }
            }
        }

        public IAStarPoint getNearestPatrolPoint(Vector3 pos)
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

        public IAStarPoint SelectRandomPatrolPoint(Vector3 pos)
        {
            if (aStarPoints == null || aStarPoints.Count == 0)
                return null;

            var nearbyPoints = aStarPoints
                .Where(p => (pos - p.Position).sqrMagnitude <= selectRandomPointAroundPlayer * selectRandomPointAroundPlayer)
                .ToList();
            if (nearbyPoints.Count > 0)
            {
                return nearbyPoints[Random.Range(0, nearbyPoints.Count)];
            }
            else
            {
                var sorted = aStarPoints
                    .OrderBy(p => (pos - p.Position).sqrMagnitude)
                    .Take(5)
                    .ToList();

                return sorted[Random.Range(0, sorted.Count)];
            }
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
                Debug.Log("More than two axes A: " + a + " B: " + b);
                return true;
            }
            else
            {
                Debug.Log("Less than two axes A: " + a + " B: " + b);
                return false;
            }
        }
    }
}
