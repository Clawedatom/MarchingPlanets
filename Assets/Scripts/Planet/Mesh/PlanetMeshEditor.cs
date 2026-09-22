using UnityEditor;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

[CustomEditor(typeof(PlanetMeshGenerator))]
public class PlanetMeshEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        PlanetMeshGenerator generator = (PlanetMeshGenerator)target;

        GUILayout.Space(15);
        EditorGUILayout.LabelField("Planet Data", EditorStyles.boldLabel);
        using (new EditorGUI.DisabledScope(!generator.HasPlanetData))
        {
            if (GUILayout.Button("Apply Assigned Planet Data"))
            {
                Undo.RecordObject(generator, "Apply Planet Data");
                generator.ApplyPlanetData();
                EditorUtility.SetDirty(generator);
            }
        }

        if (!generator.HasPlanetData)
        {
            EditorGUILayout.HelpBox(
                "Assign a PlanetData asset above to save and reuse this planet's generation recipe.",
                MessageType.Info);
        }

        GUILayout.Space(15);
        EditorGUILayout.LabelField("Randomization Controls", EditorStyles.boldLabel);

        // Individual Randomize Buttons
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Randomize Noise"))
        {
            Undo.RecordObject(generator, "Randomize Planet Noise");
            generator.RandomizeNoise();
            EditorUtility.SetDirty(generator);
            generator.GeneratePlanetMesh();

        }

        if (GUILayout.Button("Randomize Gradient"))
        {
            Undo.RecordObject(generator, "Randomize Planet Gradient");
            generator.RandomizeGradient();
            EditorUtility.SetDirty(generator);
            generator.GeneratePlanetMesh();

        }
        if (GUILayout.Button("Randomize Ocean Colour"))
        {
            Undo.RecordObject(generator, "Randomize Planet Ocean Colour");
            generator.RandomizeOceanColour();
            EditorUtility.SetDirty(generator);

        }
        if (GUILayout.Button("Randomize Sea Level"))
        {
            Undo.RecordObject(generator, "Randomize Planet Sea Level");
            generator.RandomizeSeaLevel();
            EditorUtility.SetDirty(generator);

        }
        GUILayout.EndHorizontal();

        // One-click All
        if (GUILayout.Button("Randomize All (Seed, Noise, Gradient)"))
        {
            Undo.RecordObject(generator, "Randomize All Planet Settings");
            generator.RandomizeAll();
            EditorUtility.SetDirty(generator);
            generator.GeneratePlanetMesh();
        }

        GUILayout.Space(15);
        EditorGUILayout.LabelField("Generation Controls", EditorStyles.boldLabel);

        // Main Action Buttons
        if (GUILayout.Button("Generate Planet", GUILayout.Height(35)))
        {
            Undo.RegisterFullObjectHierarchyUndo(generator.gameObject, "Generate Planet");
            generator.GeneratePlanetMesh();
            EditorUtility.SetDirty(generator);
        }

        if (GUILayout.Button("Clear Chunks", GUILayout.Height(25)))
        {
            Undo.RegisterFullObjectHierarchyUndo(generator.gameObject, "Clear Planet");
            generator.ClearExistingChunks();
            EditorUtility.SetDirty(generator);
        }
    }
}
