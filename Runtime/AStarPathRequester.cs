using SimplePathfinding;
using System.Collections;
using UnityEngine;

namespace SimplePathfinding
{
    public class AStarPathRequester : MonoBehaviour
    {
        AStarPathRequest aStarPathRequest = new();

        public WayPoint endPoint;
        IAStarPoint randomPoint;

        Color randomColor;

        public bool selectRandomPositionOnRuntime = false;

        [HideInInspector] public bool[] agentTypes = new bool[30];

        private void Start()
        {
            randomPoint = AStar.Instance.SelectRandomPatrolPoint(transform.position);
            randomColor = new Color(Random.Range(0.3f, 1f), Random.Range(0.3f, 1f), Random.Range(0.3f, 1f));
            StartCoroutine(Clock());
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
                if(selectRandomPositionOnRuntime)
                    randomPoint = AStar.Instance.SelectRandomPatrolPoint(transform.position);

                yield return new WaitForSeconds(1f);
            }
        }

        IEnumerator Clock()
        {
            while (true)
            {
                aStarPathRequest.ClearCosts();
                aStarPathRequest.RequestPath(
                    AStar.Instance.GetNearestPoint(transform.position), randomPoint, randomColor, agentTypes);
                yield return new WaitForSeconds(0.1f);

            }
        }
    }
}