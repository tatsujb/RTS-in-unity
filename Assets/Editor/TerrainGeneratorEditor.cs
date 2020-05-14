using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TerrainGenerator))]

public class TerrainGeneratorEditor : Editor
{

    public override void OnInspectorGUI()
    {
        TerrainGenerator ter = (TerrainGenerator) target;
        base.OnInspectorGUI();

        if (GUILayout.Button("Generate Terrain"))
        {
            ter.Generate(true);
        }
    }   
}
