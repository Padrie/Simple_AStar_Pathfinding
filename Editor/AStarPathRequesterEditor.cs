using UnityEditor;
using UnityEngine;

namespace SimplePathfinding
{
    [CustomEditor(typeof(AStarPathRequester))]
    public class AStarPathRequesterEditor : Editor
    {
        private bool typesFoldout = true;
        private Vector2 scroll;
        private AStar astar;

        public override void OnInspectorGUI()
        {

            AStarPathRequester requester = (AStarPathRequester)target;
            DrawDefaultInspector();

            if (astar == null) astar = FindFirstObjectByType<AStar>();
            if (astar == null) return;

            EditorGUILayout.Space();
            typesFoldout = EditorGUILayout.Foldout(typesFoldout, "Agent Types", true);

            if (Application.isPlaying)
            {
                EditorGUILayout.HelpBox("Agent types cannot be changed at runtime.", MessageType.Info);
                GUI.enabled = false;
            }

            if (typesFoldout)
            {
                int visibleCount = 0;
                foreach (var n in astar.agentTypes)
                    if (!string.IsNullOrEmpty(n)) visibleCount++;
                int height = Mathf.Min(visibleCount * 20 + 10, 200);

                scroll = EditorGUILayout.BeginScrollView(scroll, GUILayout.Height(height));

                for (int i = 0; i < astar.agentTypes.Length; i++)
                {
                    string name = astar.agentTypes[i];
                    if (string.IsNullOrEmpty(name)) continue;
                    requester.agentTypes[i] = EditorGUILayout.Toggle(name, requester.agentTypes[i]);
                }

                EditorGUILayout.EndScrollView();
            }

            if (GUI.changed) EditorUtility.SetDirty(requester);
            //GUI.enabled = true;
        }
    }
}