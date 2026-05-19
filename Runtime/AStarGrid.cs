using UnityEngine;

namespace SimplePathfinding
{
    public class AStarGrid : MonoBehaviour
    {
        Vector3Int size = new Vector3Int(20,20,10);

        private void Start()
        {
            for (int x = 0; x < size.x; x++)
            {
                for (int z = 0; z < size.z; z++)
                {
                    for (int y = 0; y < size.y; y++)
                    {
                        GameObject a = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        a.transform.position = new Vector3(x, y, z);
                    }
                }
            }
        }
    }
}