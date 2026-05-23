using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace SimplePathfinding
{
    [ExecuteInEditMode]
    public class AStarGrid : MonoBehaviour, ISerializationCallbackReceiver
    {
        public Vector3Int gridSize = new Vector3Int(20, 20, 20);
        public int cellSize = 2;

        public Dictionary<Vector3Int, AStarGridPoint> storedGridPoints = new();
        [SerializeField, HideInInspector] List<Vector3Int> pointPositions = new List<Vector3Int>();

        public bool drawGizmos = true;
        public int gizmoDrawRange = 10;

        [Header("Raycast options")]
        public float rayLength = 100f;
        public float obstacleAvoidanceRadius = 1f;
        public LayerMask obstacleMask;

        Vector3Int pos;
        Vector3Int origin;
        [SerializeField, HideInInspector] int originalCellSize;

        public void OnBeforeSerialize() {}

        public void OnAfterDeserialize()
        {
            for (int i = 0; i < pointPositions.Count; i++)
            {
                AStarGridPoint gridPoint = new AStarGridPoint(pointPositions[i]);
                if (storedGridPoints.ContainsKey(pointPositions[i]))
                    continue;
                storedGridPoints.Add(pointPositions[i], gridPoint);
            }
        }

        private void Update()
        {
            transform.position = Vector3Int.RoundToInt(transform.position);
            pos = Vector3Int.RoundToInt(transform.position);
        }

        [ContextMenu("Spawn Grid Points")]
        private void SpawnGridPoints()
        {
            storedGridPoints.Clear();
            pointPositions.Clear();
            originalCellSize = cellSize;

            origin = pos - new Vector3Int(gridSize.x / 2, gridSize.y / 2, gridSize.z / 2);

            Debug.Log("Origin set to: " + origin);

            for (int x = 0; x < gridSize.x / cellSize; x++)
            {
                for (int z = 0; z < gridSize.z / cellSize; z++)
                {
                    for (int y = 0; y < gridSize.y / cellSize; y++)
                    {
                        Vector3Int pointPos = origin + new Vector3Int(x * cellSize, y * cellSize, z * cellSize);

                        if (Physics.Raycast(pointPos, Vector3.down, out RaycastHit hit, rayLength))
                        {
                            if (Physics.CheckSphere(pointPos, obstacleAvoidanceRadius, obstacleMask) || Physics.CheckSphere(pointPos, .1f))
                                continue;
                            if ((obstacleMask & (1 << hit.collider.gameObject.layer)) != 0)
                                continue;

                            AStarGridPoint gridPoint = new AStarGridPoint(pointPos);
                            storedGridPoints.Add(pointPos, gridPoint);
                            pointPositions.Add(pointPos);
                        }
                    }
                }
            }
        }

        public List<AStarGridPoint> GetGridPoints()
        {
            return storedGridPoints.Values.ToList();
        }

        [ContextMenu("Clear Dictionary")]
        private void ClearDictionary()
        {
            storedGridPoints.Clear();
            pointPositions.Clear();
        }

        [ContextMenu("Print Coords")]
        private void PrintCoords()
        {
            print(storedGridPoints.Count);
        }

        public bool IsAgentInGridVolume(Vector3 pos)
        {
            Bounds gridBounds = new Bounds(transform.position, gridSize);
            return gridBounds.Contains(pos);
        }

        public Vector3Int GetOrigin()
        {
            return Vector3Int.RoundToInt(transform.position) - new Vector3Int(gridSize.x / 2, gridSize.y / 2, gridSize.z / 2);
        }

        private void OnDrawGizmos()
        {
            if (!drawGizmos) return;
            VolumeGizmo();
            if (storedGridPoints.Count == 0) return;
            GridPointGizmoDrawDistance();
        }

        private void VolumeGizmo()
        {
            Gizmos.DrawWireCube(transform.position, gridSize);
        }

        private void GridPointGizmoDrawDistance()
        {
            Camera sceneCam = SceneView.currentDrawingSceneView?.camera;
            if(sceneCam == null) return;
            Vector3Int camPos = Vector3Int.RoundToInt(sceneCam.transform.position);

            int camIndexX = (camPos.x - origin.x) / originalCellSize;
            int camIndexY = (camPos.y - origin.y) / originalCellSize;
            int camIndexZ = (camPos.z - origin.z) / originalCellSize;

            for (int x = camIndexX - gizmoDrawRange; x < camIndexX + gizmoDrawRange; x++)
                for (int z = camIndexZ - gizmoDrawRange; z < camIndexZ + gizmoDrawRange; z++)
                    for (int y = camIndexY - gizmoDrawRange; y < camIndexY + gizmoDrawRange; y++)
                        if (storedGridPoints.TryGetValue(origin + new Vector3Int(x * originalCellSize, y * originalCellSize, z * originalCellSize), out var point))
                        {
                            Gizmos.color = new Color(1, 0, 0, .75f);
                            Gizmos.DrawCube(point.Position, Vector3.one / 5);
                        }
        }
    }
}