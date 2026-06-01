using UnityEditor;
using UnityEngine;

namespace SimplePathfinding
{
    [InitializeOnLoad]
    public static class SceneViewGUI
    {
        static AStar aStar;

        static SceneViewGUI()
        {
            SceneView.duringSceneGui += OnSceneGUI;
        }

        static void OnSceneGUI(SceneView sceneView)
        {
            if (aStar == null)
                aStar = Object.FindAnyObjectByType<AStar>();

            if (aStar == null || !aStar.drawChunkGizmos) return;

            Handles.color = new Color(0f, 1f, 1f, 0.3f);
            foreach (var chunk in aStar.waypointChunks)
            {
                Vector3 center = new Vector3(
                    chunk.Key.x * aStar.chunkSize + aStar.chunkSize / 2f,
                    chunk.Key.y * aStar.chunkSize + aStar.chunkSize / 2f,
                    chunk.Key.z * aStar.chunkSize + aStar.chunkSize / 2f);
                Handles.DrawWireCube(center, Vector3.one * aStar.chunkSize);
            }
        }
    }
}