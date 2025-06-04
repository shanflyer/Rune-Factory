using OfficeOpenXml;
using System.Collections.Generic;
using System.IO;
using TexturePackerImporter;
using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEngine;

public class CommonTool : MonoBehaviour
{
    [MenuItem("Assets/数据/刷新DataDic")]
    public static void InitDataDic()
    {
        try
        {
            AssetDatabase.StartAssetEditing();
            var activeObject = Selection.activeObject;
            if (activeObject)
            {
                MyDataDIc myDataDIc = ScriptableObject.CreateInstance<MyDataDIc>();

                var path = AssetDatabase.GetAssetPath(activeObject);
                if (File.Exists(path))
                {
                    var data = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
                    IGameData gameData = data as IGameData;
                    if (gameData.GetDataDic() != null)
                    {
                        SaveExcel(gameData.GetDataDic(), gameData.GetKey());
                        return;
                    }
                }
                else
                {
                    DirectoryInfo directoryInfo = new DirectoryInfo(path);

                    var files = directoryInfo.GetFiles("*.asset");
                    foreach (var file in files)
                    {
                        var data = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path + "/" + file.Name);
                        IGameData gameData = data as IGameData;
                        if (gameData.GetDataDic() != null)
                        {
                            SaveExcel(gameData.GetDataDic(), gameData.GetKey());
                        }
                        else
                        {
                            myDataDIc.daraDic.Add(gameData.GetKey(), gameData.GetName());
                        }
                    }
                    if (myDataDIc.daraDic.Count > 0)
                    {
                        SaveExcel(myDataDIc.daraDic, activeObject.name);
                    }
                    // AssetDatabase.CreateAsset(myDataDIc, $"Assets/Editor/DataDic/{activeObject.name}.asset");
                }
            }
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
        }
    }
    [MenuItem("Assets/复制psb资源")]
    public static void ExtractPSB()
    {
        try
        {
           // AssetDatabase.StartAssetEditing();
            foreach (var obj in Selection.GetFiltered<Object>(SelectionMode.Assets))
            {
                var psbPath = AssetDatabase.GetAssetPath(obj);
                if (obj)
                {
                    var strs = psbPath.Split('.');
                    if (strs[strs.Length - 1] == "psb")
                    {
                        try
                        { 

                            // 载入PSB的主资源
                            Object[] assets = AssetDatabase.LoadAllAssetsAtPath(psbPath);
                            Texture2D texture = null;
                            var sprites = new System.Collections.Generic.List<Sprite>();

                            foreach (var asset in assets)
                            {
                                if (asset is Texture2D tex)
                                {
                                    texture = tex;
                                }
                                else if (asset is Sprite sprite)
                                {
                                    sprites.Add(sprite);
                                }
                            }

                            if (texture == null || sprites.Count == 0)
                            {
                                Debug.LogError("没有找到有效的Texture或Sprite");
                                return;
                            }

                            // 保存Texture为PNG
                            string outputFolder = EditorUtility.SaveFolderPanel("选择输出文件夹", "Assets", "");
                            if (string.IsNullOrEmpty(outputFolder))
                                return;

                            // 转相对路径
                            outputFolder = "Assets" + outputFolder.Substring(Application.dataPath.Length);

                            string pngPath = Path.Combine(outputFolder, Path.GetFileNameWithoutExtension(psbPath) + ".png");
                            byte[] pngData = texture.EncodeToPNG();
                            File.WriteAllBytes(pngPath, pngData);
                            AssetDatabase.ImportAsset(pngPath, ImportAssetOptions.ForceUpdate);

                            // 设置TextureImporter属性
                            TextureImporter importer = AssetImporter.GetAtPath(pngPath) as TextureImporter;
                            importer.textureType = TextureImporterType.Sprite;
                            importer.spriteImportMode = SpriteImportMode.Multiple;
                            importer.alphaIsTransparency = true; 
                            importer.isReadable = true;
                            importer.filterMode = FilterMode.Point;
                            importer.textureCompression = TextureImporterCompression.Uncompressed;
                            importer.spritePixelsPerUnit = 100;
                            importer.mipmapEnabled = false;

                            EditorUtility.SetDirty(importer);
                            AssetDatabase.ImportAsset(pngPath, ImportAssetOptions.ForceUpdate);

                            UpdateSprites(importer, sprites);

                            Debug.Log("提取完成：" + pngPath);
                        }
                        catch
                        {
                        }
                        //OutPSBFile(path);
                    }
                }

                
            }
        }
        finally
        {
           // AssetDatabase.StopAssetEditing();
        }
       
    }
    public static GameObject CopyPSBObj(string psbPath, string outputFolder)
    {
        // 载入PSB的主资源
        Object[] assets = AssetDatabase.LoadAllAssetsAtPath(psbPath);
        Texture2D texture = null; 
        GameObject obj = null;
        foreach (var asset in assets)
        {  if (obj == null && asset is GameObject pre)
            {
                obj = pre;
                break;
            }
        }

        return obj;
        //Debug.Log("提取完成：" + pngPath);
    }
    public static (string,GameObject) CopyPSBSource(string psbPath, string outputFolder)
    {
        // 载入PSB的主资源
        Object[] assets = AssetDatabase.LoadAllAssetsAtPath(psbPath);
        Texture2D texture = null;
        var sprites = new System.Collections.Generic.List<Sprite>();
        GameObject obj = null;
        foreach (var asset in assets)
        {
            if (asset is Texture2D tex)
            {
                texture = tex;
            }
            else if (asset is Sprite sprite)
            {
                sprites.Add(sprite);
            }else if(obj==null&&asset is GameObject pre)
            {
                obj = pre;
            }
        }

        if (texture == null || sprites.Count == 0)
        {
            Debug.LogError("没有找到有效的Texture或Sprite");
            return (null,null);
        }

        
         

        string pngPath =$"{outputFolder}/{Path.GetFileNameWithoutExtension(psbPath)}.png";
        byte[] pngData = texture.EncodeToPNG();
        File.WriteAllBytes(pngPath, pngData);
        AssetDatabase.ImportAsset(pngPath, ImportAssetOptions.ForceUpdate);

        // 设置TextureImporter属性
        TextureImporter importer = AssetImporter.GetAtPath(pngPath) as TextureImporter;
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Multiple;
        importer.alphaIsTransparency = true;
        importer.isReadable = true;
        importer.filterMode = FilterMode.Point;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.spritePixelsPerUnit = 100;
        importer.mipmapEnabled = false;

        EditorUtility.SetDirty(importer);
        AssetDatabase.ImportAsset(pngPath, ImportAssetOptions.ForceUpdate);

        UpdateSprites(importer, sprites);

        return (pngPath,obj);
        //Debug.Log("提取完成：" + pngPath);
    }

    static void UpdateSprites(TextureImporter importer, List<Sprite> sprites)
    {
        var dataProvider = GetSpriteEditorDataProvider(importer);
        var spriteNameFileIdDataProvider = dataProvider.GetDataProvider<ISpriteNameFileIdDataProvider>();

        var oldIds = spriteNameFileIdDataProvider.GetNameFileIdPairs();

        SpriteRect[] rects = SheetInfoToSpriteRects(sprites);
        SpriteNameFileIdPair[] ids = GenerateSpriteIds(oldIds, rects);

        dataProvider.SetSpriteRects(rects);
        spriteNameFileIdDataProvider.SetNameFileIdPairs(ids);
        dataProvider.Apply();
        EditorUtility.SetDirty(importer);
        importer.SaveAndReimport();
    }

    static ISpriteEditorDataProvider GetSpriteEditorDataProvider(TextureImporter importer)
    {
        var dataProviderFactories = new SpriteDataProviderFactories();
        dataProviderFactories.Init();
        var dataProvider = dataProviderFactories.GetSpriteEditorDataProviderFromObject(importer);
        dataProvider.InitSpriteEditorDataProvider();
        return dataProvider;
    }

    static SpriteRect[] SheetInfoToSpriteRects(List<Sprite> sprites)
    {
        int spriteCount = sprites.Count;
        SpriteRect[] rects = new SpriteRect[spriteCount];

        for (int i = 0; i < spriteCount; i++)
        {
            Sprite sprite = sprites[i];
            rects[i] = new SpriteRect
            {
                name = sprite.name,
                rect = sprite.rect,
                pivot = GetNormalizedPivot(sprite),
                alignment = SpriteAlignment.Custom,
                border = sprite.border
            };
        }

        return rects;
    }

    static SpriteNameFileIdPair[] GenerateSpriteIds(IEnumerable<SpriteNameFileIdPair> oldIds, SpriteRect[] sprites)
    {
        SpriteNameFileIdPair[] newIds = new SpriteNameFileIdPair[sprites.Length];

        for (int i = 0; i < sprites.Length; i++)
        {
            sprites[i].spriteID = IdForName(oldIds, sprites[i].name);
            newIds[i] = new SpriteNameFileIdPair(sprites[i].name, sprites[i].spriteID);
        }

        return newIds;
    }

    static GUID IdForName(IEnumerable<SpriteNameFileIdPair> oldIds, string name)
    {
        foreach (SpriteNameFileIdPair old in oldIds)
        {
            if (old.name == name)
            {
                return old.GetFileGUID();
            }
        }
        return GUID.Generate();
    }

    static Vector2 GetNormalizedPivot(Sprite sprite)
    {
        var rect = sprite.rect;
        var pivot = sprite.pivot;
        return new Vector2(pivot.x / rect.width, pivot.y / rect.height);
    }

    private const string excelPath = "Assets/Editor/DataDic/Data.xlsx";
    [MenuItem("Assets/输出psb")]
    private static void ExportSelectedPsbAtlas()
    {
        // 1. 先弹出文件夹选择对话框，让用户选择输出路径
        string outputFolder = EditorUtility.OpenFolderPanel("Select Output Folder for PSB Sprites", "", "");
        if (string.IsNullOrEmpty(outputFolder))
        {
            Debug.LogWarning("导出已取消：未选择文件夹。");
            return;
        }

        // 2. 获取当前在 Project 视图中选中的唯一一个对象（期望选中 PSB 导入后对应的 Texture2D 资源）
        Object selected = Selection.activeObject;
        if (selected == null)
        {
            Debug.LogError("请先在 Project 视图中选中一个 PSB 导入后的 Texture2D 资源，再执行导出。");
            return;
        }

        // 把选中的对象转换为纹理路径
        string assetPath = AssetDatabase.GetAssetPath(selected);
        if (string.IsNullOrEmpty(assetPath))
        {
            Debug.LogError("无法获取选中资源路径，请确认已正确选中 PSB 对应的 Texture2D。");
            return;
        }

        // 确保所选资源是一个 Texture2D
        Texture2D sourceTex = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
        if (sourceTex == null)
        {
            Debug.LogError("所选资源不是 Texture2D 类型，请选中 PSB 导入后生成的 Texture2D。");
            return;
        }

        // 3. 获取这个 Texture2D 在同一路径下所有子资源，其中包含多个 Sprite
        Object[] assets = AssetDatabase.LoadAllAssetsAtPath(assetPath);
        Texture texture=null;
        foreach (var obj in assets)
        {
            if (obj is Texture sp)
            {
                texture = sp;
                break;
            }
        }
         
       
         

      
        int width =texture.width;
        int height = texture.height;
        int x = 0;
        int y = 0;

        // 从源贴图中读取像素（需要确保可读）
        try
        {
            // 在 Unity 2019+ 版本：Sprite.texture.GetPixels(x, y, w, h)
            Color[] pixels = sourceTex.GetPixels(x, y, width, height);

            // 创建一个新的可读写 Texture2D
            Texture2D newTex = new Texture2D(width, height, TextureFormat.ARGB32, false);
            newTex.SetPixels(pixels);
            newTex.Apply();

            // 将新贴图编码为 PNG
            byte[] pngData = newTex.EncodeToPNG();
            Object.DestroyImmediate(newTex);

            // 生成输出文件名：PSB 文件名 + “_” + 子 Sprite 名称 + “.png”
            string psbFileName = Path.GetFileNameWithoutExtension(assetPath);
            // 有些 Sprite 名称中会包含 "/", 替换为下划线以免文件夹路径混乱
            string safeSpriteName = texture.name.Replace("/", "_");
            string fileName = psbFileName + "_" + safeSpriteName + ".png";
            string fullPath = Path.Combine(outputFolder, fileName);

            // 写入磁盘（覆盖同名文件）
            File.WriteAllBytes(fullPath, pngData); 
        }
        catch (System.Exception e)
        {
            
        }

        // 6. 完成后刷新 AssetDatabase，使导出的 PNG 立即显示在 Project 视图（如果输出到 Assets 目录下）
        AssetDatabase.Refresh();
         
        EditorUtility.RevealInFinder(outputFolder); // 在操作系统文件管理器中打开
    }


    [MenuItem("Assets/刷新蒙皮网格范围")]
    private static void RefreshSkinnedMesh()
    {
         
        Object selected = Selection.activeObject;
        if (selected == null)
        {
            Debug.LogError("请先在 Project 视图中选中一个 PSB 导入后的 Texture2D 资源，再执行导出。");
            return;
        }
        if(selected is GameObject obj)
        {
            var skinneds = obj.GetComponentsInChildren<SkinnedMeshRenderer>();
            foreach(var s in skinneds)
            {
                s.sharedMesh.RecalculateBounds();
                s.bounds = s.sharedMesh.bounds;
            }
        }
      
    }
    private static void SaveExcel(StringStringDictionary data, string name)
    {
        FileInfo file = new FileInfo(excelPath);
        /*if (!File.Exists(excelPath))
        {
        }
        else
        {
        }*/

        using (ExcelPackage package = new ExcelPackage(file))
        {
            ExcelWorksheet worksheet;
            try
            {
                worksheet = package.Workbook.Worksheets[name];
                var cells = worksheet.Cells;
                cells.Clear();
            }
            catch
            {
                worksheet = package.Workbook.Worksheets.Add(name);
            }
            worksheet.Cells[1, 2].Value = "Key";
            worksheet.Cells[1, 3].Value = "Name";

            using (var e = data.GetEnumerator())
            {
                int i = 2;
                while (e.MoveNext())
                {
                    worksheet.Cells[i, 2].Value = e.Current.Key;
                    worksheet.Cells[i, 3].Value = e.Current.Value;
                    i++;
                }
            }
            package.Save();
        }

        /*
        FileInfo file = new FileInfo(excelPath);
        using (ExcelPackage package = new ExcelPackage(file))
        {
            ExcelWorksheet excelWorksheet = package.Workbook.Worksheets["PVP"];

            package.Save();
        }*/
    }

    [MenuItem("Assets/图片资源工具/检测并重新保存图片")]
    public static void CheckAndResaveTexture()
    {
        try
        {
            AssetDatabase.StartAssetEditing();
            foreach (var obj in Selection.GetFiltered<Object>(SelectionMode.Assets))
            {
                var path = AssetDatabase.GetAssetPath(obj);
                if (obj)
                {
                    var strs = path.Split('.');
                    if (strs[strs.Length - 1] == "png")
                    {
                        try
                        {
                            Texture2D texture2D = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                            SaveTexture(texture2D);
                        }
                        catch
                        {
                        }
                        //OutPSBFile(path);
                    }
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
    }

    private static void OpenDirectoryInfo(DirectoryInfo directoryInfo, string parentPath)
    {
        var _dirs = directoryInfo.GetDirectories();
        var files = directoryInfo.GetFiles("*.png");

        foreach (var f in files)
        {
            try
            {
                Texture2D texture2D = AssetDatabase.LoadAssetAtPath<Texture2D>(parentPath + "/" + f.Name);
                SaveTexture(texture2D);
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

    private static async void SaveTexture(Texture2D texture2D)
    {
        int width = texture2D.width;
        int height = texture2D.height;
        int wd = width % 4;
        int hd = height % 4;

        if (wd != 0 || hd != 0)
        {
            width += 4 - wd;
            height += 4 - hd;
            string strSaveFile = AssetDatabase.GetAssetPath(texture2D);

            Texture2D texture = new Texture2D(width, height, texture2D.format, false);
            int startW = (4 - wd) / 2;
            int startH = (4 - hd) / 2;
            Color color = new Color(0, 0, 0, 0);
            Color[] colors = new Color[width * height];
            texture.SetPixels(0, 0, width, height, colors);
            var colorData = texture2D.GetPixels();
            texture.SetPixels(startW, startH, texture2D.width, texture2D.height, colorData);

            byte[] dataBytes = texture.EncodeToPNG();

            using (FileStream fs = File.Open(strSaveFile, FileMode.Create))
            {
                fs.Seek(0, SeekOrigin.End);
                await fs.WriteAsync(dataBytes, 0, dataBytes.Length);
            }
        }
    }

    [MenuItem("Assets/图片资源工具/设置可读写")]
    public static void CheckAndSetReadTexture()
    {
        try
        {
            AssetDatabase.StartAssetEditing();
            foreach (var obj in Selection.GetFiltered<Object>(SelectionMode.Assets))
            {
                var path = AssetDatabase.GetAssetPath(obj);
                if (obj)
                {
                    var strs = path.Split('.');
                    if (strs[strs.Length - 1] == "png")
                    {
                        try
                        {
                            Texture2D texture2D = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                            SaveReable(texture2D);
                        }
                        catch
                        {
                        }
                        //OutPSBFile(path);
                    }
                }

                if (string.IsNullOrEmpty(path))
                    continue;

                if (System.IO.Directory.Exists(path))
                {
                    DirectoryInfo dir = new DirectoryInfo(path);
                    OpenDirectoryInfo1(dir, path);
                }
            }
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
        }
    }

    private static void OpenDirectoryInfo1(DirectoryInfo directoryInfo, string parentPath)
    {
        var _dirs = directoryInfo.GetDirectories();
        var files = directoryInfo.GetFiles("*.png");

        foreach (var f in files)
        {
            try
            {
                Texture2D texture2D = AssetDatabase.LoadAssetAtPath<Texture2D>(parentPath + "/" + f.Name);
                SaveReable(texture2D);
            }
            catch
            {
            }
        }
        foreach (var d in _dirs)
        {
            string path = parentPath + "/" + d.Name;
            OpenDirectoryInfo1(d, path);
        }
    }

    private static void SaveReable(Texture2D texture2D)
    {
        int width = texture2D.width;
        int height = texture2D.height;
        int wd = width % 4;
        int hd = height % 4;

        if (wd != 0 || hd != 0)
        {
            width += 4 - wd;
            height += 4 - hd;
            string strSaveFile = AssetDatabase.GetAssetPath(texture2D);
            if (!texture2D.isReadable)
            {
                TextureImporter textureImporter = TextureImporter.GetAtPath(strSaveFile) as TextureImporter;
                textureImporter.isReadable = true;
                textureImporter.SaveAndReimport();
                AssetDatabase.SaveAssets();
            }
        }
    }

    [MenuItem("Assets/图片资源工具/设置不可读写")]
    public static void CheckAndSetUnReadTexture()
    {
        try
        {
            AssetDatabase.StartAssetEditing();
            foreach (var obj in Selection.GetFiltered<Object>(SelectionMode.Assets))
            {
                var path = AssetDatabase.GetAssetPath(obj);
                if (obj)
                {
                    var strs = path.Split('.');
                    if (strs[strs.Length - 1] == "png")
                    {
                        try
                        {
                            Texture2D texture2D = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                            SaveUnReable(texture2D);
                        }
                        catch
                        {
                        }
                        //OutPSBFile(path);
                    }
                }

                if (string.IsNullOrEmpty(path))
                    continue;

                if (System.IO.Directory.Exists(path))
                {
                    DirectoryInfo dir = new DirectoryInfo(path);
                    OpenDirectoryInfo2(dir, path);
                }
            }
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
        }
    }

    private static void OpenDirectoryInfo2(DirectoryInfo directoryInfo, string parentPath)
    {
        var _dirs = directoryInfo.GetDirectories();
        var files = directoryInfo.GetFiles("*.png");

        foreach (var f in files)
        {
            try
            {
                Texture2D texture2D = AssetDatabase.LoadAssetAtPath<Texture2D>(parentPath + "/" + f.Name);
                SaveUnReable(texture2D);
            }
            catch
            {
            }
        }
        foreach (var d in _dirs)
        {
            string path = parentPath + "/" + d.Name;
            OpenDirectoryInfo2(d, path);
        }
    }

    private static void SaveUnReable(Texture2D texture2D)
    {
        string strSaveFile = AssetDatabase.GetAssetPath(texture2D);
        TextureImporter textureImporter = TextureImporter.GetAtPath(strSaveFile) as TextureImporter;
        textureImporter.isReadable = false;
        textureImporter.crunchedCompression = true;
        textureImporter.compressionQuality = 50;
        textureImporter.SaveAndReimport();
        AssetDatabase.SaveAssets();
    }
}