using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

[CustomEditor(typeof(MannequinDemoGame))]
public class MannequinDemoGameEditor : Editor
{
    private BoxBoundsHandle boxHandle = new BoxBoundsHandle();

    private void OnEnable()
    {
        boxHandle.axes = PrimitiveBoundsHandle.Axes.All;
        boxHandle.wireframeColor = Color.yellow;
        boxHandle.handleColor = new Color(1f, 1f, 0f, 0.4f);
    }

    private void OnSceneGUI()
    {
        MannequinDemoGame mannequin = (MannequinDemoGame)target;
        Transform transform = mannequin.transform;

        boxHandle.center = transform.position + mannequin.DetectionBoxOffset;
        boxHandle.size = mannequin.DetectionBoxSize;

        EditorGUI.BeginChangeCheck();
        boxHandle.DrawHandle();
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(mannequin, "Edit Detection Box");
            mannequin.DetectionBoxSize = boxHandle.size;
            mannequin.DetectionBoxOffset = boxHandle.center - transform.position;
            EditorUtility.SetDirty(mannequin);
        }
    }
}
