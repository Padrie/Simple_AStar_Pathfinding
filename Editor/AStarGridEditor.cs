using UnityEditor;
using UnityEngine;

namespace SimplePathfinding
{
    [CustomEditor(typeof(AStarGrid))]
    public class AStarGridEditor : Editor
    {
        private bool allowedTypesFoldout = false;
        private Vector2 allowedTypesScroll;

        AStar astar;

        public override void OnInspectorGUI()
        {
            AStarGrid aStarGrid = (AStarGrid)target;

            base.OnInspectorGUI();

            EditorGUILayout.Space();
            allowedTypesFoldout = EditorGUILayout.Foldout(allowedTypesFoldout, "Allowed Agent Types", true);

            if (allowedTypesFoldout)
            {
                AStar astar = FindAnyObjectByType<AStar>();
                if (astar == null)
                {
                    EditorGUILayout.HelpBox("No AStar instance found in scene", MessageType.Warning);
                    return;
                }

                int visibleCount = 0;
                foreach (var name in astar.agentTypes)
                    if (!string.IsNullOrEmpty(name)) visibleCount++;

                int height = Mathf.Min(visibleCount * 20 + 10, 200);
                allowedTypesScroll = EditorGUILayout.BeginScrollView(allowedTypesScroll, GUILayout.Height(height));

                for (int i = 0; i < astar.agentTypes.Length; i++)
                {
                    string name = astar.agentTypes[i];
                    if (string.IsNullOrEmpty(name)) continue;
                    aStarGrid.allowedAgentTypes[i] = EditorGUILayout.Toggle(name, aStarGrid.allowedAgentTypes[i]);
                }

                EditorGUILayout.EndScrollView();
            }

            if (GUI.changed)
            {
                EditorUtility.SetDirty(aStarGrid);
            }

            EditorGUILayout.LabelField("Stored Points: " + aStarGrid.storedGridPoints.Count);

            if (GUILayout.Button("Spawn Grid Points"))
            {
                aStarGrid.SpawnGridPoints();
            }

            if (GUILayout.Button("Clear Grid"))
            {
                if (EditorUtility.DisplayDialog(
                    "Clear Grid",
                    "Are you sure? This will delete all grid points.",
                    "Clear",
                    "Cancel"))
                {
                    aStarGrid.ClearDictionary();
                }
            }
        }
    }
}