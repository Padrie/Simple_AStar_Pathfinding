
namespace SimplePathfinding
{
    public class PointSnapshot
    {
        public IAStarPoint point;
        public bool originalWalkable;
        public float originalWeight;
        public bool[] originalTypes;

        public PointSnapshot(IAStarPoint point)
        {
            this.point = point;
            originalWalkable = point.Walkable;
            originalWeight = point.Weight;
            originalTypes = (bool[])point.AllowedAgentTypes.Clone();
        }

        public void Restore()
        {
            point.Walkable = originalWalkable;
            point.Weight = originalWeight;
            point.AllowedAgentTypes = originalTypes;
        }
    }
}