using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class SceneViewFPS
{
    static float smoothedFPS = 0;

    static SceneViewFPS()
    {
        SceneView.duringSceneGui += OnSceneGUI;
    }

    static void OnSceneGUI(SceneView sceneView)
    {
        Handles.BeginGUI();
        smoothedFPS = Mathf.Lerp(smoothedFPS, 1f / Time.unscaledDeltaTime, Time.unscaledDeltaTime * 5);
        GUI.Label(new Rect(50, 10, 200, 20), "FPS: " + smoothedFPS.ToString("F1"));
        Handles.EndGUI();
    }
}