using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SpriteRenderer))]
public class SpriteRendererEditor : Editor
{
    private static readonly BindingFlags BindingFlagsAll =
        BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

    private Editor builtInEditor;

    private void OnEnable()
    {
        CreateBuiltInEditor();
    }

    private void OnDisable()
    {
        if (builtInEditor != null)
        {
            DestroyImmediate(builtInEditor);
            builtInEditor = null;
        }
    }

    public override void OnInspectorGUI()
    {
        DrawBuiltInInspector();

        EditorGUILayout.Space();
        using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
        {
            DrawHdrColorField();
        }
    }

    private void DrawBuiltInInspector()
    {
        if (builtInEditor == null)
        {
            CreateBuiltInEditor();
        }

        if (builtInEditor != null)
        {
            builtInEditor.OnInspectorGUI();
        }
        else
        {
            DrawDefaultInspector();
        }
    }

    private void CreateBuiltInEditor()
    {
        Type editorType = FindBuiltInSpriteRendererEditorType();
        if (editorType != null)
        {
            builtInEditor = CreateEditor(targets, editorType);
        }
    }

    private Type FindBuiltInSpriteRendererEditorType()
    {
        FieldInfo inspectedTypeField = typeof(CustomEditor).GetField("m_InspectedType", BindingFlagsAll);
        if (inspectedTypeField == null)
        {
            return null;
        }

        Type customEditorType = GetType();
        foreach (Type editorType in TypeCache.GetTypesDerivedFrom<Editor>())
        {
            if (editorType == null || editorType == customEditorType || editorType.IsAbstract)
            {
                continue;
            }

            foreach (object attribute in editorType.GetCustomAttributes(typeof(CustomEditor), true))
            {
                Type inspectedType = inspectedTypeField.GetValue(attribute) as Type;
                if (inspectedType == typeof(SpriteRenderer))
                {
                    return editorType;
                }
            }
        }

        return null;
    }

    private void DrawHdrColorField()
    {
        SpriteRenderer renderer = target as SpriteRenderer;
        if (renderer == null)
        {
            return;
        }

        EditorGUI.BeginChangeCheck();
        EditorGUI.showMixedValue = HasMixedColor(renderer.color);
        Color color = EditorGUILayout.ColorField(new GUIContent("HDR Color"), renderer.color, false, true, true);
        EditorGUI.showMixedValue = false;
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObjects(targets, "Change SpriteRenderer HDR Color");
            foreach (UnityEngine.Object obj in targets)
            {
                if (obj is SpriteRenderer item)
                {
                    item.color = color;
                    EditorUtility.SetDirty(item);
                }
            }
        }
    }

    private bool HasMixedColor(Color first)
    {
        foreach (UnityEngine.Object obj in targets)
        {
            if (obj is SpriteRenderer item && item.color != first)
            {
                return true;
            }
        }

        return false;
    }
}
