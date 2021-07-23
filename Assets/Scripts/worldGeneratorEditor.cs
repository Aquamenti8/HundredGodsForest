using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(worldGenerator))]
public class worldGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        worldGenerator worldGen = (worldGenerator)target;
        if(GUILayout.Button("Generate World"))
        {
            worldGen.Generate();
        }
        if (GUILayout.Button("Clear World"))
        {
            worldGen.ClearWorld();
        }
        if (GUILayout.Button("Save Current Map !"))
        {
            worldGen.saveCurrentMap();
        }
    }
}
