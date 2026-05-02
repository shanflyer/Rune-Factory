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
    private const string SpriteShaderName = "MySprite-Lit-Default";
    private const string WaterShaderName = "MySprite-Water-Lit-Default";
    private static readonly int FeatureFlagsId = Shader.PropertyToID("_FeatureFlags");

    private static readonly (MySpriteLitFeatureFlags Flag, string Keyword, int PropertyId)[] SpriteKeywordMappings =
    {
        (MySpriteLitFeatureFlags.DAMPBLEND, "_FEATURE_DAMP", Shader.PropertyToID("_FEATURE_DAMP")),
        (MySpriteLitFeatureFlags.MOVE, "_FEATURE_MOVE", Shader.PropertyToID("_FEATURE_MOVE")),
        (MySpriteLitFeatureFlags.SEASONCOLORBLEND, "_FEATURE_SEASON", Shader.PropertyToID("_FEATURE_SEASON")),
        (MySpriteLitFeatureFlags.SNOWBLEND, "_FEATURE_SNOW", Shader.PropertyToID("_FEATURE_SNOW")),
        (MySpriteLitFeatureFlags.GRASSBLEND, "_FEATURE_GRASS", Shader.PropertyToID("_FEATURE_GRASS")),
        (MySpriteLitFeatureFlags.SHADOWSTEP, "_FEATURE_SHADOW", Shader.PropertyToID("_FEATURE_SHADOW")),
        (MySpriteLitFeatureFlags.FLOWERSTEP, "_FEATURE_FLOWER", Shader.PropertyToID("_FEATURE_FLOWER")),
    };

    private static readonly (MySpriteLitFeatureFlags Flag, string Keyword, int PropertyId)[] WaterKeywordMappings =
    {
        (MySpriteLitFeatureFlags.SHADOWSTEP, "_FEATURE_SHADOW", Shader.PropertyToID("_FEATURE_SHADOW")),
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
                if (mat == null || mat.shader == null || !mat.HasProperty(FeatureFlagsId))
                    continue;

                var mappings = mat.shader.name switch
                {
                    SpriteShaderName => SpriteKeywordMappings,
                    WaterShaderName => WaterKeywordMappings,
                    _ => null,
                };

                if (mappings == null)
                    continue;

                totalMatched++;
                EditorUtility.DisplayProgressBar(
                    "Sync MySprite-Lit-Default Keywords",
                    path,
                    (float)(i + 1) / guids.Length);

                if (ApplyKeywordsFromFlags(mat, mappings))
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

    private static bool ApplyKeywordsFromFlags(Material mat, (MySpriteLitFeatureFlags Flag, string Keyword, int PropertyId)[] mappings)
    {
        var flags = (MySpriteLitFeatureFlags)(uint)mat.GetInt(FeatureFlagsId);
        var changed = false;

        foreach (var (flag, keyword, propertyId) in mappings)
        {
            var shouldEnable = (flags & flag) != 0;
            var hasKeyword = mat.IsKeywordEnabled(keyword);
            var propertyValue = mat.HasProperty(propertyId) ? mat.GetFloat(propertyId) : -1f;
            var shouldPropertyValue = shouldEnable ? 1f : 0f;

            if (shouldEnable != hasKeyword)
            {
                if (shouldEnable)
                    mat.EnableKeyword(keyword);
                else
                    mat.DisableKeyword(keyword);

                changed = true;
            }

            if (mat.HasProperty(propertyId) && !Mathf.Approximately(propertyValue, shouldPropertyValue))
            {
                mat.SetFloat(propertyId, shouldPropertyValue);
                changed = true;
            }
        }

        return changed;
    }
}

