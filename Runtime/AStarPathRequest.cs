using System;
using System.Collections.Generic;
using UnityEngine;

namespace SimplePathfinding
{
    public class AStarPathRequest
    {
        public Dictionary<AStarPoint, (float g,float h)> storedCosts = new();
        public List<AStarPoint> aStarPointPath = new();
        public AStarPoint camefrom;

        public AStarPoint startPoint;
        public AStarPoint endPoint;

        public void RequestPath(AStarPoint start, AStarPoint end)
        {
            endPoint = AStar.Instance.SelectRandomPatrolPoint();
            startPoint = AStar.Instance.getNearestPatrolPoint
                (AStar.Instance.currentRequester.transform.position).GetComponent<AStarPoint>();
        }

        public void SetCosts(AStarPoint point, float g, float h, AStarPoint from)
        {
            storedCosts[point] = (g,h);
            camefrom = from;
        }

        public void ClearCosts()
        {
            storedCosts.Clear();
        }

        public float GetG(AStarPoint point) => storedCosts.TryGetValue(point, out var costs) ? costs.g : float.MaxValue;
        public float GetH(AStarPoint point) => storedCosts.TryGetValue(point, out var costs) ? costs.h : float.MaxValue;
        public float GetF(AStarPoint point) => storedCosts.TryGetValue(point, out var costs) ? costs.g + costs.h : float.MaxValue;
    }
}