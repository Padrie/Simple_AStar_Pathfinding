using SimplePathfinding;
using System.Collections;
using UnityEngine;

namespace SimplePathfinding
{
    public class AStarPathRequester : MonoBehaviour
    {
        AStarPathRequest aStarPathRequest = new();

        AStarPoint randomPoint;

        Color randomColor;

        public bool selectRandomPositionOnRuntime = false;

        private void Start()
        {
            randomPoint = AStar.Instance.SelectRandomPatrolPoint(transform.position);
            randomColor = new Color(Random.Range(0.3f, 1f), Random.Range(0.3f, 1f), Random.Range(0.3f, 1f));
            StartCoroutine(Clock());
            StartCoroutine(SelectRandomPosition());
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
                aStarPathRequest.RequestPath(
                    AStar.Instance.getNearestPatrolPoint(transform.position).GetComponent<AStarPoint>(), randomPoint, randomColor);

                yield return new WaitForSeconds(0.1f);

                aStarPathRequest.ClearCosts();
            }
        }
    }
}