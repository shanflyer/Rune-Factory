using OfficeOpenXml;
using System.IO;
using UnityEditor;
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

    private const string excelPath = "Assets/Editor/DataDic/Data.xlsx";

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