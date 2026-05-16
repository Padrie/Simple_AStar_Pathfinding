using System;
using System.Collections.Generic;
using UnityEngine;

namespace SimplePathfinding
{
    public class AStarPathRequest
    {
        public Dictionary<AStarPoint, (float g,float h, AStarPoint cameFrom)> storedCosts = new();
        public List<AStarPoint> aStarPointPath = new();

        public AStarPoint startPoint;
        public AStarPoint endPoint;

        public void RequestPath(AStarPoint start, AStarPoint end, Color randomColor)
        {
            startPoint = start;
            endPoint = end;

            AStar.Instance.CalculatePath(this, randomColor);
        }

        public void SetCosts(AStarPoint point, float g, float h, AStarPoint from)
        {
            storedCosts[point] = (g,h, from);
        }

        public void ClearCosts()
        {
            storedCosts.Clear();
            aStarPointPath.Clear();
        }

        public float GetG(AStarPoint point) => storedCosts.TryGetValue(point, out var costs) ? costs.g : float.MaxValue;
        public float GetH(AStarPoint point) => storedCosts.TryGetValue(point, out var costs) ? costs.h : float.MaxValue;
        public float GetF(AStarPoint point) => storedCosts.TryGetValue(point, out var costs) ? costs.g + costs.h : float.MaxValue;
        public AStarPoint GetCameFrom(AStarPoint point) => storedCosts.TryGetValue(point, out var costs) ? costs.cameFrom : null;
    }
}