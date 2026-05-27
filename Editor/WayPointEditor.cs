using UnityEditor;
using UnityEngine;

namespace SimplePathfinding
{
    [CustomEditor(typeof(WayPoint))]
    public class WayPointEditor : Editor
    {
        private bool allowedTypesFoldout = false;
        private Vector2 allowedTypesScroll;

        AStar astar;

        public override void OnInspectorGUI()
        {
            WayPoint wayPoint = (WayPoint)target;

            base.OnInspectorGUI();

            EditorGUILayout.Space();
            allowedTypesFoldout = EditorGUILayout.Foldout(allowedTypesFoldout, "Allowed Agent Types", true);

            if (Application.isPlaying)
            {
                EditorGUILayout.HelpBox("Agent types cannot be changed at runtime.", MessageType.Info);
                GUI.enabled = false;
            }

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
                    wayPoint.allowedAgentTypes[i] = EditorGUILayout.Toggle(name, wayPoint.allowedAgentTypes[i]);
                }

                EditorGUILayout.EndScrollView();
            }

            if (GUI.changed)
            {
                EditorUtility.SetDirty(wayPoint);
            }

            GUI.enabled = true;
        }
    }
}
