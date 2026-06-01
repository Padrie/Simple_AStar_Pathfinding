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

        public Dictionary<Vector3Int, GridPoint> storedGridPoints = new();
        [SerializeField, HideInInspector] List<Vector3Int> pointPositions = new();

        public bool drawGizmos = true;
        public int gizmoDrawRange = 10;

        [Header("Raycast options")]
        public float rayLength = 100f;
        public float obstacleAvoidanceRadius = 1f;
        public LayerMask obstacleMask;

        Vector3Int pos;
        Vector3Int origin;
        [SerializeField, HideInInspector] int originalCellSize;
        [HideInInspector] public bool[] allowedAgentTypes = new bool[30];

        public void OnBeforeSerialize() {}

        public void OnAfterDeserialize()
        {
            for (int i = 0; i < pointPositions.Count; i++)
            {
                GridPoint gridPoint = new GridPoint(pointPositions[i]);
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

        public void SpawnGridPoints()
        {
            storedGridPoints.Clear();
            pointPositions.Clear();
            originalCellSize = cellSize;

            origin = pos - new Vector3Int(gridSize.x / 2, gridSize.y / 2, gridSize.z / 2);

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

                            GridPoint gridPoint = new GridPoint(pointPos);
                            gridPoint.allowedAgentTypes = (bool[])allowedAgentTypes.Clone();
                            storedGridPoints.Add(pointPos, gridPoint);
                            pointPositions.Add(pointPos);
                        }
                    }
                }
            }
        }

        public List<GridPoint> GetGridPoints()
        {
            return storedGridPoints.Values.ToList();
        }

        public void ClearDictionary()
        {
            storedGridPoints.Clear();
            pointPositions.Clear();
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