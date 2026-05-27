using UnityEditor;
using UnityEngine;

namespace SimplePathfinding
{
    [CustomEditor(typeof(AStar))]
    public class AStarEditor : Editor
    {
        private bool agentTypesFoldout = true;
        private Vector2 agentTypesScroll;

        public override void OnInspectorGUI()
        {
            AStar astar = (AStar)target;

            base.OnInspectorGUI();

            EditorGUILayout.Space();
            agentTypesFoldout = EditorGUILayout.Foldout(agentTypesFoldout, "Agent Types", true);

            if (Application.isPlaying)
            {
                EditorGUILayout.HelpBox("Agent types cannot be changed at runtime.", MessageType.Info);
                GUI.enabled = false;
            }

            if (agentTypesFoldout)
            {
                agentTypesScroll = EditorGUILayout.BeginScrollView(agentTypesScroll, GUILayout.Height(200));

                for (int i = 0; i < astar.agentTypes.Length; i++)
                {
                    EditorGUILayout.BeginHorizontal();

                    EditorGUILayout.LabelField(i.ToString(), GUILayout.Width(30));
                    astar.agentTypes[i] = EditorGUILayout.TextField(astar.agentTypes[i]);

                    EditorGUILayout.EndHorizontal();
                }

                EditorGUILayout.EndScrollView();
            }

            if (GUI.changed)
            {
                EditorUtility.SetDirty(astar);
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Stored GridPoints: " + astar.gridpointList.Count);
            EditorGUILayout.LabelField("Stored WayPoints: " + astar.waypointList.Count);

            if (GUILayout.Button("Bake"))
            {
                astar.SetupPoints();
            }

            if (GUILayout.Button("Clear Bake"))
            {
                if(EditorUtility.DisplayDialog(
                    "Clear Bake",
                    "Are you sure? This can't be undone and will delete all points and neighbors.",
                    "Clear",
                    "Cancel"))
                {
                    astar.ClearPoints();
                }
            }

            GUI.enabled = true;
        }
    }
}
