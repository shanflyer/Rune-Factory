using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

public enum sxx
{
}

public class SourceTool : MonoBehaviour
{
    [MenuItem("Assets/音效工具/刷新音效资源数据")]
    public static void RefreshAudioSourceEnum()
    {
        Dictionary<string, List<string>> sources = new Dictionary<string, List<string>>();
        try
        {
            AssetDatabase.StartAssetEditing();
            foreach (var obj in Selection.GetFiltered<Object>(SelectionMode.Assets))
            {
                var path = AssetDatabase.GetAssetPath(obj);
                if (obj is AudioClip)
                {
                    var strs = path.Split('/');
                    var title = strs[strs.Length - 2];

                    if (!sources.TryGetValue(title, out List<string> list))
                    {
                        list = new List<string>();
                        sources[title] = list;
                    }
                    list.Add(strs[strs.Length - 1].Split('.')[0]);
                }

                if (string.IsNullOrEmpty(path))
                    continue;

                if (System.IO.Directory.Exists(path))
                {
                    DirectoryInfo dir = new DirectoryInfo(path);
                    OpenDirectoryInfo(dir, path);
                }
            }
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
        }

        void OpenDirectoryInfo(DirectoryInfo directoryInfo, string parentPath)
        {
            var _dirs = directoryInfo.GetDirectories();
            var files = directoryInfo.GetFiles("*.mp3");

            foreach (var f in files)
            {
                try
                {
                    var title = directoryInfo.Name;

                    if (!sources.TryGetValue(title, out List<string> list))
                    {
                        list = new List<string>();
                        sources[title] = list;
                    }
                    list.Add(f.Name.Split('.')[0]);
                }
                catch
                {
                }
            }
            foreach (var d in _dirs)
            {
                string path = parentPath + "/" + d.Name;
                OpenDirectoryInfo(d, path);
            }
        }

        string property = "";

        foreach (var source in sources)
        {
            property = $"{property}\n{"public enum "}{source.Key}{"\r\n{\r\n  "}";

            foreach (var d in source.Value)
            {
                property = $"{property}{d},";
            }
            property = $"{property}{"\r\n}"}";
        }
        File.WriteAllText("Assets/Scripts/Audio/AudioSource.cs", property);

        AssetDatabase.ImportAsset("Assets/Scripts/Audio/AudioSource.cs");
    }

    [MenuItem("Assets/数据/引用数据刷新")]
    public static void SetGameDataSerializeObj()
    {
        Object[] selection = Selection.GetFiltered(typeof(Object), SelectionMode.DeepAssets);
        string[] resources = new string[selection.Length];

        try
        {
            AssetDatabase.StartAssetEditing();
            for (int i = 0; i < selection.Length; i++)
            {
                resources[i] = AssetDatabase.GetAssetPath(selection[i]);
                IGameData gameData = selection[i] as IGameData;
                if (gameData != null)
                {
                    gameData.SetReferenceData();
                    EditorUtility.SetDirty(selection[i]);
                    AssetDatabase.SaveAssets();
                }
            }
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
        }
    }

    [MenuItem("Assets/输出精灵资源")]
    public static void OutSpriteSource()
    {
        foreach (var obj in Selection.GetFiltered<Object>(SelectionMode.Assets))
        {
            var path = AssetDatabase.GetAssetPath(obj);
            if (obj)
            {
                var strs = path.Split('.');
                if (strs[strs.Length - 1] == "png")
                {
                    OutSprite(path);
                }
            }

            if (string.IsNullOrEmpty(path))
                continue;
        }
    }

    [MenuItem("Assets/输出精灵资源X2")]
    public static void OutSpriteSource2()
    {
        foreach (var obj in Selection.GetFiltered<Object>(SelectionMode.Assets))
        {
            var path = AssetDatabase.GetAssetPath(obj);
            if (obj)
            {
                var strs = path.Split('.');
                if (strs[strs.Length - 1] == "png")
                {
                    OutSprite(path, 2);
                }
            }

            if (string.IsNullOrEmpty(path))
                continue;
        }
    }

    private static void OutSprite(string path, int scale = 1)
    {
        var sources = AssetDatabase.LoadAllAssetsAtPath(path);
        foreach (var source in sources)
        {
            if (source.GetType().Name == "Sprite")
            {
                Sprite sprite = (Sprite)source;
                SaveTexture(sprite, scale);
            }
        }
    }

    private static void SaveTexture(Sprite sprite, int scale = 1)
    {
        int width = Mathf.RoundToInt(sprite.rect.width * scale);
        int height = Mathf.RoundToInt(sprite.rect.height * scale);
        Texture2D texture2D = new Texture2D(width, height);

        int x0 = 0;
        int y0 = 0;
        int x1 = width;
        int y1 = height;

        List<Color> defaultColors = new List<Color>();
        for (int i = 0; i < x1 * y1; i++)
        {
            defaultColors.Add(new Color(0, 0, 0, 0));
        }
        texture2D.SetPixels(defaultColors.ToArray());

        Color[] SpriteColors = sprite.texture.GetPixels(Mathf.RoundToInt(sprite.rect.x), Mathf.RoundToInt(sprite.rect.y),
            Mathf.RoundToInt(sprite.rect.width), Mathf.RoundToInt(sprite.rect.height));
        //Color[] SpriteColors = sprite.texture.GetPixels(4, 4, 18, 52);

        Color[] outColor = new Color[SpriteColors.Length * scale * scale];

        if (scale > 1)
        {
            for (int i = 0; i < SpriteColors.Length; i++)
            {
                int raw = i % Mathf.RoundToInt(sprite.rect.width) * scale;
                int col = i / Mathf.RoundToInt(sprite.rect.width) * scale;

                for (int j = 0; j < scale; j++)
                {
                    int index = col * width + j + raw;
                    outColor[index] = SpriteColors[i];
                    int index1 = col * width + width + j + raw;
                    outColor[index1] = SpriteColors[i];

                }
            }
        }
        else
        {
            outColor = SpriteColors;
        }
       

        texture2D.SetPixels(x0, y0, width, height, outColor.ToArray());

        string dir = "OutTexture";

        SaveFileTexture(dir, texture2D, sprite.name);
    }

    [MenuItem("Assets/输出植物精灵资源")]
    public static void OutFarmSpriteSource()
    {
        foreach (var obj in Selection.GetFiltered<Object>(SelectionMode.Assets))
        {
            var path = AssetDatabase.GetAssetPath(obj);
            if (obj)
            {
                var strs = path.Split('.');
                if (strs[strs.Length - 1] == "png")
                {
                    OutFarmSprite(path);
                }
            }

            if (string.IsNullOrEmpty(path))
                continue;
        }
    }
    private static void OutFarmSprite(string path)
    {
        var sources = AssetDatabase.LoadAllAssetsAtPath(path);
        foreach (var source in sources)
        {
            if (source.GetType().Name == "Sprite")
            {
                Sprite sprite = (Sprite)source;
                SaveSpriteTexture(sprite);
            }
        }
    }
    static void SaveSpriteTexture(Sprite sprite, int sizeX=32,int sizeY=64)
    {
        Texture2D texture2D = new Texture2D(sizeX, sizeY);
        Vector2Int pivot = Vector2Int.RoundToInt(sprite.pivot);


        int x0 = sizeX / 2 - pivot.x;
        int y0 = 0;
        int x1 = sizeX / 2 + Mathf.RoundToInt(sprite.rect.width) - pivot.x;
        int y1 =  Mathf.RoundToInt(sprite.rect.height);

        List<Color> defaultColors = new List<Color>();
        for (int i = 0; i < sizeX * sizeY; i++)
        {
            defaultColors.Add(new Color(0, 0, 0, 0));
        }
        texture2D.SetPixels(defaultColors.ToArray());

        Color[] SpriteColors = sprite.texture.GetPixels(Mathf.RoundToInt(sprite.rect.x), Mathf.RoundToInt(sprite.rect.y),
            Mathf.RoundToInt(sprite.rect.width), Mathf.RoundToInt(sprite.rect.height));
        //Color[] SpriteColors = sprite.texture.GetPixels(4, 4, 18, 52);

        List<Color> colors = new List<Color>();
        for (int y = 0; y < sizeY; y++)
        {
            for (int x = 0; x < sizeX; x++)
            {
                if (x >= x0 - 1 && x <= x1 - 1 && y >= y0 - 1 && y <= y1 - 1)
                {

                }
                else
                {
                    colors.Add(new Color(0, 0, 0, 0));
                }
            }
        }
        texture2D.SetPixels(x0, y0, (int)sprite.rect.width, (int)sprite.rect.height, SpriteColors);
         

        string dir = "OutTexture";


        SaveFileTexture(dir, texture2D, sprite.name);
    }

    private static async void SaveFileTexture(string outPath, Texture2D texture, string name)
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
}