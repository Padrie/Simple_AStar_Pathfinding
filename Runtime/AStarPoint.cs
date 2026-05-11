using System.Collections.Generic;
using TMPro;
using UnityEngine;


namespace SimplePathfinding
{
    public class AStarPoint : MonoBehaviour
    {
        public bool drawGizmos = true;

        Color patrolPointColor = Color.yellow;

        [HideInInspector, Tooltip("Cost of distance from starting node")] float gScore = 0;
        [HideInInspector, Tooltip("Cost of distance from end node")] float hScore = 0;
        [HideInInspector] public Vector3 pos;

        [SerializeField] TextMeshPro gText;
        [SerializeField] TextMeshPro hText;
        [SerializeField] TextMeshPro fText;

        [HideInInspector] public AStarPoint cameFrom;
        public List<AStarPoint> neighbors;


        public void ChangeGizmoColor(Color color)
        {
            patrolPointColor = color;
        }

        private void Awake()
        {
            pos = transform.position;
        }

        public void UpdateText()
        {
            gText.text = "G=" + gScore;
            hText.text = "H=" + hScore;
            fText.text = "F=" + gScore + hScore;
        }

        public void Setup(float g, Vector3 neighborPos, Vector3 goalPos, AStarPoint cameFrom)
        {
            SetG(g);
            SetH(neighborPos, goalPos);
            this.cameFrom = cameFrom;
        }

        public void Reset()
        {
            cameFrom = null;
            gScore = 0;
            hScore = 0;
            ChangeGizmoColor(Color.yellow);
            UpdateText();
        }

        public void SetG(float score)
        {
            gScore = score;
        }

        public float GetG()
        {
            return gScore;
        }

        private void SetH(Vector3 start, Vector3 goal)
        {
            hScore = (start - goal).sqrMagnitude;
        }

        public float GetH()
        {
            return hScore;
        }

        public float GetF()
        {
            return GetH() + GetG();
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
