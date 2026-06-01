using UnityEditor;
using UnityEngine;

namespace SimplePathfinding
{
    [CustomEditor(typeof(FilterVolume))]
    public class FilterVolumeEditor : Editor
    {
        private bool typesFoldout = true;
        private Vector2 scroll;
        private AStar astar;

        public override void OnInspectorGUI()
        {

            FilterVolume volume = (FilterVolume)target;
            DrawDefaultInspector();

            if (astar == null) astar = FindAnyObjectByType<AStar>();
            if (astar == null) return;

            EditorGUILayout.Space();
            typesFoldout = EditorGUILayout.Foldout(typesFoldout, "Agent Types", true);

            EditorGUI.BeginChangeCheck();

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
                    volume.agentTypes[i] = EditorGUILayout.Toggle(name, volume.agentTypes[i]);
                }

                EditorGUILayout.EndScrollView();
            }

            if (EditorGUI.EndChangeCheck())
                EditorUtility.SetDirty(volume);
        }
    }
}