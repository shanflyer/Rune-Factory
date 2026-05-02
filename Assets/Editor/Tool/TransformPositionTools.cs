using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class TransformPositionTools
{
    [MenuItem("GameObject/工具/位置/Z等于Y", false, 0)]
    private static void SetSelectedTransformPositionZToY()
    {
        Transform[] selectedTransforms = Selection.transforms;
        if (selectedTransforms == null || selectedTransforms.Length == 0)
        {
            return;
        }

        List<Object> undoObjects = new List<Object>(selectedTransforms.Length);
        for (int i = 0; i < selectedTransforms.Length; i++)
        {
            if (selectedTransforms[i] != null)
            {
                undoObjects.Add(selectedTransforms[i]);
            }
        }

        if (undoObjects.Count == 0)
        {
            return;
        }

        Undo.RecordObjects(undoObjects.ToArray(), "位置 Z 等于 Y");

        for (int i = 0; i < selectedTransforms.Length; i++)
        {
            Transform current = selectedTransforms[i];
            if (current == null)
            {
                continue;
            }

            Vector3 position = current.position;
            position.z = position.y;
            current.position = position;
            EditorUtility.SetDirty(current);
        }
    }

    [MenuItem("GameObject/工具/位置/Z等于Y", true)]
    private static bool ValidateSetSelectedTransformPositionZToY()
    {
        return Selection.transforms != null && Selection.transforms.Length > 0;
    }

    [MenuItem("CONTEXT/Transform/位置/Z等于Y")]
    private static void SetContextTransformPositionZToY(MenuCommand command)
    {
        if (command.context is not Transform current)
        {
            return;
        }

        Undo.RecordObject(current, "位置 Z 等于 Y");

        Vector3 position = current.position;
        position.z = position.y;
        current.position = position;
        EditorUtility.SetDirty(current);
    }
}
