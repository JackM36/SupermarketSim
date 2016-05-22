using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(SteeringManager))]
public class SteeringManagerEditor : Editor 
{
    SteeringManager steeringManager;

    void OnEnable()
    {
        steeringManager = (SteeringManager)target;
    }

    public override void OnInspectorGUI()
    {
        GUILayout.Space(10);
        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Steering behaviour", EditorStyles.boldLabel, GUILayout.Width(230));
        EditorGUILayout.LabelField("Priority", EditorStyles.boldLabel,  GUILayout.Width(170));
        GUILayout.EndHorizontal();

        for (int i = 0; i < steeringManager.steeringBehaviours.Count; i++)
        {
            GUILayout.BeginHorizontal();

            // enable
            steeringManager.steeringBehaviours[i].enabled = EditorGUILayout.Toggle(steeringManager.steeringBehaviours[i].enabled, GUILayout.Width(20));
            // behaviour name
            string behaviourName = steeringManager.steeringBehaviours[i].type.ToString();
            EditorGUILayout.LabelField(ObjectNames.NicifyVariableName(behaviourName), EditorStyles.boldLabel, GUILayout.Width(210));
            //priority
            steeringManager.steeringBehaviours[i].priority = EditorGUILayout.Slider(steeringManager.steeringBehaviours[i].priority, 1f, 100f, GUILayout.Width(170));

            GUILayout.EndHorizontal();
        }

        GUILayout.Space(10);
        DrawDefaultInspector();
    }
}
