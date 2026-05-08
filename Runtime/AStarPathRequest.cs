using System.Collections.Generic;
using UnityEngine;

namespace SimplePathfinding
{
    public class AStarPathRequest
    {
        public Dictionary<AStarPoint, int> storedCosts = new();

        public void SetCosts(AStarPoint point, int g, int h)
        {
            storedCosts[point] = BitPacker.Pack(g, h);
        }

        public int GetG(AStarPoint point) => storedCosts.TryGetValue(point, out int p) ? BitPacker.UnpackG(p) : 0;
        public int GetH(AStarPoint point) => storedCosts.TryGetValue(point, out int p) ? BitPacker.UnpackH(p) : 0;
        public int GetF(AStarPoint point) => storedCosts.TryGetValue(point, out int p) ? BitPacker.UnpackF(p) : 0;
    }
}