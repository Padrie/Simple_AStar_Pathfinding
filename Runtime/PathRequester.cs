using SimplePathfinding;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace SimplePathfinding
{
    public class PathRequester : MonoBehaviour
    {
        PathRequest aStarPathRequest = new();

        public WayPoint endPoint;
        IAStarPoint randomPoint;

        Color randomColor;

        public bool selectRandomPositionOnRuntime = false;

        [HideInInspector] public bool[] agentTypes = new bool[30];

        CancellationTokenSource cts;

        private void Start()
        {
            cts = new CancellationTokenSource();

            randomPoint = AStar.Instance.SelectRandomPoint(transform.position);
            randomColor = new Color(Random.Range(0.3f, 1f), Random.Range(0.3f, 1f), Random.Range(0.3f, 1f));
            Clock();
            StartCoroutine(SelectRandomPosition());
        }

        private void OnValidate()
        {
            bool anyTrue = false;
            for (int i = 0; i < 30; i++)
                if (agentTypes[i]) { anyTrue = true; break; }

            if (!anyTrue) agentTypes[0] = true;
        }

        IEnumerator SelectRandomPosition()
        {
            while (true)
            {
                if (selectRandomPositionOnRuntime)
                    randomPoint = AStar.Instance.SelectRandomPoint(transform.position);

                yield return new WaitForSeconds(1f);
            }
        }

        async void Clock()
        {
            while (!cts.IsCancellationRequested)
            {
                aStarPathRequest.ClearCosts();
                if (endPoint == null)
                {
                    await aStarPathRequest.RequestPathAsync(
                        AStar.Instance.GetNearestPoint(transform.position), randomPoint, agentTypes);
                }
                else
                {
                    await aStarPathRequest.RequestPathAsync(
                        AStar.Instance.GetNearestPoint(transform.position), endPoint, agentTypes);
                }

                var path = aStarPathRequest.aStarPointPath;
                for (int i = 0; i < path.Count - 1; i++)
                    Debug.DrawLine(path[i].Position, path[i + 1].Position, randomColor, 0.15f);

                await Task.Delay(100);
            }
        }

        private void OnDestroy()
        {
            cts.Cancel();
            cts.Dispose();
        }
    }
}