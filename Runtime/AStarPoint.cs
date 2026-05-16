using System.Collections.Generic;
using TMPro;
using UnityEngine;


namespace SimplePathfinding
{
    public class AStarPoint : MonoBehaviour
    {
        public bool drawGizmos = true;

        Color patrolPointColor = Color.yellow;

        [HideInInspector] public Vector3 pos;

        //[SerializeField] TextMeshPro gText;
        //[SerializeField] TextMeshPro hText;
        //[SerializeField] TextMeshPro fText;

        public List<AStarPoint> neighbors;


        public void ChangeGizmoColor(Color color)
        {
            patrolPointColor = color;
        }

        private void Awake()
        {
            pos = transform.position;
        }

        private void OnDrawGizmos()
        {
            if (drawGizmos)
            {
                Gizmos.color = patrolPointColor;
                //Gizmos.DrawCube(transform.position, Vector3.one / 3);
            }
        }
    }
}
