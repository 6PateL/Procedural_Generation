using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEditor;
[CustomEditor(typeof(MapGenerator))]

public class MapGeneratorEditor : Editor 
{
    public override void OnInspectorGUI()
    {
        MapGenerator mapGenerator = (MapGenerator)target;
        
        if (DrawDefaultInspector())
        {
            if (mapGenerator.AutoUpdate)
            {
               mapGenerator.DrawMapInEditor(); 
            }  
        }
        
        if (GUILayout.Button("Generate"))
        {
            mapGenerator.DrawMapInEditor();
        }
    }
}