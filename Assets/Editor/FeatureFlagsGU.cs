// Assets/Editor/FeatureFlagsGUI.cs

using System;
using UnityEditor;
using UnityEngine;

// 你的 8 个功能位（名称随你改）
[Flags]
public enum FeatureFlags : uint
{
    WATER = 1u << 0,
    DAMPBLEND = 1u << 1,
    MOVE = 1u << 2,
    SEASONCOLORBLEND = 1u << 3,
    SNOWBLEND = 1u << 4,
    GRASSBLEND = 1u << 5,
    SHADOWSTEP = 1u << 6,
    FLOWERSTEP = 1u << 7,
    SIMAPLE = 1u << 8
}

public sealed class FeatureFlagsGUI : ShaderGUI
{
    private static readonly int _FeatureFlagsID = Shader.PropertyToID("_FeatureFlags");

    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] props)
    {
        // 画一个分割
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Feature Flags", EditorStyles.boldLabel);

        // 支持多选
        foreach (var obj in materialEditor.targets)
        {
            var mat = (Material)obj;
            var cur = mat.HasProperty(_FeatureFlagsID) ? mat.GetInt(_FeatureFlagsID) : 0;

            // 用可移植的 FlagsField（Unity 2021+ 有 EditorGUI.EnumFlagsField）
            var flags = (FeatureFlags)(uint)cur;
            flags = (FeatureFlags)EditorGUILayout.EnumFlagsField(flags);

            var newValue = (int)(uint)flags;
            if (newValue != cur)
            {
                Undo.RecordObject(mat, "Change Feature Flags");
                mat.SetInt(_FeatureFlagsID, newValue);
                EditorUtility.SetDirty(mat);
            }

            break; // 我们只需要画一次控件；上面写回会作用到所有选中材质
        }

        // 先画默认属性（贴图、颜色等）
        base.OnGUI(materialEditor, props);
    }
}