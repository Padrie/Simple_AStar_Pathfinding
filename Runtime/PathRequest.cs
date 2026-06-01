using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace SimplePathfinding
{
    public class PathRequest
    {
        internal Dictionary<IAStarPoint, (float g,float h, IAStarPoint cameFrom)> storedCosts = new();
        internal List<IAStarPoint> pointPath = new();

        internal IAStarPoint startPoint;
        internal IAStarPoint endPoint;

        internal PriorityQueue<IAStarPoint> openPoints = new PriorityQueue<IAStarPoint>();
        internal HashSet<IAStarPoint> openSet = new HashSet<IAStarPoint>();
        internal HashSet<IAStarPoint> closedPoints = new HashSet<IAStarPoint>();

        internal bool[] agentTypes = new bool[30];

        public async Task RequestPathAsync(IAStarPoint start, IAStarPoint end, bool[] types)
        {
            if (start == null || end == null) return;

            bool startAllowed = false;
            for (int i = 0; i < 30; i++)
                if (types[i] && start.AllowedAgentTypes[i]) { startAllowed = true; break; }

            if (!startAllowed) return;
            
            startPoint = start;
            endPoint = end;
            agentTypes = types;

            await Task.Run(() => AStar.Instance.CalculatePath(this));
        }

        public void SetCosts(IAStarPoint point, float g, float h, IAStarPoint from)
        {
            storedCosts[point] = (g,h, from);
        }

        public void ClearCosts()
        {
            storedCosts.Clear();
            pointPath.Clear();
            openPoints.Clear();
            openSet.Clear();
            closedPoints.Clear();
        }

        public float GetG(IAStarPoint point) => storedCosts.TryGetValue(point, out var costs) ? costs.g : float.MaxValue;
        public float GetH(IAStarPoint point) => storedCosts.TryGetValue(point, out var costs) ? costs.h : float.MaxValue;
        public float GetF(IAStarPoint point) => storedCosts.TryGetValue(point, out var costs) ? costs.g + costs.h : float.MaxValue;
        public IAStarPoint GetCameFrom(IAStarPoint point) => storedCosts.TryGetValue(point, out var costs) ? costs.cameFrom : null;
    }
}