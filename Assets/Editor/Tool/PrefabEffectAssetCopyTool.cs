using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

internal sealed class PrefabEffectAssetCopyTool : EditorWindow
{
    private const string CopyMarkerPrefix = "PrefabEffectAssetCopyTool::";
    private const string TexturesFolderName = "Textures";
    private const string MaterialsFolderName = "Materials";
    private const string MeshesFolderName = "Meshes";

    private sealed class ScanResult
    {
        public readonly List<string> Prefabs = new List<string>();
        public readonly List<string> Materials = new List<string>();
        public readonly List<string> Textures = new List<string>();
        public readonly List<string> MeshAssets = new List<string>();
    }

    private static readonly HashSet<string> TextureExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        ".png",
        ".jpg",
        ".jpeg",
        ".tga",
        ".psd",
        ".tif",
        ".tiff",
        ".bmp",
        ".gif",
        ".hdr",
        ".exr",
        ".dds",
    };

    private string _sourceRootText = "Assets/Resources/Prefabs/Effect";
    private string _targetRoot = "Assets/CopiedEffectAssets";
    private Vector2 _scroll;
    private ScanResult _result;

    [MenuItem("Tools/Asset/Prefab Effect Asset Copy Tool")]
    private static void OpenWindow()
    {
        var window = GetWindow<PrefabEffectAssetCopyTool>("FX Asset Copy");
        window.minSize = new Vector2(860f, 520f);
    }

    private void OnGUI()
    {
        EditorGUILayout.Space();
        _sourceRootText = EditorGUILayout.TextField("Prefab Source Roots", _sourceRootText);
        _targetRoot = EditorGUILayout.TextField("Target Root", _targetRoot);

        EditorGUILayout.HelpBox(
            "Scans prefabs under the source roots, copies missing dependencies into three flat folders under the target root: Textures, Materials, Meshes. Name collisions are auto-renamed. Existing copies from the same source are reused. After copying, the original prefabs are rewritten to reference the new materials and meshes, and the new materials are rewritten to reference the new textures.",
            MessageType.Info);

        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Scan", GUILayout.Height(30f)))
                Scan();

            using (new EditorGUI.DisabledScope(_result == null))
            {
                if (GUILayout.Button("Copy And Relink", GUILayout.Height(30f)))
                    CopyAndRelink();
            }
        }

        EditorGUILayout.Space();
        DrawSummary();
        EditorGUILayout.Space();
        DrawPreview();
    }

    private void DrawSummary()
    {
        if (_result == null)
        {
            EditorGUILayout.HelpBox("No scan results yet.", MessageType.None);
            return;
        }

        EditorGUILayout.HelpBox(
            $"Prefabs: {_result.Prefabs.Count}\nMaterials: {_result.Materials.Count}\nTextures: {_result.Textures.Count}\nMeshes: {_result.MeshAssets.Count}",
            MessageType.Info);
    }

    private void DrawPreview()
    {
        if (_result == null)
            return;

        _scroll = EditorGUILayout.BeginScrollView(_scroll);
        DrawSection("Prefabs", _result.Prefabs);
        DrawSection("Materials", _result.Materials);
        DrawSection("Textures", _result.Textures);
        DrawSection("Meshes", _result.MeshAssets);
        EditorGUILayout.EndScrollView();
    }

    private static void DrawSection(string title, IReadOnlyList<string> paths)
    {
        EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
        for (var i = 0; i < paths.Count; i++)
            EditorGUILayout.SelectableLabel(paths[i], GUILayout.Height(EditorGUIUtility.singleLineHeight));
        EditorGUILayout.Space();
    }

    private void Scan()
    {
        var sourceRoots = SplitRoots(_sourceRootText);
        var prefabs = FindPrefabs(sourceRoots);
        var materialSet = new HashSet<string>(StringComparer.Ordinal);
        var textureSet = new HashSet<string>(StringComparer.Ordinal);
        var meshSet = new HashSet<string>(StringComparer.Ordinal);

        try
        {
            for (var i = 0; i < prefabs.Count; i++)
            {
                var prefabPath = prefabs[i];
                EditorUtility.DisplayProgressBar("Prefab Effect Asset Copy", prefabPath, (float)(i + 1) / Math.Max(1, prefabs.Count));

                foreach (var dependency in AssetDatabase.GetDependencies(prefabPath, true))
                {
                    if (!dependency.StartsWith("Assets/", StringComparison.Ordinal))
                        continue;

                    if (dependency.EndsWith(".mat", StringComparison.OrdinalIgnoreCase))
                    {
                        materialSet.Add(dependency);
                        continue;
                    }

                    if (IsTextureAsset(dependency))
                    {
                        textureSet.Add(dependency);
                        continue;
                    }

                    if (IsMeshAsset(dependency))
                        meshSet.Add(dependency);
                }
            }
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }

        _result = new ScanResult();
        _result.Prefabs.AddRange(prefabs.OrderBy(path => path, StringComparer.Ordinal));
        _result.Materials.AddRange(materialSet.OrderBy(path => path, StringComparer.Ordinal));
        _result.Textures.AddRange(textureSet.OrderBy(path => path, StringComparer.Ordinal));
        _result.MeshAssets.AddRange(meshSet.OrderBy(path => path, StringComparer.Ordinal));
    }

    private void CopyAndRelink()
    {
        if (_result == null)
            return;

        if (!IsValidAssetsPath(_targetRoot))
        {
            EditorUtility.DisplayDialog("Prefab Effect Asset Copy", "Target Root must be under Assets/.", "OK");
            return;
        }

        var texturesRoot = CombineAssetPath(_targetRoot, TexturesFolderName);
        var materialsRoot = CombineAssetPath(_targetRoot, MaterialsFolderName);
        var meshesRoot = CombineAssetPath(_targetRoot, MeshesFolderName);

        EnsureDirectory(texturesRoot);
        EnsureDirectory(materialsRoot);
        EnsureDirectory(meshesRoot);

        var textureCopies = new Dictionary<string, string>(StringComparer.Ordinal);
        var materialCopies = new Dictionary<string, string>(StringComparer.Ordinal);
        var meshCopies = new Dictionary<string, string>(StringComparer.Ordinal);

        var totalCopyCount = _result.Textures.Count + _result.Materials.Count + _result.MeshAssets.Count;
        var currentIndex = 0;

        try
        {
            foreach (var sourcePath in _result.Textures)
            {
                currentIndex++;
                EditorUtility.DisplayProgressBar("Prefab Effect Asset Copy", sourcePath, (float)currentIndex / Math.Max(1, totalCopyCount));
                if (TryGetOrCreateCopy(sourcePath, texturesRoot, out var copiedPath))
                    textureCopies[sourcePath] = copiedPath;
            }

            foreach (var sourcePath in _result.Materials)
            {
                currentIndex++;
                EditorUtility.DisplayProgressBar("Prefab Effect Asset Copy", sourcePath, (float)currentIndex / Math.Max(1, totalCopyCount));
                if (TryGetOrCreateCopy(sourcePath, materialsRoot, out var copiedPath))
                    materialCopies[sourcePath] = copiedPath;
            }

            foreach (var sourcePath in _result.MeshAssets)
            {
                currentIndex++;
                EditorUtility.DisplayProgressBar("Prefab Effect Asset Copy", sourcePath, (float)currentIndex / Math.Max(1, totalCopyCount));
                if (TryGetOrCreateCopy(sourcePath, meshesRoot, out var copiedPath))
                    meshCopies[sourcePath] = copiedPath;
            }
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }

        RelinkCopiedMaterials(materialCopies, textureCopies);
        RelinkPrefabs(_result.Prefabs, materialCopies, meshCopies);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog(
            "Prefab Effect Asset Copy",
            $"Textures: {textureCopies.Count}\nMaterials: {materialCopies.Count}\nMeshes: {meshCopies.Count}\nPrefabs updated: {_result.Prefabs.Count}",
            "OK");
    }

    private static void RelinkCopiedMaterials(
        IReadOnlyDictionary<string, string> materialCopies,
        IReadOnlyDictionary<string, string> textureCopies)
    {
        var materialPaths = materialCopies.Values.ToList();

        try
        {
            for (var i = 0; i < materialPaths.Count; i++)
            {
                var materialPath = materialPaths[i];
                EditorUtility.DisplayProgressBar("Prefab Effect Asset Copy", $"Relink material {materialPath}", (float)(i + 1) / Math.Max(1, materialPaths.Count));

                var material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
                if (material == null || material.shader == null)
                    continue;

                var shader = material.shader;
                var propertyCount = ShaderUtil.GetPropertyCount(shader);
                var changed = false;

                for (var propertyIndex = 0; propertyIndex < propertyCount; propertyIndex++)
                {
                    if (ShaderUtil.GetPropertyType(shader, propertyIndex) != ShaderUtil.ShaderPropertyType.TexEnv)
                        continue;

                    var propertyName = ShaderUtil.GetPropertyName(shader, propertyIndex);
                    var referencedObject = material.GetTexture(propertyName);
                    if (referencedObject == null)
                        continue;

                    var sourcePath = AssetDatabase.GetAssetPath(referencedObject);
                    if (string.IsNullOrEmpty(sourcePath) || !textureCopies.TryGetValue(sourcePath, out var targetPath))
                        continue;

                    var copiedTexture = ResolveCopiedReference(referencedObject, targetPath);
                    if (copiedTexture == null || copiedTexture == referencedObject)
                        continue;

                    material.SetTexture(propertyName, copiedTexture as Texture);
                    changed = true;
                }

                if (changed)
                    EditorUtility.SetDirty(material);
            }
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }
    }

    private static void RelinkPrefabs(
        IReadOnlyList<string> prefabPaths,
        IReadOnlyDictionary<string, string> materialCopies,
        IReadOnlyDictionary<string, string> meshCopies)
    {
        try
        {
            for (var i = 0; i < prefabPaths.Count; i++)
            {
                var prefabPath = prefabPaths[i];
                EditorUtility.DisplayProgressBar("Prefab Effect Asset Copy", $"Relink prefab {prefabPath}", (float)(i + 1) / Math.Max(1, prefabPaths.Count));

                var prefabRoot = PrefabUtility.LoadPrefabContents(prefabPath);
                var changed = false;

                try
                {
                    changed |= RelinkRendererMaterials(prefabRoot, materialCopies);
                    changed |= RelinkMeshComponents(prefabRoot, meshCopies);

                    var components = prefabRoot.GetComponentsInChildren<Component>(true);
                    for (var componentIndex = 0; componentIndex < components.Length; componentIndex++)
                    {
                        var component = components[componentIndex];
                        if (component == null)
                            continue;

                        var serializedObject = new SerializedObject(component);
                        var iterator = serializedObject.GetIterator();
                        var enterChildren = true;
                        var componentChanged = false;

                        while (iterator.Next(enterChildren))
                        {
                            enterChildren = false;

                            if (iterator.propertyType != SerializedPropertyType.ObjectReference)
                                continue;

                            var referencedObject = iterator.objectReferenceValue;
                            if (referencedObject == null)
                                continue;

                            var sourcePath = AssetDatabase.GetAssetPath(referencedObject);
                            if (string.IsNullOrEmpty(sourcePath))
                                continue;

                            if (materialCopies.TryGetValue(sourcePath, out var copiedMaterialPath))
                            {
                                var copiedObject = ResolveCopiedReference(referencedObject, copiedMaterialPath);
                                if (copiedObject != null && copiedObject != referencedObject)
                                {
                                    iterator.objectReferenceValue = copiedObject;
                                    componentChanged = true;
                                }

                                continue;
                            }

                            if (meshCopies.TryGetValue(sourcePath, out var copiedMeshPath))
                            {
                                var copiedObject = ResolveCopiedReference(referencedObject, copiedMeshPath);
                                if (copiedObject != null && copiedObject != referencedObject)
                                {
                                    iterator.objectReferenceValue = copiedObject;
                                    componentChanged = true;
                                }
                            }
                        }

                        if (!componentChanged)
                            continue;

                        serializedObject.ApplyModifiedPropertiesWithoutUndo();
                        EditorUtility.SetDirty(component);
                        changed = true;
                    }

                    if (changed)
                        PrefabUtility.SaveAsPrefabAsset(prefabRoot, prefabPath);
                }
                finally
                {
                    PrefabUtility.UnloadPrefabContents(prefabRoot);
                }
            }
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }
    }

    private static bool RelinkRendererMaterials(
        GameObject prefabRoot,
        IReadOnlyDictionary<string, string> materialCopies)
    {
        var changed = false;
        var renderers = prefabRoot.GetComponentsInChildren<Renderer>(true);
        for (var i = 0; i < renderers.Length; i++)
        {
            var renderer = renderers[i];
            var sharedMaterials = renderer.sharedMaterials;
            if (sharedMaterials == null || sharedMaterials.Length == 0)
                continue;

            var rendererChanged = false;
            for (var materialIndex = 0; materialIndex < sharedMaterials.Length; materialIndex++)
            {
                var sourceMaterial = sharedMaterials[materialIndex];
                if (sourceMaterial == null)
                    continue;

                var sourcePath = AssetDatabase.GetAssetPath(sourceMaterial);
                if (string.IsNullOrEmpty(sourcePath) || !materialCopies.TryGetValue(sourcePath, out var copiedMaterialPath))
                    continue;

                var copiedMaterial = ResolveCopiedReference(sourceMaterial, copiedMaterialPath) as Material;
                if (copiedMaterial == null || copiedMaterial == sourceMaterial)
                    continue;

                sharedMaterials[materialIndex] = copiedMaterial;
                rendererChanged = true;
            }

            if (!rendererChanged)
                continue;

            renderer.sharedMaterials = sharedMaterials;
            EditorUtility.SetDirty(renderer);
            changed = true;
        }

        return changed;
    }

    private static bool RelinkMeshComponents(
        GameObject prefabRoot,
        IReadOnlyDictionary<string, string> meshCopies)
    {
        var changed = false;

        var meshFilters = prefabRoot.GetComponentsInChildren<MeshFilter>(true);
        for (var i = 0; i < meshFilters.Length; i++)
        {
            var meshFilter = meshFilters[i];
            changed |= ReplaceSharedMesh(meshFilter, meshFilter.sharedMesh, meshCopies, mesh => meshFilter.sharedMesh = mesh);
        }

        var skinnedMeshRenderers = prefabRoot.GetComponentsInChildren<SkinnedMeshRenderer>(true);
        for (var i = 0; i < skinnedMeshRenderers.Length; i++)
        {
            var skinnedMeshRenderer = skinnedMeshRenderers[i];
            changed |= ReplaceSharedMesh(skinnedMeshRenderer, skinnedMeshRenderer.sharedMesh, meshCopies, mesh => skinnedMeshRenderer.sharedMesh = mesh);
        }

        var particleSystemRenderers = prefabRoot.GetComponentsInChildren<ParticleSystemRenderer>(true);
        for (var i = 0; i < particleSystemRenderers.Length; i++)
        {
            var particleRenderer = particleSystemRenderers[i];
            changed |= ReplaceParticleMeshes(particleRenderer, meshCopies);
        }

        return changed;
    }

    private static bool ReplaceSharedMesh(
        Component owner,
        Mesh sourceMesh,
        IReadOnlyDictionary<string, string> meshCopies,
        Action<Mesh> assign)
    {
        if (sourceMesh == null)
            return false;

        var sourcePath = AssetDatabase.GetAssetPath(sourceMesh);
        if (string.IsNullOrEmpty(sourcePath) || !meshCopies.TryGetValue(sourcePath, out var copiedMeshPath))
            return false;

        var copiedMesh = ResolveCopiedReference(sourceMesh, copiedMeshPath) as Mesh;
        if (copiedMesh == null || copiedMesh == sourceMesh)
            return false;

        assign(copiedMesh);
        EditorUtility.SetDirty(owner);
        return true;
    }

    private static bool ReplaceParticleMeshes(
        ParticleSystemRenderer particleRenderer,
        IReadOnlyDictionary<string, string> meshCopies)
    {
        var changed = false;

        changed |= ReplaceSharedMesh(particleRenderer, particleRenderer.mesh, meshCopies, mesh => particleRenderer.mesh = mesh);

        var serializedObject = new SerializedObject(particleRenderer);
        serializedObject.Update();

        var meshProperty = serializedObject.FindProperty("m_Mesh");
        if (meshProperty != null && meshProperty.objectReferenceValue is Mesh sourceMesh)
        {
            var sourcePath = AssetDatabase.GetAssetPath(sourceMesh);
            if (!string.IsNullOrEmpty(sourcePath) && meshCopies.TryGetValue(sourcePath, out var copiedMeshPath))
            {
                var copiedMesh = ResolveCopiedReference(sourceMesh, copiedMeshPath) as Mesh;
                if (copiedMesh != null && copiedMesh != sourceMesh)
                {
                    meshProperty.objectReferenceValue = copiedMesh;
                    changed = true;
                }
            }
        }

        var meshesProperty = serializedObject.FindProperty("m_Meshes");
        if (meshesProperty != null && meshesProperty.isArray)
        {
            for (var i = 0; i < meshesProperty.arraySize; i++)
            {
                var element = meshesProperty.GetArrayElementAtIndex(i);
                if (!(element.objectReferenceValue is Mesh arraySourceMesh))
                    continue;

                var sourcePath = AssetDatabase.GetAssetPath(arraySourceMesh);
                if (string.IsNullOrEmpty(sourcePath) || !meshCopies.TryGetValue(sourcePath, out var copiedMeshPath))
                    continue;

                var copiedMesh = ResolveCopiedReference(arraySourceMesh, copiedMeshPath) as Mesh;
                if (copiedMesh == null || copiedMesh == arraySourceMesh)
                    continue;

                element.objectReferenceValue = copiedMesh;
                changed = true;
            }
        }

        if (!changed)
            return false;

        serializedObject.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(particleRenderer);
        return true;
    }

    private static Object ResolveCopiedReference(Object sourceObject, string copiedAssetPath)
    {
        if (sourceObject == null || string.IsNullOrEmpty(copiedAssetPath))
            return null;

        var sourceType = sourceObject.GetType();
        var directLoad = AssetDatabase.LoadAssetAtPath(copiedAssetPath, sourceType);
        if (directLoad != null && AssetDatabase.IsMainAsset(directLoad))
            return directLoad;

        var copiedAssets = AssetDatabase.LoadAllAssetsAtPath(copiedAssetPath);
        for (var i = 0; i < copiedAssets.Length; i++)
        {
            var candidate = copiedAssets[i];
            if (candidate == null || candidate.GetType() != sourceType)
                continue;

            if (candidate.name == sourceObject.name)
                return candidate;
        }

        return directLoad;
    }

    private static bool TryGetOrCreateCopy(string sourcePath, string destinationFolder, out string copiedPath)
    {
        copiedPath = FindExistingCopy(sourcePath, destinationFolder);
        if (!string.IsNullOrEmpty(copiedPath))
            return true;

        var fileName = Path.GetFileName(sourcePath);
        var destinationPath = AssetDatabase.GenerateUniqueAssetPath(CombineAssetPath(destinationFolder, fileName));

        if (!AssetDatabase.CopyAsset(sourcePath, destinationPath))
        {
            Debug.LogError($"Failed to copy asset:\n{sourcePath}\n-> {destinationPath}");
            copiedPath = null;
            return false;
        }

        var importer = AssetImporter.GetAtPath(destinationPath);
        if (importer != null)
        {
            importer.userData = BuildCopyMarker(sourcePath);
            importer.SaveAndReimport();
        }

        copiedPath = destinationPath;
        return true;
    }

    private static string FindExistingCopy(string sourcePath, string destinationFolder)
    {
        var marker = BuildCopyMarker(sourcePath);
        var assetGuids = AssetDatabase.FindAssets(string.Empty, new[] { destinationFolder });
        for (var i = 0; i < assetGuids.Length; i++)
        {
            var assetPath = AssetDatabase.GUIDToAssetPath(assetGuids[i]);
            var importer = AssetImporter.GetAtPath(assetPath);
            if (importer == null)
                continue;

            if (string.Equals(importer.userData, marker, StringComparison.Ordinal))
                return assetPath;
        }

        return null;
    }

    private static string BuildCopyMarker(string sourcePath)
    {
        return $"{CopyMarkerPrefix}{sourcePath}";
    }

    private static List<string> FindPrefabs(IEnumerable<string> roots)
    {
        return AssetDatabase.FindAssets("t:Prefab", roots.ToArray())
            .Select(AssetDatabase.GUIDToAssetPath)
            .Where(path => path.EndsWith(".prefab", StringComparison.OrdinalIgnoreCase))
            .Distinct()
            .OrderBy(path => path, StringComparer.Ordinal)
            .ToList();
    }

    private static bool IsTextureAsset(string path)
    {
        var extension = Path.GetExtension(path);
        return !string.IsNullOrEmpty(extension) && TextureExtensions.Contains(extension);
    }

    private static bool IsMeshAsset(string path)
    {
        if (path.EndsWith(".prefab", StringComparison.OrdinalIgnoreCase) ||
            path.EndsWith(".mat", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var assetsAtPath = AssetDatabase.LoadAllAssetsAtPath(path);
        for (var i = 0; i < assetsAtPath.Length; i++)
        {
            if (assetsAtPath[i] is Mesh)
                return true;
        }

        return false;
    }

    private static bool IsValidAssetsPath(string assetPath)
    {
        return !string.IsNullOrWhiteSpace(assetPath) &&
               (assetPath.Equals("Assets", StringComparison.Ordinal) || assetPath.StartsWith("Assets/", StringComparison.Ordinal));
    }

    private static string[] SplitRoots(string rootText)
    {
        return rootText
            .Split(new[] { '\r', '\n', ';', ',' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(path => path.Trim())
            .Where(IsValidAssetsPath)
            .Distinct(StringComparer.Ordinal)
            .ToArray();
    }

    private static string CombineAssetPath(string left, string right)
    {
        return $"{left.TrimEnd('/', '\\')}/{right.TrimStart('/', '\\')}";
    }

    private static void EnsureDirectory(string assetDirectoryPath)
    {
        if (string.IsNullOrEmpty(assetDirectoryPath))
            return;

        var projectRoot = Directory.GetParent(Application.dataPath)?.FullName;
        if (string.IsNullOrEmpty(projectRoot))
            return;

        var normalizedAssetPath = assetDirectoryPath.Replace('\\', '/');
        if (!normalizedAssetPath.StartsWith("Assets", StringComparison.Ordinal))
            return;

        var absolutePath = Path.Combine(projectRoot, normalizedAssetPath);
        if (!Directory.Exists(absolutePath))
            Directory.CreateDirectory(absolutePath);
    }
}
