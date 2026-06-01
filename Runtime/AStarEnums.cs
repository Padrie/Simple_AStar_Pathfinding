using UnityEngine;

namespace SimplePathfinding
{
    public enum PathFindingStyle
    {
        Weight1,
        Weight2,
        Weight3
    }

    public enum FilterVolumeMode
    {
        Once,
        Realtime,
        Manual
    }

    public enum PathRequestMode
    {
        Once,
        Realtime,
        Manual
    }
}