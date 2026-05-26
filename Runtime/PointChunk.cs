using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace SimplePathfinding
{
    [Serializable]
    public class PointChunk
    {
        public List<WayPoint> pointList = new();
        public Vector3Int chunkCoord;

        public PointChunk(Vector3Int coord)
        {
            chunkCoord = coord;
        }
    }
}
