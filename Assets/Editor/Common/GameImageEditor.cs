using UnityEditor;
using UnityEditor.UI;
using UnityEngine;

[CustomEditor(typeof(GameImage))]
[CanEditMultipleObjects]
public sealed class GameImageEditor : ImageEditor
{
    SerializedProperty m_GameNullClear;
    SerializedProperty m_GameColorGradient;
    SerializedProperty m_GameColorTL;
    SerializedProperty m_GameColorTR;
    SerializedProperty m_GameColorBL;
    SerializedProperty m_GameColorBR;

    protected override void OnEnable()
    {
        base.OnEnable();
        m_GameNullClear = serializedObject.FindProperty("m_GameNullClear");
        m_GameColorGradient = serializedObject.FindProperty("m_GameColorGradient");
        m_GameColorTL = serializedObject.FindProperty("m_GameColorTL");
        m_GameColorTR = serializedObject.FindProperty("m_GameColorTR");
        m_GameColorBL = serializedObject.FindProperty("m_GameColorBL");
        m_GameColorBR = serializedObject.FindProperty("m_GameColorBR");
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        serializedObject.Update();
        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(m_GameNullClear, new GUIContent("Null Clear"));
        EditorGUILayout.PropertyField(m_GameColorGradient, new GUIContent("Color Gradient"));

        if (m_GameColorGradient.boolValue || m_GameColorGradient.hasMultipleDifferentValues)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(m_GameColorTL, new GUIContent("Top Left"));
            EditorGUILayout.PropertyField(m_GameColorTR, new GUIContent("Top Right"));
            EditorGUILayout.PropertyField(m_GameColorBL, new GUIContent("Bottom Left"));
            EditorGUILayout.PropertyField(m_GameColorBR, new GUIContent("Bottom Right"));
            EditorGUI.indentLevel--;
        }

        serializedObject.ApplyModifiedProperties();
    }
}
