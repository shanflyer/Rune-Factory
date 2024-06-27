using UnityEngine;
using Newtonsoft;
using Newtonsoft.Json;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

public class Test : MonoBehaviour
{
    public List<FilmData> formulaDatas=new List<FilmData>();
    public void TestNewtosoftTostring()
    {
        string path = DataPath.GetDataPath(typeof(FilmData));
        var allData= Resources.LoadAll<FilmData>(path);
        if (allData != null && allData.Length > 0)
        {
            string strs=JsonConvert.SerializeObject(allData);
            File.WriteAllText("test.json", strs);
            Debug.Log(strs);
        }
    }
    public void TesttNewtosoftToObj()
    {
        var strs = File.ReadAllText("test.json");
        formulaDatas = JsonConvert.DeserializeObject<List<FilmData>>(strs);
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
[CustomEditor(typeof(Test))]
public class TestEditor : Editor
{
    public Test test => target as Test;
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (GUILayout.Button("测试json序列化"))
        {
            test.TestNewtosoftTostring();
        }
        if (GUILayout.Button("测试json反序列化"))
        {
            test.TesttNewtosoftToObj();
        }
    }
}