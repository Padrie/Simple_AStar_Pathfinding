using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace SimplePathfinding
{
    public class PathRequester : MonoBehaviour
    {
        PathRequest pathRequest = new();

        public WayPoint endPoint;
        [SerializeField] PathRequestMode startupBehaviour = PathRequestMode.Realtime;
        [Tooltip("Seconds between path requests"), SerializeField, Min(0.001f)] float updateInterval = 0.1f;

        [SerializeField] bool selectRandomPointOnStart = false;
        [SerializeField] bool drawPath = true;
        [HideInInspector] public bool[] agentTypes = new bool[30];

        IAStarPoint randomPoint;
        Color randomColor;
        List<IAStarPoint> currentPath;
        CancellationTokenSource cts;
        bool isRequesting = true;
        bool isGettingPath;

        public IReadOnlyList<IAStarPoint> Path => currentPath;
        public event System.Action<List<IAStarPoint>> OnPathReady;

        int pathIndex = 0;

        public Vector3 CurrentPoint => (currentPath != null && pathIndex < currentPath.Count)
            ? currentPath[pathIndex].Position
            : transform.position;

        public bool HasPath => currentPath != null && currentPath.Count > 0;
        public bool HasReachedEnd => currentPath == null || pathIndex >= currentPath.Count - 1;
        public float PathLength
        {
            get
            {
                if (currentPath == null || currentPath.Count < 2) return 0f;

                float total = 0f;

                for (int i = 0; i < currentPath.Count - 1; i++)
                {
                    total += Vector3.Distance(currentPath[i].Position, currentPath[i + 1].Position);
                }

                return total;
            }
        }

        private void Awake()
        {
            cts = new CancellationTokenSource();
        }

        private void Start()
        {
            if (selectRandomPointOnStart && endPoint == null)
                randomPoint = AStar.Instance.SelectRandomPoint(transform.position);

            randomColor = new Color(Random.Range(0.3f, 1f), Random.Range(0.3f, 1f), Random.Range(0.3f, 1f));

            switch (startupBehaviour)
            {
                case PathRequestMode.Once:
                    isRequesting = false;
                    RequestPath();
                    break;
                case PathRequestMode.Realtime:
                    isRequesting = true;
                    break;
                case PathRequestMode.Manual:
                    isRequesting = false;
                    break;
                default:
                    break;
            }

            Loop();
        }

        private void OnValidate()
        {
            bool anyTrue = false;
            for (int i = 0; i < 30; i++)
                if (agentTypes[i]) { anyTrue = true; break; }

            if (!anyTrue) agentTypes[0] = true;
        }

        private void Update()
        {
            if (!drawPath || currentPath == null) return;

            for (int i = 0; i < currentPath.Count - 1; i++)
                Debug.DrawLine(currentPath[i].Position, currentPath[i + 1].Position, randomColor, 0f);
        }

        private async Task DoRequest()
        {
            if (isGettingPath) return;
            isGettingPath = true;

            try
            {
                pathRequest.ClearCosts();

                IAStarPoint target = endPoint != null ? endPoint : randomPoint;

                await pathRequest.RequestPathAsync(
                    AStar.Instance.GetNearestPoint(transform.position), target, agentTypes);

                currentPath = new List<IAStarPoint>(pathRequest.pointPath);

                pathIndex = 0;
                if (currentPath.Count > 0)
                    OnPathReady?.Invoke(currentPath);
            }
            finally
            {
                isGettingPath = false;
            }
        }

        public async void RequestPath()
        {
            if (cts.IsCancellationRequested) return;
            await DoRequest();
        }

        private async void Loop()
        {
            while (!cts.IsCancellationRequested)
            {
                if (isRequesting)
                    await DoRequest();
                await Task.Delay(Mathf.RoundToInt(updateInterval * 1000));
            }
        }

        public bool TryGetNearestPoint(out Vector3 position)
        {
            var point = AStar.Instance.GetNearestPoint(transform.position);

            if (AStar.Instance == null)
            {
                position = default;
                return false;
            }

            position = point.Position;

            return true;
        }

        public bool AdvancePoint()
        {
            if (currentPath == null || pathIndex >= currentPath.Count - 1) return false;
            pathIndex++;
            return true;
        }

        public void StopRequesting() => isRequesting = false;
        public void StartRequesting() => isRequesting = true;

        private void OnDestroy()
        {
            cts.Cancel();
            cts.Dispose();
        }
    }
}