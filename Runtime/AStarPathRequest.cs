using System;
using System.Collections.Generic;
using UnityEngine;

namespace SimplePathfinding
{
    public class AStarPathRequest
    {
        public Dictionary<IAStarPoint, (float g,float h, IAStarPoint cameFrom)> storedCosts = new();
        public List<IAStarPoint> aStarPointPath = new();

        public IAStarPoint startPoint;
        public IAStarPoint endPoint;

        public void RequestPath(IAStarPoint start, IAStarPoint end, Color randomColor)
        {
            startPoint = start;
            endPoint = end;

            AStar.Instance.CalculatePath(this, randomColor);
        }

        public void SetCosts(IAStarPoint point, float g, float h, IAStarPoint from)
        {
            storedCosts[point] = (g,h, from);
        }

        public void ClearCosts()
        {
            storedCosts.Clear();
            aStarPointPath.Clear();
        }

        public float GetG(IAStarPoint point) => storedCosts.TryGetValue(point, out var costs) ? costs.g : float.MaxValue;
        public float GetH(IAStarPoint point) => storedCosts.TryGetValue(point, out var costs) ? costs.h : float.MaxValue;
        public float GetF(IAStarPoint point) => storedCosts.TryGetValue(point, out var costs) ? costs.g + costs.h : float.MaxValue;
        public IAStarPoint GetCameFrom(IAStarPoint point) => storedCosts.TryGetValue(point, out var costs) ? costs.cameFrom : null;
    }
}