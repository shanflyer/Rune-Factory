using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;
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

                    if(!sources.TryGetValue(title,out List<string> list))
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
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
