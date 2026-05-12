using SimplePathfinding;
using System.Collections;
using UnityEngine;

namespace SimplePathfinding
{
    public class AStarPathRequester : MonoBehaviour
    {
        private void Start()
        {
            StartCoroutine(Clock());
        }

        IEnumerator Clock()
        {
            while (true)
            {
                //AStar.Instance.GetPath();
                yield return new WaitForSeconds(0.01f);
            }

            yield return null;
        }
    }
}