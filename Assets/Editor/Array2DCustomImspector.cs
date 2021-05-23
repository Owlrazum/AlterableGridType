using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(Array2D))]
public class SomeScriptEditor : Editor
{
    public override void OnInspectorGUI()
    {
        EditorGUILayout.HelpBox("Due to performace issues, the max value of row and col is 50", MessageType.Info);
        DrawDefaultInspector();
    }
}