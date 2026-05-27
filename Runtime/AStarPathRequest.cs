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

        public PriorityQueue<IAStarPoint> openAStarPoints = new PriorityQueue<IAStarPoint>();
        public HashSet<IAStarPoint> openSet = new HashSet<IAStarPoint>();
        public HashSet<IAStarPoint> closedAStarPoints = new HashSet<IAStarPoint>();

        [HideInInspector] public bool[] agentTypes = new bool[30];

        public void RequestPath(IAStarPoint start, IAStarPoint end, Color randomColor, bool[] agentTypes)
        {
            if (start == null || end == null)
            {
                Debug.LogWarning("RequestPath: start or end is null");
                return;
            }

            bool startAllowed = false;
            for (int i = 0; i < 30; i++)
                if (agentTypes[i] && start.AllowedAgentTypes[i]) { startAllowed = true; break; }

            if (!startAllowed)
            {
                Debug.LogWarning("Start point doesn't allow agent type");
                return;
            }

            startPoint = start;
            endPoint = end;
            this.agentTypes = agentTypes;

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
            openAStarPoints.Clear();
            openSet.Clear();
            closedAStarPoints.Clear();
        }

        public float GetG(IAStarPoint point) => storedCosts.TryGetValue(point, out var costs) ? costs.g : float.MaxValue;
        public float GetH(IAStarPoint point) => storedCosts.TryGetValue(point, out var costs) ? costs.h : float.MaxValue;
        public float GetF(IAStarPoint point) => storedCosts.TryGetValue(point, out var costs) ? costs.g + costs.h : float.MaxValue;
        public IAStarPoint GetCameFrom(IAStarPoint point) => storedCosts.TryGetValue(point, out var costs) ? costs.cameFrom : null;
    }
}