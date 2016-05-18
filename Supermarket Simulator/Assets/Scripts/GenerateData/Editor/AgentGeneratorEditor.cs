using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(AgentGenerator))]
public class AgentGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        if (GUILayout.Button("Generate Data"))
        {
            AgentGenerator gen = (AgentGenerator)target;
            gen.generate();
        }
    }
}
