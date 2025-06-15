using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;
using System.Collections.Generic;

public class PsbNormalAtlasGenerator_Exact : EditorWindow
{
    private Object originalPsb;
    private Object normalPsb;

    [MenuItem("Tools/Generate Normal Atlas from PSB UV")]
    public static void ShowWindow()
    {
        GetWindow<PsbNormalAtlasGenerator_Exact>("Generate Normal Atlas");
    }

    void OnGUI()
    {
        GUILayout.Label("Select PSB Files", EditorStyles.boldLabel);
        originalPsb = EditorGUILayout.ObjectField("Original PSB File", originalPsb, typeof(Object), false);
        normalPsb = EditorGUILayout.ObjectField("Normal PSB File", normalPsb, typeof(Object), false);

        if (GUILayout.Button("Generate"))
        {
            if (originalPsb == null || normalPsb == null)
            {
                EditorUtility.DisplayDialog("Error", "Please select both PSB files.", "OK");
                return;
            }

            string originalPath = AssetDatabase.GetAssetPath(originalPsb);
            string normalPath = AssetDatabase.GetAssetPath(normalPsb);

            GenerateAtlas(originalPath, normalPath);
        }
    }

    void GenerateAtlas(string originalPsbPath, string normalPsbPath)
    {
        var originalSprites = LoadSpriteMapFromPsb(originalPsbPath);
        var normalSprites = LoadSpriteMapFromPsb(normalPsbPath);

        if (originalSprites.Count == 0 || normalSprites.Count == 0)
        {
            Debug.LogError("Sprite maps are empty.");
            return;
        }

        // 使用原始图的图集尺寸
        Texture2D originalTexture = originalSprites.Values.First().texture;
        int atlasWidth = originalTexture.width;
        int atlasHeight = originalTexture.height;

        Texture2D resultAtlas = new Texture2D(atlasWidth, atlasHeight, TextureFormat.RGBA32, false);
        resultAtlas.SetPixels32(new Color32[atlasWidth * atlasHeight]);

        int copied = 0;

        foreach (var kvp in originalSprites)
        {
            string path = kvp.Key;
            Sprite oriSprite = kvp.Value;

            if (!normalSprites.TryGetValue(path, out Sprite normalSprite))
            {
                Debug.LogWarning($"[Skip] No normal sprite for path: {path}");
                continue;
            }

            Rect oriRect = oriSprite.rect;
            Rect normRect = normalSprite.rect;

            int srcX = Mathf.FloorToInt(normRect.x);
            int srcY = Mathf.FloorToInt(normRect.y);
            int w = Mathf.FloorToInt(normRect.width);
            int h = Mathf.FloorToInt(normRect.height);

            Color[] pixels = normalSprite.texture.GetPixels(srcX, srcY, w, h);

            int dstX = Mathf.FloorToInt(oriRect.x);
            int dstY = Mathf.FloorToInt(oriRect.y);

            resultAtlas.SetPixels(dstX, dstY, w, h, pixels);
            copied++;
        }

        resultAtlas.Apply();

        string outputPath = $"Assets/Texture/NewNormal/{originalTexture.name.Split('.')[0]}_normal.png";
        File.WriteAllBytes(outputPath, resultAtlas.EncodeToPNG());
        AssetDatabase.Refresh();

       // Debug.Log($"✅ Generated Normal Atlas. Sprites copied: {copied}. Saved to: {outputPath}");
    }

    Dictionary<string, Sprite> LoadSpriteMapFromPsb(string psbPath)
    {
        var map = new Dictionary<string, Sprite>();

        Object[] assets = AssetDatabase.LoadAllAssetsAtPath(psbPath);
        GameObject prefabRoot = assets.FirstOrDefault(a => a is GameObject) as GameObject;
        Sprite[] sprites = assets.OfType<Sprite>().ToArray();

        if (prefabRoot == null || sprites.Length == 0)
        {
            Debug.LogError($"Cannot parse PSB: {psbPath}");
            return map;
        }

        void Traverse(Transform t, string path)
        {
            var sr = t.GetComponent<SpriteRenderer>();
            if (sr != null && sr.sprite != null)
            {
                if (!map.ContainsKey(path))
                    map.Add(path, sr.sprite);
                else
                    Debug.LogWarning($"Duplicate sprite path: {path}");
            }

            foreach (Transform child in t)
                Traverse(child, $"{path}/{child.name}");
        }

        Traverse(prefabRoot.transform, prefabRoot.name);
        return map;
    }
}
