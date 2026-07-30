using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(OutOfBound))]
public class OutOfBoundEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(serializedObject.FindProperty("objectiveDialoguePair"), true);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("relatedObjectives"), true);

        SerializedProperty triggerTargetTypeProp = serializedObject.FindProperty("triggerTargetType");
        EditorGUILayout.PropertyField(triggerTargetTypeProp);

        OutOfBound.TriggerTargetType selectedType = (OutOfBound.TriggerTargetType)triggerTargetTypeProp.enumValueIndex;

        if (selectedType == OutOfBound.TriggerTargetType.Tag)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("targetTag"));
        }
        else if (selectedType == OutOfBound.TriggerTargetType.GameObjectName)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("targetObjectName"));
        }

        serializedObject.ApplyModifiedProperties();
    }
}
