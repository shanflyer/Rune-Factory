using System;
using UnityEditor;
using UnityEngine;

[Flags]
internal enum MySpriteLitFeatureFlags : uint
{
    WATER = 1u << 0,
    DAMPBLEND = 1u << 1,
    MOVE = 1u << 2,
    SEASONCOLORBLEND = 1u << 3,
    SNOWBLEND = 1u << 4,
    GRASSBLEND = 1u << 5,
    SHADOWSTEP = 1u << 6,
    FLOWERSTEP = 1u << 7,
    SIMPLE = 1u << 8,
    CHARACTER = 1u << 9,
}

internal static class SyncMySpriteLitKeywordsTool
{
    private const string ShaderName = "MySprite-Lit-Default";
    private static readonly int FeatureFlagsId = Shader.PropertyToID("_FeatureFlags");

    private static readonly (MySpriteLitFeatureFlags Flag, string Keyword)[] KeywordMappings =
    {
        (MySpriteLitFeatureFlags.WATER, "_FEATURE_WATER"),
        (MySpriteLitFeatureFlags.DAMPBLEND, "_FEATURE_DAMP"),
        (MySpriteLitFeatureFlags.MOVE, "_FEATURE_MOVE"),
        (MySpriteLitFeatureFlags.SEASONCOLORBLEND, "_FEATURE_SEASON"),
        (MySpriteLitFeatureFlags.SNOWBLEND, "_FEATURE_SNOW"),
        (MySpriteLitFeatureFlags.GRASSBLEND, "_FEATURE_GRASS"),
        (MySpriteLitFeatureFlags.SHADOWSTEP, "_FEATURE_SHADOW"),
        (MySpriteLitFeatureFlags.FLOWERSTEP, "_FEATURE_FLOWER"),
        (MySpriteLitFeatureFlags.SIMPLE, "_FEATURE_SIMPLE"),
        (MySpriteLitFeatureFlags.CHARACTER, "_FEATURE_CHARACTER"),
    };

    [MenuItem("Tools/Shader/Sync MySprite-Lit-Default Keywords")]
    private static void SyncAllMaterials()
    {
        var guids = AssetDatabase.FindAssets("t:Material");
        var totalMatched = 0;
        var totalChanged = 0;

        try
        {
            for (var i = 0; i < guids.Length; i++)
            {
                var path = AssetDatabase.GUIDToAssetPath(guids[i]);
                var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (mat == null || mat.shader == null || mat.shader.name != ShaderName || !mat.HasProperty(FeatureFlagsId))
                    continue;

                totalMatched++;
                EditorUtility.DisplayProgressBar(
                    "Sync MySprite-Lit-Default Keywords",
                    path,
                    (float)(i + 1) / guids.Length);

                if (ApplyKeywordsFromFlags(mat))
                {
                    totalChanged++;
                    EditorUtility.SetDirty(mat);
                }
            }
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog(
            "Sync MySprite-Lit-Default Keywords",
            $"Matched materials: {totalMatched}\nUpdated materials: {totalChanged}",
            "OK");
    }

    private static bool ApplyKeywordsFromFlags(Material mat)
    {
        var flags = (MySpriteLitFeatureFlags)(uint)mat.GetInt(FeatureFlagsId);
        var changed = false;

        foreach (var (flag, keyword) in KeywordMappings)
        {
            var shouldEnable = (flags & flag) != 0;
            var hasKeyword = mat.IsKeywordEnabled(keyword);

            if (shouldEnable == hasKeyword)
                continue;

            if (shouldEnable)
                mat.EnableKeyword(keyword);
            else
                mat.DisableKeyword(keyword);

            changed = true;
        }

        return changed;
    }
}
