using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SimplePathfinding
{
    public class AStar : MonoBehaviour
    {
        static readonly Vector3 RAYCAST_OFFSET = new Vector3(0, 0.25f, 0);

        [SerializeField] List<AStarPoint> aStarPoints;
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

        HashSet<AStarPoint> touchedPatrolPoints = new HashSet<AStarPoint>();

        public static AStar Instance;

        private void Awake()
        {
            Instance = this;

            RefreshPatrolPoints();

            aStarPoints.RemoveAll(x => !x);
        }

        private void Start()
        {
            if (!drawGizmos)
            {
                foreach (AStarPoint p in aStarPoints)
                {
                    p.drawGizmos = false;
                }
            }

            GetNeighbors();
        }

        //TODO: Replace FindGameObjectsWithTag with a better solution
        public void RefreshPatrolPoints()
        {
            GameObject[] a = GameObject.FindGameObjectsWithTag("AStarPoints");
            for (int i = 0; i < a.Length; i++)
                aStarPoints.Add(a[i].GetComponent<AStarPoint>());

            Debug.Log("Patrol Points refreshed. Total points: " + aStarPoints.Count);
        }

        public void CalculatePath(AStarPathRequest aStarPathRequest, Color randomColor)
        {
            bool calculatingPath = true;

            PriorityQueue<AStarPoint> openAStarPoints = new PriorityQueue<AStarPoint>();
            HashSet<AStarPoint> openSet = new HashSet<AStarPoint>();
            HashSet<AStarPoint> closedAStarPoints = new HashSet<AStarPoint>();

            aStarPathRequest.SetCosts(aStarPathRequest.startPoint, 0,
                SetH(aStarPathRequest.startPoint.transform.position, aStarPathRequest.endPoint.transform.position), null);

            openAStarPoints.Enqueue(aStarPathRequest.startPoint, aStarPathRequest.GetF(aStarPathRequest.startPoint));

            while (calculatingPath)
            {
                if (openAStarPoints.Count == 0)
                {
                    print(openAStarPoints.Count);
                    Debug.LogWarning("Open list is empty");
                    break;
                }

                AStarPoint currentAStarPoint = openAStarPoints.Dequeue();
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

                if (currentAStarPoint.neighbors != null)
                {
                    foreach (AStarPoint neighbor in currentAStarPoint.neighbors)
                    {
                        if (neighbor == null) continue;
                        if (closedAStarPoints.Contains(neighbor))
                        {
                            continue;
                        }

                        float tentativeG = aStarPathRequest.GetG(currentAStarPoint) + Vector3.Distance(currentAStarPoint.pos, neighbor.pos);

                        bool isNewNode = !openSet.Contains(neighbor);

                        if (isNewNode || tentativeG < aStarPathRequest.GetG(neighbor))
                        {
                            aStarPathRequest.SetCosts(neighbor, tentativeG, SetH(neighbor.pos, aStarPathRequest.endPoint.pos), currentAStarPoint);
                            openAStarPoints.Enqueue(neighbor, aStarPathRequest.GetF(neighbor));

                            if (isNewNode)
                                openSet.Add(neighbor);

                            //setupVisuals(neighbor);
                        }
                    }
                }
            }
        }

        public List<AStarPoint> ReconstructPath(AStarPoint current, AStarPathRequest aStarPathRequest, Color randomColor)
        {
            List<AStarPoint> path = new List<AStarPoint>();

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
                    Debug.DrawLine(path[i].transform.position, path[i + 1].transform.position, randomColor, .1f);
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
                positions[i] = aStarPoints[i].pos;

            for (int i = 0; i < n; i++)
            {
                AStarPoint a = aStarPoints[i];
                a.neighbors.Clear();

                float searchRadius = initialRadius;
                List<(AStarPoint p, float dist)> validCandidates = new List<(AStarPoint p, float dist)>();

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

                        List<(AStarPoint p, float dist)> visible = new List<(AStarPoint p, float dist)>();
                        foreach (var c in validCandidates)
                        {
                            if (visible.Count >= maxNeighbors) break;

                            Vector3 from = positions[i] + RAYCAST_OFFSET;
                            Vector3 to = c.p.pos + RAYCAST_OFFSET;
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
                                a.neighbors.Add(v.p);
                            }
                            break;
                        }
                    }

                    searchRadius += radiusStep;
                }
            }

            for (int i = 0; i < n; i++)
            {
                AStarPoint a = aStarPoints[i];
                foreach (var b in a.neighbors)
                {
                    if (!b.neighbors.Contains(a))
                        b.neighbors.Add(a);
                }
            }
        }

        public GameObject getNearestPatrolPoint(Vector3 pos)
        {
            if (aStarPoints == null || aStarPoints.Count == 0) return null;

            GameObject smallestDistanceObject = aStarPoints[0].gameObject;
            float smallestDistance = (pos - aStarPoints[0].pos).sqrMagnitude;

            for (int i = 1; i < aStarPoints.Count; i++)
            {
                float d = (pos - aStarPoints[i].pos).sqrMagnitude;
                if (d < smallestDistance)
                {
                    smallestDistance = d;
                    smallestDistanceObject = aStarPoints[i].gameObject;
                }
            }

            return smallestDistanceObject;
        }

        public AStarPoint SelectRandomPatrolPoint(Vector3 pos)
        {
            if (aStarPoints == null || aStarPoints.Count == 0)
                return null;

            var nearbyPoints = aStarPoints
                .Where(p => (pos - p.transform.position).sqrMagnitude <= selectRandomPointAroundPlayer * selectRandomPointAroundPlayer)
                .ToList();
            if (nearbyPoints.Count > 0)
            {
                return nearbyPoints[Random.Range(0, nearbyPoints.Count)];
            }
            else
            {
                var sorted = aStarPoints
                    .OrderBy(p => (pos - p.transform.position).sqrMagnitude)
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
