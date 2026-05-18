using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

internal sealed class FXShaderMigrationTool : EditorWindow
{
    private enum ScanScope
    {
        EffectPrefabs,
        Selection,
        AllMaterials,
    }

    private sealed class PreviewItem
    {
        public string Path;
        public string SourceShader;
        public string TargetShader;
        public string RecipeName;
        public string Note;
        public bool Convertible;
    }

    private sealed class TextureValue
    {
        public Texture Texture;
        public Vector2 Scale;
        public Vector2 Offset;
    }

    private sealed class MaterialPropertyBag
    {
        public readonly Dictionary<string, float> Floats = new Dictionary<string, float>(StringComparer.Ordinal);
        public readonly Dictionary<string, Color> Colors = new Dictionary<string, Color>(StringComparer.Ordinal);
        public readonly Dictionary<string, Vector4> Vectors = new Dictionary<string, Vector4>(StringComparer.Ordinal);
        public readonly Dictionary<string, TextureValue> Textures = new Dictionary<string, TextureValue>(StringComparer.Ordinal);

        public string ShaderName;
        public int RenderQueue;

        public static MaterialPropertyBag Capture(Material material)
        {
            var bag = new MaterialPropertyBag
            {
                ShaderName = material.shader != null ? material.shader.name : "<null>",
                RenderQueue = material.renderQueue,
            };

            var shader = material.shader;
            if (shader == null)
                return bag;

            var propertyCount = ShaderUtil.GetPropertyCount(shader);
            for (var i = 0; i < propertyCount; i++)
            {
                var propertyName = ShaderUtil.GetPropertyName(shader, i);
                var propertyType = ShaderUtil.GetPropertyType(shader, i);

                switch (propertyType)
                {
                    case ShaderUtil.ShaderPropertyType.Color:
                    {
                        var value = material.GetColor(propertyName);
                        bag.Colors[propertyName] = value;
                        bag.Vectors[propertyName] = value;
                        break;
                    }
                    case ShaderUtil.ShaderPropertyType.Vector:
                        bag.Vectors[propertyName] = material.GetVector(propertyName);
                        break;
                    case ShaderUtil.ShaderPropertyType.Float:
                    case ShaderUtil.ShaderPropertyType.Range:
                        bag.Floats[propertyName] = material.GetFloat(propertyName);
                        break;
                    case ShaderUtil.ShaderPropertyType.TexEnv:
                        bag.Textures[propertyName] = new TextureValue
                        {
                            Texture = material.GetTexture(propertyName),
                            Scale = material.GetTextureScale(propertyName),
                            Offset = material.GetTextureOffset(propertyName),
                        };
                        break;
                }
            }

            return bag;
        }

        public bool TryGetFloat(out float value, params string[] names)
        {
            foreach (var name in names)
            {
                if (string.IsNullOrEmpty(name))
                    continue;

                if (Floats.TryGetValue(name, out value))
                    return true;
            }

            value = default;
            return false;
        }

        public bool TryGetColor(out Color value, params string[] names)
        {
            foreach (var name in names)
            {
                if (string.IsNullOrEmpty(name))
                    continue;

                if (Colors.TryGetValue(name, out value))
                    return true;
            }

            value = default;
            return false;
        }

        public bool TryGetVector(out Vector4 value, params string[] names)
        {
            foreach (var name in names)
            {
                if (string.IsNullOrEmpty(name))
                    continue;

                if (Vectors.TryGetValue(name, out value))
                    return true;
            }

            value = default;
            return false;
        }

        public bool TryGetTexture(out TextureValue value, params string[] names)
        {
            foreach (var name in names)
            {
                if (string.IsNullOrEmpty(name))
                    continue;

                if (Textures.TryGetValue(name, out value))
                    return true;
            }

            value = null;
            return false;
        }
    }

    private sealed class RenderStatePreset
    {
        public float SrcBlend;
        public float DstBlend;
        public float SrcBlendAlpha;
        public float DstBlendAlpha;
        public float Cull;
        public float ZWrite;
        public float ZTest;
        public bool UseLegacyBlend2;
    }

    private sealed class MigrationRecipe
    {
        public string Name;
        public string TargetShader;
        public string[] SourceShaders;
        public RenderStatePreset RenderState;
        public Action<MaterialPropertyBag, Material> Apply;
    }

    private static readonly RenderStatePreset AdditivePreset = new RenderStatePreset
    {
        SrcBlend = 5f,
        DstBlend = 1f,
        SrcBlendAlpha = 1f,
        DstBlendAlpha = 10f,
        Cull = 0f,
        ZWrite = 0f,
        ZTest = 4f,
    };

    private static readonly RenderStatePreset AlphaPreset = new RenderStatePreset
    {
        SrcBlend = 5f,
        DstBlend = 10f,
        SrcBlendAlpha = 1f,
        DstBlendAlpha = 10f,
        Cull = 0f,
        ZWrite = 0f,
        ZTest = 4f,
    };

    private static readonly RenderStatePreset LegacyCenterGlowAddPreset = new RenderStatePreset
    {
        SrcBlend = 1f,
        DstBlend = 1f,
        SrcBlendAlpha = 1f,
        DstBlendAlpha = 10f,
        Cull = 0f,
        ZWrite = 0f,
        ZTest = 4f,
        UseLegacyBlend2 = true,
    };

    private static readonly MigrationRecipe[] Recipes =
    {
        new MigrationRecipe
        {
            Name = "Sprite Simple Add",
            TargetShader = "Project/FX/FX_SpriteCore_URP",
            SourceShaders = new[] { "SampleEffectAdd" },
            RenderState = AdditivePreset,
            Apply = ApplySpriteSimpleAdd,
        },
        new MigrationRecipe
        {
            Name = "Sprite Simple Alpha",
            TargetShader = "Project/FX/FX_SpriteCore_URP",
            SourceShaders = new[] { "SampleEffectMul", "SampleEffectMulCloud", "Universal Render Pipeline/Particles/Unlit", "Cartoon FX/Remaster/Particle Ubershader" },
            RenderState = AlphaPreset,
            Apply = ApplySpriteSimpleAlpha,
        },
        new MigrationRecipe
        {
            Name = "Sprite Center Glow Add",
            TargetShader = "Project/FX/FX_SpriteCore_URP",
            SourceShaders = new[] { "Hovl/Particles/Add_CenterGlow", "Shader Graphs/URP_Add_CG" },
            RenderState = LegacyCenterGlowAddPreset,
            Apply = ApplySpriteCenterGlow,
        },
        new MigrationRecipe
        {
            Name = "Sprite Center Glow Alpha",
            TargetShader = "Project/FX/FX_SpriteCore_URP",
            SourceShaders = new[] { "Hovl/Particles/Blend_CenterGlow", "Shader Graphs/URP_Blend_CG", "Shader Graphs/URP_Blend_CG_BlendDepth", "Unlit/MyURP_Blend_CG" },
            RenderState = AlphaPreset,
            Apply = ApplySpriteCenterGlow,
        },
        new MigrationRecipe
        {
            Name = "Sprite Trail",
            TargetShader = "Project/FX/FX_SpriteCore_URP",
            SourceShaders = new[] { "Hovl/Particles/AddTrail" },
            RenderState = AlphaPreset,
            Apply = ApplySpriteTrail,
        },
        new MigrationRecipe
        {
            Name = "Sprite Line Path",
            TargetShader = "Project/FX/FX_SpriteCore_URP",
            SourceShaders = new[] { "Hovl/Particles/Blend_LinePath", "Hovl/Particles/Scroll", "Shader Graphs/URP_SwordSlash", "Shader Graphs/URP_LightGlow" },
            RenderState = AlphaPreset,
            Apply = ApplySpriteLinePath,
        },
        new MigrationRecipe
        {
            Name = "Sprite Fire",
            TargetShader = "Project/FX/FX_SpriteCore_URP",
            SourceShaders = new[] { "Hovl/Particles/Fire" },
            RenderState = AlphaPreset,
            Apply = ApplySpriteFire,
        },
        new MigrationRecipe
        {
            Name = "Sprite Lightning",
            TargetShader = "Project/FX/FX_SpriteCore_URP",
            SourceShaders = new[] { "Hovl/Particles/Lightning" },
            RenderState = AlphaPreset,
            Apply = ApplySpriteLightning,
        },
        new MigrationRecipe
        {
            Name = "Sprite Ice/Fresnel",
            TargetShader = "Project/FX/FX_SpriteCore_URP",
            SourceShaders = new[] { "Hovl/Particles/Add_Fresnel", "Shader Graphs/URP_Ice" },
            RenderState = AlphaPreset,
            Apply = ApplySpriteIce,
        },
        new MigrationRecipe
        {
            Name = "Distortion Only",
            TargetShader = "Project/FX/FX_Distortion_URP",
            SourceShaders = new[] { "Hovl/Particles/Distortion", "Shader Graphs/URP_Distortion" },
            RenderState = AlphaPreset,
            Apply = ApplyDistortionOnly,
        },
        new MigrationRecipe
        {
            Name = "Distortion Overlay",
            TargetShader = "Project/FX/FX_Distortion_URP",
            SourceShaders = new[] { "Shader Graphs/URP_BlendDistort" },
            RenderState = AlphaPreset,
            Apply = ApplyDistortionOverlay,
        },
        new MigrationRecipe
        {
            Name = "Dissolve",
            TargetShader = "Project/FX/FX_SpriteCore_URP",
            SourceShaders = new[] { "Hovl/Particles/DissolveNoise", "Shader Graphs/Fx_ParticleDissolve_apb", "Shader Graphs/Fx_RockDissolve", "Project/FX/FX_Dissolve_URP" },
            RenderState = AlphaPreset,
            Apply = ApplyDissolve,
        },
        new MigrationRecipe
        {
            Name = "Soft Noise",
            TargetShader = "Project/FX/FX_SpriteCore_URP",
            SourceShaders = new[] { "Shader Graphs/URP_SoftNoise" },
            RenderState = AlphaPreset,
            Apply = ApplySoftNoise,
        },
        new MigrationRecipe
        {
            Name = "Shockwave",
            TargetShader = "Project/FX/FX_SpriteCore_URP",
            SourceShaders = new[] { "Hovl/Particles/ShockWave", "Shader Graphs/URP_ShockWave", "Project/FX/FX_Shockwave_URP" },
            RenderState = AlphaPreset,
            Apply = ApplyShockwave,
        },
        new MigrationRecipe
        {
            Name = "TwoSided",
            TargetShader = "Project/FX/FX_TwoSided_URP",
            SourceShaders = new[] { "Shader Graphs/URP_Blend_TwoSides" },
            RenderState = AlphaPreset,
            Apply = ApplyTwoSided,
        },
        new MigrationRecipe
        {
            Name = "BuiltIn Particle Simple",
            TargetShader = "Project/FX/FX_SpriteCore_URP",
            SourceShaders = Array.Empty<string>(),
            RenderState = AdditivePreset,
            Apply = ApplyBuiltInParticleSimple,
        },
    };

    private ScanScope _scanScope = ScanScope.EffectPrefabs;
    private string _rootText = "Assets/Resources/Prefabs/Effect";
    private Vector2 _scroll;
    private readonly List<PreviewItem> _previewItems = new List<PreviewItem>();

    [MenuItem("Tools/Shader/FX Shader Migration Tool")]
    private static void OpenWindow()
    {
        var window = GetWindow<FXShaderMigrationTool>("FX Shader Migration");
        window.minSize = new Vector2(900f, 500f);
    }

    private void OnGUI()
    {
        EditorGUILayout.Space();
        _scanScope = (ScanScope)EditorGUILayout.EnumPopup("Scan Scope", _scanScope);
        using (new EditorGUI.DisabledScope(_scanScope != ScanScope.EffectPrefabs))
        {
            _rootText = EditorGUILayout.TextField("Effect Roots", _rootText);
        }

        EditorGUILayout.Space();
        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Scan", GUILayout.Height(30f)))
            {
                ScanMaterials();
            }

            using (new EditorGUI.DisabledScope(_previewItems.Count == 0 || _previewItems.All(item => !item.Convertible)))
            {
                if (GUILayout.Button("Replace Previewed Materials", GUILayout.Height(30f)))
                {
                    ReplacePreviewedMaterials();
                }
            }
        }

        EditorGUILayout.Space();
        DrawSummary();
        EditorGUILayout.Space();
        DrawPreviewTable();
    }

    private void DrawSummary()
    {
        var convertibleCount = _previewItems.Count(item => item.Convertible);
        var unsupportedCount = _previewItems.Count(item => !item.Convertible);

        EditorGUILayout.HelpBox(
            $"Scanned materials: {_previewItems.Count}\nConvertible: {convertibleCount}\nUnsupported: {unsupportedCount}",
            MessageType.Info);
    }

    private void DrawPreviewTable()
    {
        using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
        {
            GUILayout.Label("Material", GUILayout.Width(360f));
            GUILayout.Label("Source Shader", GUILayout.Width(240f));
            GUILayout.Label("Target Shader", GUILayout.Width(240f));
            GUILayout.Label("Recipe", GUILayout.Width(180f));
            GUILayout.Label("Note");
        }

        _scroll = EditorGUILayout.BeginScrollView(_scroll);
        foreach (var item in _previewItems)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.SelectableLabel(item.Path, GUILayout.Width(360f), GUILayout.Height(EditorGUIUtility.singleLineHeight));
                EditorGUILayout.SelectableLabel(item.SourceShader, GUILayout.Width(240f), GUILayout.Height(EditorGUIUtility.singleLineHeight));
                EditorGUILayout.SelectableLabel(item.TargetShader, GUILayout.Width(240f), GUILayout.Height(EditorGUIUtility.singleLineHeight));
                EditorGUILayout.SelectableLabel(item.RecipeName, GUILayout.Width(180f), GUILayout.Height(EditorGUIUtility.singleLineHeight));
                EditorGUILayout.SelectableLabel(item.Note, GUILayout.Height(EditorGUIUtility.singleLineHeight));
            }
        }
        EditorGUILayout.EndScrollView();
    }

    private void ScanMaterials()
    {
        _previewItems.Clear();
        var materialPaths = CollectMaterialPaths();

        try
        {
            for (var i = 0; i < materialPaths.Count; i++)
            {
                var path = materialPaths[i];
                EditorUtility.DisplayProgressBar("FX Shader Migration", path, (float)(i + 1) / materialPaths.Count);

                var material = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (material == null || material.shader == null)
                    continue;

                var preview = BuildPreviewItem(material, path);
                if (preview != null)
                    _previewItems.Add(preview);
            }
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }
    }

    private void ReplacePreviewedMaterials()
    {
        var convertibleItems = _previewItems.Where(item => item.Convertible).ToList();
        if (convertibleItems.Count == 0)
            return;

        var changedCount = 0;

        try
        {
            AssetDatabase.StartAssetEditing();
            for (var i = 0; i < convertibleItems.Count; i++)
            {
                var item = convertibleItems[i];
                EditorUtility.DisplayProgressBar("FX Shader Migration", item.Path, (float)(i + 1) / convertibleItems.Count);

                var material = AssetDatabase.LoadAssetAtPath<Material>(item.Path);
                if (material == null || material.shader == null)
                    continue;

                if (!TryGetRecipe(material, out var recipe))
                    continue;

                if (ApplyRecipe(material, recipe))
                {
                    EditorUtility.SetDirty(material);
                    changedCount++;
                }
            }
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
            EditorUtility.ClearProgressBar();
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        ScanMaterials();

        EditorUtility.DisplayDialog(
            "FX Shader Migration",
            $"Updated materials: {changedCount}",
            "OK");
    }

    private List<string> CollectMaterialPaths()
    {
        switch (_scanScope)
        {
            case ScanScope.AllMaterials:
                return AssetDatabase.FindAssets("t:Material")
                    .Select(AssetDatabase.GUIDToAssetPath)
                    .Where(path => path.EndsWith(".mat", StringComparison.OrdinalIgnoreCase))
                    .Distinct()
                    .OrderBy(path => path, StringComparer.Ordinal)
                    .ToList();

            case ScanScope.Selection:
                return CollectSelectionMaterials();

            default:
                return CollectEffectDependencyMaterials();
        }
    }

    private List<string> CollectEffectDependencyMaterials()
    {
        var roots = SplitRoots(_rootText);
        var prefabGuids = AssetDatabase.FindAssets("t:Prefab", roots);
        var prefabPaths = prefabGuids.Select(AssetDatabase.GUIDToAssetPath).ToArray();
        var dependencies = AssetDatabase.GetDependencies(prefabPaths, true);

        return dependencies
            .Where(path => path.EndsWith(".mat", StringComparison.OrdinalIgnoreCase))
            .Distinct()
            .OrderBy(path => path, StringComparer.Ordinal)
            .ToList();
    }

    private static List<string> CollectSelectionMaterials()
    {
        var materialPaths = new HashSet<string>(StringComparer.Ordinal);
        foreach (var guid in Selection.assetGUIDs)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            if (string.IsNullOrEmpty(path))
                continue;

            if (Directory.Exists(path))
            {
                foreach (var subGuid in AssetDatabase.FindAssets("t:Material", new[] { path }))
                    materialPaths.Add(AssetDatabase.GUIDToAssetPath(subGuid));

                var prefabPaths = AssetDatabase.FindAssets("t:Prefab", new[] { path })
                    .Select(AssetDatabase.GUIDToAssetPath)
                    .ToArray();
                foreach (var dependency in AssetDatabase.GetDependencies(prefabPaths, true))
                {
                    if (dependency.EndsWith(".mat", StringComparison.OrdinalIgnoreCase))
                        materialPaths.Add(dependency);
                }

                continue;
            }

            if (path.EndsWith(".mat", StringComparison.OrdinalIgnoreCase))
            {
                materialPaths.Add(path);
                continue;
            }

            if (path.EndsWith(".prefab", StringComparison.OrdinalIgnoreCase))
            {
                foreach (var dependency in AssetDatabase.GetDependencies(path, true))
                {
                    if (dependency.EndsWith(".mat", StringComparison.OrdinalIgnoreCase))
                        materialPaths.Add(dependency);
                }
            }
        }

        return materialPaths.OrderBy(path => path, StringComparer.Ordinal).ToList();
    }

    private static string[] SplitRoots(string rootText)
    {
        return rootText
            .Split(new[] { '\r', '\n', ';', ',' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(path => path.Trim())
            .Where(path => !string.IsNullOrEmpty(path))
            .Distinct(StringComparer.Ordinal)
            .ToArray();
    }

    private static PreviewItem BuildPreviewItem(Material material, string path)
    {
        if (material.shader == null)
            return null;

        if (TryGetRecipe(material, out var recipe))
        {
            return new PreviewItem
            {
                Path = path,
                SourceShader = material.shader.name,
                TargetShader = recipe.TargetShader,
                RecipeName = recipe.Name,
                Note = "Ready",
                Convertible = true,
            };
        }

        if (material.shader.name.StartsWith("Project/FX/", StringComparison.Ordinal))
        {
            return new PreviewItem
            {
                Path = path,
                SourceShader = material.shader.name,
                TargetShader = material.shader.name,
                RecipeName = "Already Unified",
                Note = "Skip",
                Convertible = false,
            };
        }

        return new PreviewItem
        {
            Path = path,
            SourceShader = material.shader.name,
            TargetShader = string.Empty,
            RecipeName = "Unsupported",
            Note = "No recipe",
            Convertible = false,
        };
    }

    private static bool TryGetRecipe(Material material, out MigrationRecipe recipe)
    {
        recipe = Recipes.FirstOrDefault(candidate =>
            candidate.SourceShaders.Length > 0 &&
            candidate.SourceShaders.Contains(material.shader.name, StringComparer.Ordinal));

        if (recipe != null)
            return true;

        if (LooksLikeBuiltInParticle(material))
        {
            recipe = Recipes.First(candidate => candidate.Name == "BuiltIn Particle Simple");
            return true;
        }

        return false;
    }

    public static bool TryMigrateMaterial(Material material)
    {
        if (material == null || material.shader == null)
            return false;

        if (!TryGetRecipe(material, out var recipe))
            return false;

        return ApplyRecipe(material, recipe);
    }

    private static bool ApplyRecipe(Material material, MigrationRecipe recipe)
    {
        var targetShader = Shader.Find(recipe.TargetShader);
        if (targetShader == null)
        {
            Debug.LogError($"Target shader not found: {recipe.TargetShader}");
            return false;
        }

        var bag = MaterialPropertyBag.Capture(material);
        material.shader = targetShader;
        material.shaderKeywords = Array.Empty<string>();
        material.renderQueue = bag.RenderQueue;

        ApplyRenderState(material, bag, recipe.RenderState);
        recipe.Apply(bag, material);

        material.SetOverrideTag("LegacyFXShader", bag.ShaderName);
        material.SetOverrideTag("UnifiedFXRecipe", recipe.Name);
        return true;
    }

    private static void ApplyRenderState(Material target, MaterialPropertyBag bag, RenderStatePreset preset)
    {
        var srcBlend = preset.SrcBlend;
        var dstBlend = preset.DstBlend;
        var srcBlendAlpha = preset.SrcBlendAlpha;
        var dstBlendAlpha = preset.DstBlendAlpha;
        var cull = preset.Cull;
        var zWrite = preset.ZWrite;
        var zTest = preset.ZTest;

        if (bag.TryGetFloat(out var floatValue, "_SrcBlend"))
            srcBlend = floatValue;
        else if (preset.UseLegacyBlend2 && bag.TryGetFloat(out floatValue, "_Blend2"))
            dstBlend = floatValue;

        if (bag.TryGetFloat(out floatValue, "_DstBlend"))
            dstBlend = floatValue;
        if (bag.TryGetFloat(out floatValue, "_SrcBlendAlpha", "_SrcBlend1"))
            srcBlendAlpha = floatValue;
        if (bag.TryGetFloat(out floatValue, "_DstBlendAlpha", "_DstBlend1"))
            dstBlendAlpha = floatValue;
        if (bag.TryGetFloat(out floatValue, "_Cull", "_CullMode"))
            cull = floatValue;
        if (bag.TryGetFloat(out floatValue, "_ZWrite"))
            zWrite = floatValue;
        if (bag.TryGetFloat(out floatValue, "_ZTest"))
            zTest = floatValue;

        SetFloat(target, "_SrcBlend", srcBlend);
        SetFloat(target, "_DstBlend", dstBlend);
        SetFloat(target, "_SrcBlendAlpha", srcBlendAlpha);
        SetFloat(target, "_DstBlendAlpha", dstBlendAlpha);
        SetFloat(target, "_Cull", cull);
        SetFloat(target, "_ZWrite", zWrite);
        SetFloat(target, "_ZTest", zTest);
    }

    private static void ApplySpriteSimpleAdd(MaterialPropertyBag bag, Material target)
    {
        ApplySpriteSimpleCommon(bag, target, 0f);
        SetFloat(target, "_Emission", FirstFloat(bag, 1f, "_Emission", "_HdrMultiply"));
        SetFloat(target, "_Opacity", FirstFloat(bag, 1f, "_Opacity"));
    }

    private static void ApplySpriteSimpleAlpha(MaterialPropertyBag bag, Material target)
    {
        ApplySpriteSimpleCommon(bag, target, 0f);
        SetFloat(target, "_Emission", FirstFloat(bag, 1f, "_Emission", "_HdrMultiply"));
        SetFloat(target, "_Opacity", FirstFloat(bag, 1f, "_Opacity"));
    }

    private static void ApplySpriteSimpleCommon(MaterialPropertyBag bag, Material target, float effectMode)
    {
        SetFloat(target, "_EffectMode", effectMode);
        CopyTexture(target, "_MainTex", bag, "_MainTex", "_BaseMap", "_MainTexture", "Texture2D_F593E37E", "Texture2D_EDA87E5");
        CopyTexture(target, "_Noise", bag, "_Noise", "_OpacityTex", "_EmissionTex");
        CopyColor(target, "_Color", bag, Color.white, "_Color", "_BaseColor");
        CopyVector(target, "_SpeedMainTexUVNoiseZW", bag, Vector4.zero, "_SpeedMainTexUVNoiseZW");
        ApplySoftParticles(target, bag);
    }

    private static void ApplyBuiltInParticleSimple(MaterialPropertyBag bag, Material target)
    {
        ApplySpriteSimpleCommon(bag, target, 0f);
        CopyColor(target, "_Color", bag, Color.white, "_TintColor", "_Color");
        SetFloat(target, "_Emission", 1f);
        SetFloat(target, "_Opacity", bag.TryGetColor(out var tintColor, "_TintColor") ? tintColor.a : 1f);
    }

    private static void ApplySpriteCenterGlow(MaterialPropertyBag bag, Material target)
    {
        ApplySpriteSimpleCommon(bag, target, 1f);
        CopyTexture(target, "_Mask", bag, "_Mask");
        CopyTexture(target, "_Flow", bag, "_Flow");
        CopyVector(target, "_DistortionSpeedXYPowerZ", bag, Vector4.zero, "_DistortionSpeedXYPowerZ");
        SetFloat(target, "_Emission", FirstFloat(bag, 1f, "_Emission"));
        SetFloat(target, "_Opacity", FirstFloat(bag, 1f, "_Opacity"));
        SetFloat(target, "_CenterGlowStrength", FirstFloat(bag, 1f, "_Usecenterglow") > 0.5f ? 1f : 0f);
    }

    private static void ApplySpriteTrail(MaterialPropertyBag bag, Material target)
    {
        SetFloat(target, "_EffectMode", 2f);
        CopyTexture(target, "_MainTex", bag, "_MainTexture", "_MainTex");
        CopyTexture(target, "_Noise", bag, "_Noise");
        CopyColor(target, "_StartColor", bag, Color.white, "_StartColor");
        CopyColor(target, "_EndColor", bag, Color.white, "_EndColor");
        CopyVector(target, "_SpeedMainTexUVNoiseZW", bag, Vector4.zero, "_SpeedMainTexUVNoiseZW");
        SetFloat(target, "_GradientPower", FirstFloat(bag, 1f, "_Colorpower"));
        SetFloat(target, "_GradientRange", FirstFloat(bag, 1f, "_Colorrange"));
        SetFloat(target, "_MaskPower", FirstFloat(bag, 1f, "_Maskpower"));
        SetFloat(target, "_Emission", FirstFloat(bag, 1f, "_Emission"));
        ApplySoftParticles(target, bag);
    }

    private static void ApplySpriteLinePath(MaterialPropertyBag bag, Material target)
    {
        SetFloat(target, "_EffectMode", 3f);
        CopyTexture(target, "_MainTex", bag, "_MainTex", "_MainTexture");
        CopyTexture(target, "_Noise", bag, "_Noise", "_EmissionTex");
        CopyTexture(target, "_Flow", bag, "_Flow");
        CopyColor(target, "_Color", bag, Color.white, "_Color");
        CopyColor(target, "_EdgeColor", bag, Color.black, "_AddColor");
        CopyVector(target, "_SpeedMainTexUVNoiseZW", bag, Vector4.zero, "_SpeedMainTexUVNoiseZW");
        CopyVectorToFlowScroll(target, bag, "_SpeedFlow");
        SetFloat(target, "_Emission", FirstFloat(bag, 1f, "_Emission"));
        SetFloat(target, "_Opacity", FirstFloat(bag, 1f, "_Opacity"));
        SetFloat(target, "_DissolveThreshold", FirstFloat(bag, 0f, "_Dissolve"));
        SetFloat(target, "_DissolveSoftness", FirstFloat(bag, 0.1f, "_Usesmoothdissolve") > 0.5f ? 0.2f : 0.08f);
        SetFloat(target, "_FlowStrength", FirstFloat(bag, 0f, "_Flowpower"));
        ApplySoftParticles(target, bag);
    }

    private static void ApplySpriteFire(MaterialPropertyBag bag, Material target)
    {
        SetFloat(target, "_EffectMode", 4f);
        var hasTex1 = CopyTexture(target, "_Tex1", bag, "_Tex1", "_Tex0");
        var hasTex2 = CopyTexture(target, "_Tex2", bag, "_Tex2");
        CopyTexture(target, "_Mask", bag, "_Mask");
        CopyColor(target, "_Color", bag, Color.white, "_Color1");
        CopyColor(target, "_SecondaryColor", bag, Color.white, "_Color2");
        CopyVector(target, "_SpeedTex1", bag, Vector4.zero, "_SpeedTex1");
        CopyVector(target, "_SpeedTex2XYEmission", bag, new Vector4(0f, 0f, 1f, 0f), "_SpeedTex2XYEmission");
        var legacyEmission = 1f;
        if (bag.TryGetVector(out var legacyFireVector, "_SpeedTex2XYEmission"))
            legacyEmission = legacyFireVector.z;
        SetFloat(target, "_Emission", FirstFloat(bag, legacyEmission, "_Emission"));
        SetFloat(target, "_Opacity", FirstFloat(bag, 1f, "_Opacity"));
        SetToggleKeyword(target, "_UseTex1Tex", "_FX_USE_TEX1_TEX", hasTex1);
        SetToggleKeyword(target, "_UseTex2Tex", "_FX_USE_TEX2_TEX", hasTex2);
        ApplySoftParticles(target, bag);
    }

    private static void ApplySpriteLightning(MaterialPropertyBag bag, Material target)
    {
        SetFloat(target, "_EffectMode", 5f);
        CopyTexture(target, "_MainTex", bag, "_MainTexture", "_MainTex");
        CopyTexture(target, "_Noise", bag, "_Noise");
        var hasFlowMap = CopyTexture(target, "_FlowMap", bag, "_FlowMap");
        CopyColor(target, "_Color", bag, Color.white, "_Color");
        SetVector(target, "_SpeedTex2XYEmission", new Vector4(
            FirstFloat(bag, 0f, "_UFlowSpeed"),
            FirstFloat(bag, 0f, "_VFlowSpeed"),
            FirstFloat(bag, 1f, "_Emission"),
            0f));
        SetFloat(target, "_FlowStrength", FirstFloat(bag, 0.1f, "_FlowStrength"));
        SetFloat(target, "_Emission", FirstFloat(bag, 1f, "_Emission"));
        SetToggleKeyword(target, "_UseFlowMapTex", "_FX_USE_FLOWMAP_TEX", hasFlowMap);
        ApplySoftParticles(target, bag);
    }

    private static void ApplySpriteIce(MaterialPropertyBag bag, Material target)
    {
        SetFloat(target, "_EffectMode", 6f);
        CopyTexture(target, "_MainTex", bag, "_MainTex");
        CopyColor(target, "_Color", bag, Color.white, "_Color");
        CopyColor(target, "_SecondaryColor", bag, Color.white, "_UpColor");
        CopyColor(target, "_EdgeColor", bag, Color.white, "_FresnelColor");
        SetFloat(target, "_Emission", FirstFloat(bag, 1f, "_Emission"));
        SetFloat(target, "_FresnelStrength", FirstFloat(bag, 1f, "_FresnelScale", "_FresnelEmission"));
        SetFloat(target, "_FresnelPower", FirstFloat(bag, 4f, "_FresnelPower"));
        SetFloat(target, "_SecondaryBlend", FirstFloat(bag, 0.5f, "_ColorPosition"));
        ApplySoftParticles(target, bag);
    }

    private static void ApplyDistortionOnly(MaterialPropertyBag bag, Material target)
    {
        SetFloat(target, "_EffectMode", 0f);
        CopyTexture(target, "_NormalMap", bag, "_NormalMap");
        CopyTexture(target, "_MainTex", bag, "_MainTex");
        CopyColor(target, "_Color", bag, Color.white, "_Color");
        SetFloat(target, "_Distortionpower", FirstFloat(bag, 0.05f, "_Distortionpower"));
        SetFloat(target, "_Opacity", FirstFloat(bag, 1f, "_Opacity"));
        ApplySoftParticles(target, bag);
    }

    private static void ApplyDistortionOverlay(MaterialPropertyBag bag, Material target)
    {
        SetFloat(target, "_EffectMode", 1f);
        CopyTexture(target, "_MainTex", bag, "_MainTex");
        CopyTexture(target, "_Noise", bag, "_Noise");
        CopyTexture(target, "_Flow", bag, "_Flow");
        CopyTexture(target, "_Mask", bag, "_Mask");
        CopyColor(target, "_Color", bag, Color.white, "_Color");
        CopyVector(target, "_SpeedMainTexUVNoiseZW", bag, Vector4.zero, "_SpeedMainTexUVNoiseZW");
        CopyVector(target, "_DistortionSpeedXYPowerZ", bag, Vector4.zero, "_DistortionSpeedXYPowerZ");
        SetFloat(target, "_Emission", FirstFloat(bag, 1f, "_Emission"));
        SetFloat(target, "_Opacity", FirstFloat(bag, 1f, "_Opacity"));
        SetFloat(target, "_Distortionpower", FirstFloat(bag, 0.05f, "_Distortionpower"));
        SetFloat(target, "_Softedges", FirstFloat(bag, 0f, "_Softedges"));
        SetFloat(target, "_Sideopacitymult", FirstFloat(bag, 5f, "_Sideopacitymult"));
        ApplySoftParticles(target, bag);
    }

    private static void ApplyDissolve(MaterialPropertyBag bag, Material target)
    {
        var effectMode = 7f;
        if (string.Equals(bag.ShaderName, "Shader Graphs/Fx_ParticleDissolve_apb", StringComparison.Ordinal) ||
            string.Equals(bag.ShaderName, "Shader Graphs/Fx_RockDissolve", StringComparison.Ordinal))
        {
            effectMode = 9f;
        }
        else if (string.Equals(bag.ShaderName, "Project/FX/FX_Dissolve_URP", StringComparison.Ordinal) &&
                 bag.TryGetFloat(out var legacyMode, "_EffectMode"))
        {
            effectMode = legacyMode > 1.5f ? 9f : legacyMode > 0.5f ? 8f : 7f;
        }

        SetFloat(target, "_EffectMode", effectMode);
        CopyTexture(target, "_MainTex", bag, "_MainTex", "Texture2D_F593E37E", "Texture2D_EDA87E5");
        CopyTexture(target, "_Noise", bag, "_TextureNoise", "_Noise", "_OpacityTex", "_DissolveTex");
        var hasDissolveTex = CopyTexture(target, "_DissolveTex", bag, "_Dissolvenoise", "_Noise", "_OpacityTex", "_DissolveTex");
        CopyColor(target, "_Color", bag, Color.white, "_Maincolor", "_Color", "_BaseColor");
        CopyColor(target, "_SecondaryColor", bag, Color.white, "_Noisecolor", "_Color");
        CopyColor(target, "_EdgeColor", bag, Color.white, "_Dissolvecolor", "_AddColor", "_EmissionColor");
        if (bag.TryGetVector(out var legacyNoiseVector, "_NoisespeedXYEmissonZPowerW", "Vector2_176F980A"))
        {
            SetVector(target, "_NoiseScroll", legacyNoiseVector);
            SetFloat(target, "_Emission", legacyNoiseVector.z == 0f ? 1f : legacyNoiseVector.z);
            SetFloat(target, "_NoisePower", legacyNoiseVector.w == 0f ? 1f : legacyNoiseVector.w);
        }
        else
        {
            SetVector(target, "_NoiseScroll", Vector4.zero);
            SetFloat(target, "_Emission", 1f);
            SetFloat(target, "_NoisePower", 1f);
        }
        CopyVector(target, "_DissolveScroll", bag, Vector4.zero, "_DissolvespeedXY");
        SetFloat(target, "_Opacity", FirstFloat(bag, 1f, "_Opacity"));
        SetFloat(target, "_DissolveThreshold", FirstFloat(bag, 0.5f, "_Dissolve", "_Cutoff", "Vector1_D283BF28"));
        SetFloat(target, "_DissolveSoftness", FirstFloat(bag, 0.08f, "_DissolveSoftness"));
        SetFloat(target, "_DissolveEdgeWidth", FirstFloat(bag, 0.1f, "_DissolveEdgeWidth"));
        SetToggleKeyword(target, "_UseDissolveTex", "_FX_USE_DISSOLVE_TEX", hasDissolveTex);
        ApplySoftParticles(target, bag);
    }

    private static void ApplySoftNoise(MaterialPropertyBag bag, Material target)
    {
        SetFloat(target, "_EffectMode", 8f);
        CopyTexture(target, "_MainTex", bag, "_MainTex");
        CopyTexture(target, "_Noise", bag, "_OpacityTex");
        var hasDissolveTex = CopyTexture(target, "_DissolveTex", bag, "_OpacityTex");
        SetColor(target, "_Color", Color.white);
        SetColor(target, "_SecondaryColor", Color.white);
        SetColor(target, "_EdgeColor", Color.white);
        CopyVector(target, "_NoiseScroll", bag, new Vector4(0f, -0.5f, 1f, 1f), "_OpacityTexspeedXY");
        SetFloat(target, "_Emission", 1f);
        SetFloat(target, "_NoisePower", 1f);
        SetFloat(target, "_Opacity", 1f);
        SetFloat(target, "_DissolveThreshold", 0.5f);
        SetFloat(target, "_DissolveSoftness", 0.35f);
        SetFloat(target, "_DissolveEdgeWidth", 0.1f);
        SetToggleKeyword(target, "_UseDissolveTex", "_FX_USE_DISSOLVE_TEX", hasDissolveTex);
        ApplySoftParticles(target, bag);
    }

    private static void ApplyShockwave(MaterialPropertyBag bag, Material target)
    {
        var effectMode = 10f;
        if (string.Equals(bag.ShaderName, "Project/FX/FX_Shockwave_URP", StringComparison.Ordinal) &&
            bag.TryGetFloat(out var legacyMode, "_EffectMode"))
        {
            effectMode = legacyMode > 1.5f ? 12f : legacyMode > 0.5f ? 11f : 10f;
        }

        SetFloat(target, "_EffectMode", effectMode);
        CopyTexture(target, "_MainTex", bag, "_MainTexture", "_MainTex");
        CopyTexture(target, "_Noise", bag, "_Noise");
        CopyTexture(target, "_Flow", bag, "_Flow");
        CopyTexture(target, "_Mask", bag, "_Mask");
        CopyColor(target, "_Color", bag, Color.white, "_Color");
        CopyVector(target, "_NoiseScroll", bag, Vector4.zero, "_NoiseSpeedXYPowerZ");
        CopyVector(target, "_DistortionSpeedXYPowerZ", bag, Vector4.zero, "_DistortionSpeedXYPowerZ");
        SetFloat(target, "_Emission", FirstFloat(bag, 1f, "_Emission"));
        SetFloat(target, "_Opacity", FirstFloat(bag, 1f, "_Opacity"));
        SetFloat(target, "_InnerRadius", FirstFloat(bag, 0.2f, "_InnerRadius"));
        SetFloat(target, "_OuterRadius", FirstFloat(bag, 0.8f, "_OuterRadius"));
        SetFloat(target, "_RingSoftness", FirstFloat(bag, 0.1f, "_RingSoftness"));
        ApplySoftParticles(target, bag);
    }

    private static void ApplyTwoSided(MaterialPropertyBag bag, Material target)
    {
        CopyTexture(target, "_FrontTex", bag, "_MainTex");
        CopyTexture(target, "_BackTex", bag, "_MainTex");
        CopyTexture(target, "_Noise", bag, "_Noise");
        CopyTexture(target, "_Mask", bag, "_Mask");
        CopyColor(target, "_FrontColor", bag, Color.white, "_FrontFacesColor", "_Color");
        CopyColor(target, "_BackColor", bag, Color.white, "_BackFacesColor", "_Color");
        CopyColor(target, "_FrontFresnelColor", bag, Color.white, "_FresnelColor");
        CopyColor(target, "_BackFresnelColor", bag, Color.white, "_BackFresnelColor");
        CopyVector(target, "_FrontScroll", bag, Vector4.zero, "_SpeedMainTexUVNoiseZW");
        CopyVector(target, "_BackScroll", bag, Vector4.zero, "_SpeedMainTexUVNoiseZW");
        CopyVector(target, "_NoiseScroll", bag, Vector4.zero, "_SpeedMainTexUVNoiseZW");
        SetFloat(target, "_Emission", FirstFloat(bag, 1f, "_Emission"));
        SetFloat(target, "_Opacity", FirstFloat(bag, 1f, "_Opacity"));
        SetFloat(target, "_SideOpacity", FirstFloat(bag, 1f, "_Sideopacity"));
        SetFloat(target, "_FrontFresnelStrength", FirstFloat(bag, 0f, "_UseFresnel") > 0.5f ? FirstFloat(bag, 1f, "_FresnelEmission", "_Fresnel") : 0f);
        SetFloat(target, "_BackFresnelStrength", FirstFloat(bag, 0f, "_UseBackFresnel") > 0.5f ? FirstFloat(bag, 1f, "_BackFresnelEmission", "_BackFresnel") : 0f);
        SetFloat(target, "_FresnelPower", FirstFloat(bag, 4f, "_Fresnel", "_BackFresnel"));
        ApplySoftParticles(target, bag);
    }

    private static void ApplySoftParticles(Material target, MaterialPropertyBag bag)
    {
        var useSoftParticles = false;
        var nearFade = 0f;
        var farFade = 1f;
        var hasExplicitFade = false;

        if (bag.TryGetFloat(out var flag, "_UseSP", "_SoftParticlesEnabled", "_Usedepth"))
            useSoftParticles |= flag > 0.5f;

        if (bag.TryGetFloat(out var invFade, "_InvFade"))
        {
            useSoftParticles |= invFade > 0f;
            if (!hasExplicitFade)
                farFade = Mathf.Max(0.1f, 1f / Mathf.Max(invFade, 0.001f));
        }

        if (bag.TryGetFloat(out flag, "_SoftParticlesNearFadeDistance", "_SoftParticlesFadeDistanceNear"))
        {
            nearFade = flag;
            hasExplicitFade = true;
        }
        if (bag.TryGetFloat(out flag, "_SoftParticlesFarFadeDistance", "_SoftParticlesFadeDistanceFar"))
        {
            farFade = flag;
            hasExplicitFade = true;
        }

        if (farFade <= nearFade)
            farFade = Mathf.Max(nearFade + 0.1f, 1f);

        SetFloat(target, "_UseSoftParticle", useSoftParticles ? 1f : 0f);
        SetFloat(target, "_SoftParticleNearFadeDistance", nearFade);
        SetFloat(target, "_SoftParticleFarFadeDistance", farFade);
    }

    private static bool LooksLikeBuiltInParticle(Material material)
    {
        return material != null &&
               material.shader != null &&
               string.IsNullOrEmpty(AssetDatabase.GetAssetPath(material.shader)) &&
               material.HasProperty("_MainTex") &&
               material.HasProperty("_TintColor") &&
               material.HasProperty("_InvFade");
    }

    private static bool CopyTexture(Material target, string targetName, MaterialPropertyBag bag, params string[] sourceNames)
    {
        if (!target.HasProperty(targetName))
            return false;

        if (!bag.TryGetTexture(out var value, sourceNames))
            return false;

        target.SetTexture(targetName, value.Texture);
        target.SetTextureScale(targetName, value.Scale);
        target.SetTextureOffset(targetName, value.Offset);
        return value.Texture != null;
    }

    private static void CopyColor(Material target, string targetName, MaterialPropertyBag bag, Color defaultValue, params string[] sourceNames)
    {
        if (!target.HasProperty(targetName))
            return;

        target.SetColor(targetName, bag.TryGetColor(out var value, sourceNames) ? value : defaultValue);
    }

    private static void CopyVector(Material target, string targetName, MaterialPropertyBag bag, Vector4 defaultValue, params string[] sourceNames)
    {
        if (!target.HasProperty(targetName))
            return;

        target.SetVector(targetName, bag.TryGetVector(out var value, sourceNames) ? value : defaultValue);
    }

    private static void CopyVectorToFlowScroll(Material target, MaterialPropertyBag bag, params string[] sourceNames)
    {
        if (!target.HasProperty("_FlowScroll"))
            return;

        if (!bag.TryGetFloat(out var speed, sourceNames))
            return;

        target.SetVector("_FlowScroll", new Vector4(speed, speed, 0f, 0f));
    }

    private static float FirstFloat(MaterialPropertyBag bag, float defaultValue, params string[] sourceNames)
    {
        return bag.TryGetFloat(out var value, sourceNames) ? value : defaultValue;
    }

    private static void SetFloat(Material target, string name, float value)
    {
        if (target.HasProperty(name))
            target.SetFloat(name, value);
    }

    private static void SetToggleKeyword(Material target, string propertyName, string keyword, bool enabled)
    {
        SetFloat(target, propertyName, enabled ? 1f : 0f);
        if (enabled)
            target.EnableKeyword(keyword);
        else
            target.DisableKeyword(keyword);
    }

    private static void SetColor(Material target, string name, Color value)
    {
        if (target.HasProperty(name))
            target.SetColor(name, value);
    }

    private static void SetVector(Material target, string name, Vector4 value)
    {
        if (target.HasProperty(name))
            target.SetVector(name, value);
    }
}
