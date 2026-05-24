using System;
using System.Collections.Generic;
using System.IO; 
using UnityEngine; 
using UnityEngine.Tilemaps;  
using System.Threading.Tasks;
#if UNITY_EDITOR
using UnityEditor;
#endif


public class MapTexture:MonoBehaviour
{
#if UNITY_EDITOR
    public List<Tilemap> tilemaps = new List<Tilemap>();
    public string path = "Assets/Texture/MapTexture";
    public Material blendMat;
    public bool isSingleMap;
    RenderTexture renderTexture;

    public void CreateTexture()
    {
        AsyncTaskRunner.Run(CreateTextureAsync, nameof(CreateTexture));
    }

    private async Task CreateTextureAsync()
    {
        Vector4[] offsets = new Vector4[tilemaps.Count];

        Texture2D mainTex = null;
        List<Texture2D> mainTextures = new List<Texture2D>();
        Vector2 mainCenter = Vector2.zero;

        List<List<Texture2D>> texture2Ds = new List<List<Texture2D>>();
        blendMat.SetTexture($"_Texture1", null);
        blendMat.SetTexture($"_Texture2", null);
        blendMat.SetTexture($"_Texture3", null);
        blendMat.SetTexture($"_Texture4", null);
        for (int i = 0; i < tilemaps.Count; i++)
        {
            tilemaps[i].ResizeBounds();
            if (i == 0)
            {
                mainTex = GetTexture(tilemaps[i], out mainCenter, out mainTextures);
            }
            else
            {
                var texture = GetTexture(tilemaps[i], out var center, out var textures);
                texture2Ds.Add(textures);
                Debug.Log($"center-{center}");
                Vector2 posOffset = tilemaps[i].transform.position;

                var offset = center - mainCenter + posOffset * 100.0f;
                var startSize = new Vector2(
                    mainTex.width * 0.5f - texture.width * 0.5f + offset.x,
                    mainTex.height * 0.5f - texture.height * 0.5f + offset.y);
                var endSize = new Vector2(
                    mainTex.width * 0.5f + texture.width * 0.5f + offset.x,
                    mainTex.height * 0.5f + texture.height * 0.5f + offset.y);
                Vector4 uvOffset = new Vector4
                {
                    x = startSize.x / mainTex.width,
                    y = startSize.y / mainTex.height,
                    z = endSize.x / mainTex.width,
                    w = endSize.y / mainTex.height
                };
                offsets[i - 1] = uvOffset;
                blendMat.SetTexture($"_Texture{i}", texture);
                //Debug.Log($"offset:{offset}--texture.size:{texture.width}-{texture.height}--startSize{startSize}--endSize{endSize}--offsets{i}-{uvOffset}");
            }
        }

        SpriteRenderer childRender = null;
        Transform child = tilemaps[0].transform.parent.Find(isSingleMap ? tilemaps[0].name : tilemaps[0].transform.parent.name);
        if (child != null)
        {
            childRender = child.GetComponent<SpriteRenderer>();
        }
        if (childRender == null)
        {
            GameObject childObj = new GameObject(isSingleMap ? tilemaps[0].name : tilemaps[0].transform.parent.name);
            childRender = childObj.AddComponent<SpriteRenderer>();
            childRender.sharedMaterial = tilemaps[0].GetComponent<TilemapRenderer>().sharedMaterial;
            childObj.transform.SetParent(tilemaps[0].transform.parent, false);
            childObj.transform.SetSiblingIndex(0);
            childObj.transform.localPosition = new Vector3(mainCenter.x * 0.01f, mainCenter.y * 0.01f, isSingleMap ? -100 : 200);
        }

        blendMat.SetInt("TextureCount", tilemaps.Count - 1);
        blendMat.SetVectorArray("texoffset", offsets);
        // CommandBuffer commandBuffer = new CommandBuffer();
        RenderTexture.ReleaseTemporary(renderTexture);
        renderTexture = RenderTexture.GetTemporary(mainTex.width, mainTex.height, 0);
        Graphics.Blit(mainTex, renderTexture, blendMat, 0);
        Texture2D texture2D = new Texture2D(mainTex.width, mainTex.height, TextureFormat.BGRA32, false);

        RenderTexture.active = renderTexture;
        texture2D.ReadPixels(new Rect(0, 0, mainTex.width, mainTex.height), 0, 0);
        texture2D.Apply();
        SaveFileTexture1(path, texture2D, isSingleMap ? $"{tilemaps[0].transform.parent.name}-{tilemaps[0].name}" : tilemaps[0].transform.parent.name);
        //rawImage.texture = texture2D; 

        for (int i = 0; i < mainTextures.Count; i++)
        {
            blendMat.SetTexture($"_Texture1", null);
            blendMat.SetTexture($"_Texture2", null);
            blendMat.SetTexture($"_Texture3", null);
            blendMat.SetTexture($"_Texture4", null);
            blendMat.SetInt("TextureCount", tilemaps.Count - 1);
            blendMat.SetVectorArray("texoffset", offsets);
            RenderTexture renderTexture = RenderTexture.GetTemporary(mainTex.width, mainTex.height, 0);

            for (int index = 0; index < texture2Ds.Count; index++)
            {
                blendMat.SetTexture($"_Texture{index + 1}", texture2Ds[index][i]);
            }
            blendMat.mainTexture = mainTextures[i];
            //blendMat.SetVectorArray("texoffset", offsets);

            Graphics.Blit(mainTextures[i], renderTexture, blendMat, 0);
            Texture2D texture2D1 = new Texture2D(mainTex.width, mainTex.height, TextureFormat.BGRA32, false);

            RenderTexture.active = renderTexture;
            texture2D1.ReadPixels(new Rect(0, 0, mainTex.width, mainTex.height), 0, 0);
            texture2D1.Apply();
            SaveFileTexture1(path, texture2D1, isSingleMap ? $"{tilemaps[0].transform.parent.name}-{tilemaps[0].name}{mainTextures[i].name}" : $"{tilemaps[0].transform.parent.name}{mainTextures[i].name}");
            RenderTexture.ReleaseTemporary(renderTexture);
        }
        RenderTexture.ReleaseTemporary(renderTexture);
        AssetDatabase.Refresh();

        for (int i = 0; i < secondaryNames.Count; i++)
        {
            TextureImporter textureImporter = (TextureImporter)TextureImporter.GetAtPath(
                isSingleMap ? $"{path}/{tilemaps[0].transform.parent.name}-{tilemaps[0].name}{secondaryNames[i]}.png" : $"{path}/{tilemaps[0].transform.parent.name}{secondaryNames[i]}.png");
            textureImporter.mipmapEnabled = false;
            textureImporter.filterMode = FilterMode.Point;
            textureImporter.compressionQuality = 50;
            textureImporter.textureCompression = TextureImporterCompression.Compressed;
            textureImporter.crunchedCompression = true;
            textureImporter.alphaIsTransparency = true;
            textureImporter.mipmapEnabled = false;
            if (secondaryNames[i] == "_NormalMap")
            {
                textureImporter.textureType = TextureImporterType.NormalMap;
            }
            else
            {
                textureImporter.textureType = TextureImporterType.Default;
            }
            textureImporter.SaveAndReimport();
            AssetDatabase.Refresh();
        }
        TextureImporter mainTextImoorter = (TextureImporter)TextureImporter.GetAtPath(isSingleMap ? $"{path}/{tilemaps[0].transform.parent.name}-{tilemaps[0].name}.png"
            : $"{path}/{tilemaps[0].transform.parent.name}.png");
        mainTextImoorter.mipmapEnabled = false;
        mainTextImoorter.filterMode = FilterMode.Point;
        mainTextImoorter.compressionQuality = 50;
        mainTextImoorter.textureCompression = TextureImporterCompression.Compressed;
        mainTextImoorter.crunchedCompression = true;
        mainTextImoorter.alphaIsTransparency = true;
        mainTextImoorter.textureType = TextureImporterType.Sprite;
        mainTextImoorter.spriteImportMode = SpriteImportMode.Single;

        SecondarySpriteTexture[] secondarySpriteTextures = new SecondarySpriteTexture[secondaryNames.Count];
        for (int i = 0; i < secondaryNames.Count; i++)
        {
            string sourcePath = isSingleMap ? $"{path}/{tilemaps[0].transform.parent.name}-{tilemaps[0].name}{secondaryNames[i]}" +
                $".png" : $"{path}/{tilemaps[0].transform.parent.name}{secondaryNames[i]}.png";
            Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(sourcePath);
            SecondarySpriteTexture secondarySpriteTexture = new SecondarySpriteTexture
            {
                name = secondaryNames[i],
                texture = tex
            };
            secondarySpriteTextures[i] = secondarySpriteTexture;
        }
        mainTextImoorter.secondarySpriteTextures = secondarySpriteTextures;
        mainTextImoorter.SaveAndReimport();
        AssetDatabase.Refresh();

        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(isSingleMap ? $"{path}/{tilemaps[0].transform.parent.name}-{tilemaps[0].name}.png" : $"{path}/{tilemaps[0].transform.parent.name}.png");
        childRender.sprite = sprite;

    }
    public void Release()
    {
        RenderTexture.ReleaseTemporary(renderTexture);
    }
    Dictionary<Sprite, Color[]> spriteColorDic = new Dictionary<Sprite, Color[]>();

    public Vector2 GetTextureData(Tilemap tilemap, out Color[] textureColors, out Vector2Int size)
    {
        size = new Vector2Int(Mathf.RoundToInt(tilemap.cellSize.x * 100), Mathf.RoundToInt(tilemap.cellSize.y * 100));
        Vector2Int TexSize = new Vector2Int(Mathf.RoundToInt(tilemap.cellSize.x * 100 * tilemap.size.x), Mathf.RoundToInt(tilemap.cellSize.y * 100 * tilemap.size.y));
        textureColors = new Color[TexSize.x * TexSize.y];

        Vector2 offset = new Vector2(Mathf.RoundToInt(tilemap.size.x * tilemap.cellSize.x * 50),
            Mathf.RoundToInt(tilemap.size.y * tilemap.cellSize.y * 50));
        return offset;
        //SaveFileTexture(path, texture, mapName+ tilemap.name);
    }
    public List<string> secondaryNames = new List<string>
    {
        "_NormalMap","_MoveMask","_SnowTex"
    };
    public Texture2D GetTexture(Tilemap tilemap, out Vector2 center, out List<Texture2D> Textures)
    {
        Vector2Int size = new Vector2Int(Mathf.RoundToInt(tilemap.cellSize.x * 100), Mathf.RoundToInt(tilemap.cellSize.y * 100));

        Texture2D texture = new Texture2D(Mathf.RoundToInt(tilemap.cellSize.x * 100 * tilemap.size.x),
            Mathf.RoundToInt(tilemap.cellSize.y * 100 * tilemap.size.y), TextureFormat.RGBA32, false);
        Color[] textureColors = new Color[texture.width * texture.height];
        texture.SetPixels(textureColors);

        Textures = new List<Texture2D>();
        for (int i = 0; i < secondaryNames.Count; i++)
        {
            Texture2D _texture = new Texture2D(Mathf.RoundToInt(tilemap.cellSize.x * 100 * tilemap.size.x),
           Mathf.RoundToInt(tilemap.cellSize.y * 100 * tilemap.size.y), TextureFormat.RGBA32, false);
            _texture.SetPixels(textureColors);
            _texture.name = secondaryNames[i];
            Textures.Add(_texture);
        }


        for (int x = tilemap.cellBounds.xMin; x <= tilemap.cellBounds.xMax; x++)
        {
            for (int y = tilemap.cellBounds.yMin; y <= tilemap.cellBounds.yMax; y++)
            {
                var sprite = tilemap.GetSprite(new Vector3Int(x, y, 0));

               

                if (sprite != null)
                {
                    if (!sprite.texture.isReadable)
                    {
                        AssetImporter assetImporter = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(sprite.texture));
                        TextureImporter textureImporter = (TextureImporter)assetImporter;
                        textureImporter.isReadable = true;
                        textureImporter.filterMode = FilterMode.Point;
                        textureImporter.textureCompression = TextureImporterCompression.Uncompressed;
                        textureImporter.SaveAndReimport();
                        AssetDatabase.Refresh();
                    }
                    if (!spriteColorDic.TryGetValue(sprite, out var spriteColor))
                    {
                        try
                        {
                            spriteColor = sprite.texture.GetPixels(Mathf.RoundToInt(sprite.rect.x), Mathf.RoundToInt(sprite.rect.y),
                                  Mathf.RoundToInt(sprite.rect.width), Mathf.RoundToInt(sprite.rect.height));
                        }
                        catch
                        {
                            Debug.LogError($"sprite:{sprite.texture.name}");
                        }

                    }
                    int startX = Mathf.RoundToInt((x - tilemap.cellBounds.xMin) * tilemap.cellSize.x * 100);
                    int startY = Mathf.RoundToInt((y - tilemap.cellBounds.yMin) * tilemap.cellSize.y * 100);
                    texture.SetPixels(startX, startY, (int)sprite.rect.width, (int)sprite.rect.height, spriteColor);

                    SecondarySpriteTexture[] secondaryTextures = new SecondarySpriteTexture[5];
                    int secondaryCout = sprite.GetSecondaryTextures(secondaryTextures);
                    for (int i = 0; i < secondaryCout; i++)
                    {
                        var secondaryTexture = secondaryTextures[i];
                        int index = secondaryNames.FindIndex(s => s == secondaryTexture.name);
                        if (index >= 0)
                        {
                            try
                            {
                                var colors = secondaryTexture.texture.GetPixels(Mathf.RoundToInt(sprite.rect.x), Mathf.RoundToInt(sprite.rect.y),
                                  Mathf.RoundToInt(sprite.rect.width), Mathf.RoundToInt(sprite.rect.height));
                                if (index < 2)
                                {
                                    for (int j = 0; j < colors.Length; j++)
                                    {
                                        var color = colors[j];

                                        color.a = spriteColor[j].a;
                                        colors[j] = color;
                                    }
                                }
                                Textures[index].SetPixels(startX, startY, (int)sprite.rect.width, (int)sprite.rect.height, colors);
                            }
                            catch
                            {
                                Color[] colors = new Color[spriteColor.Length];
                                if (index < 2)
                                {
                                    Color _color = secondaryTexture.texture.GetPixel(0, 0);
                                    for (int j = 0; j < colors.Length; j++)
                                    {
                                        var color = colors[j];
                                        color = _color;
                                        color.a = spriteColor[j].a;
                                        colors[j] = color;
                                    }
                                }
                                Textures[index].SetPixels(startX, startY, (int)sprite.rect.width, (int)sprite.rect.height, colors);
                            }


                        }
                    }
                }
            }
        }
        for (int i = 0; i < Textures.Count; i++)
        {
            Textures[i].Apply();
            //SaveFileTexture(path, Textures[i], $"{tilemap.name}{Textures[i].name}");
        }
        center = new Vector2(Mathf.RoundToInt(tilemap.cellSize.x * tilemap.cellBounds.center.x * 100),
            Mathf.RoundToInt(tilemap.cellSize.y * tilemap.cellBounds.center.y * 100));

        texture.Apply();
        //SaveFileTexture(path, texture, mapName + tilemap.name);
        return texture;
        //
    }
    private static void SaveFileTexture1(string outPath, Texture2D texture, string name)
    {
        byte[] dataBytes = texture.EncodeToPNG();
        if (!Directory.Exists(outPath))
        {
            Directory.CreateDirectory(outPath);
        }
        string strSaveFile = outPath + "/" + name + ".png";
        if (File.Exists(strSaveFile))
        {
            File.Delete(strSaveFile);
        }
        using (FileStream fs = File.Open(strSaveFile, FileMode.CreateNew))
        {
            fs.Seek(0, SeekOrigin.End);
            fs.Write(dataBytes, 0, dataBytes.Length);
        }
    }
    private static async Task SaveFileTexture(string outPath, Texture2D texture, string name)
    {
        byte[] dataBytes = texture.EncodeToPNG();
        if (!Directory.Exists(outPath))
        {
            Directory.CreateDirectory(outPath);
        }
        string strSaveFile = outPath + "/" + name + ".png";
        if (File.Exists(strSaveFile))
        {
            File.Delete(strSaveFile);
        }
        using (FileStream fs = File.Open(strSaveFile, FileMode.CreateNew))
        {
            fs.Seek(0, SeekOrigin.End);
            await fs.WriteAsync(dataBytes, 0, dataBytes.Length);
        }
    }
#endif

}

#if UNITY_EDITOR
[CustomEditor(typeof(MapTexture))] 
public class MapTextureEditor : Editor
{
    public MapTexture mapTexture => target as MapTexture;
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (GUILayout.Button("Test"))
        {
            mapTexture.CreateTexture();
        }
        if (GUILayout.Button("Release"))
        {
            mapTexture.Release();
        }
    }
}
#endif
