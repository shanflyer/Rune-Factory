using Excel;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Reflection;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

public class ExcelDataEditor : MyEditor
{
    public static ExcelDataEditor Instance;

    [MenuItem("工具/Excel数据管理")]
    public static void WindowShow()
    {
        ExcelDataEditor excelDataEditor = CreateWindow<ExcelDataEditor>("Excel数据管理");
        excelDataEditor.minSize = excelDataEditor.maxSize = new Vector2(400, 480);
        excelDataEditor.Show();

        Instance = excelDataEditor;
    }

    private List<ExcelDataObj> selectDatas = new List<ExcelDataObj>();

    public void AddExcelDataObj(ExcelDataObj excelDataObj)
    {
        if (!selectDatas.Contains(excelDataObj))
        {
            selectDatas.Add(excelDataObj);
        }
    }

    public void RemoveExcelDataObj(ExcelDataObj excelDataObj)
    {
        if (selectDatas.Contains(excelDataObj))
        {
            selectDatas.Remove(excelDataObj);
        }
    }

    public void OnGUI()
    {
        excelDataPanel.DisplayCommonObjList<ExcelDataObj>(360, 400, commonObjs, 2, false);
        DrawButton("输出选择数据", () =>
        {
            RefreshAllData(selectDatas);
        }, 200);
        DrawButton("输出全部数据", () =>
        {
            List<ExcelDataObj> _selectDatas = new List<ExcelDataObj>();
            foreach (var data in commonObjs)
            {
                _selectDatas.Add((ExcelDataObj)data);
            }

            RefreshAllData(_selectDatas);
        }, 200);
    }

    private List<CommonObj> commonObjs = new List<CommonObj>();
    private CommonEditor excelDataPanel;

    private void LoadExcelData()
    {
        string path = "/Datas";
        DirectoryInfo directoryInfo = new DirectoryInfo(Application.dataPath);
        path = $"{directoryInfo.Parent}{path}";

        DirectoryInfo dataDirectoryInfo = new DirectoryInfo(path);
        var files = dataDirectoryInfo.GetFiles("*.xlsx");
        foreach (var file in files)
        {
            ExcelDataObj excelDataObj = new ExcelDataObj(file.Name.Split('.')[0], file.FullName);
            commonObjs.Add(excelDataObj);
        }
    }

    private void RefreshAllData(List<ExcelDataObj> selectDatas)
    {
        try
        {
            AssetDatabase.StartAssetEditing();

            foreach (var selectData in selectDatas)
            {
                string listStr = selectData.GetName();

                FileStream fileStream = File.Open(selectData.path, FileMode.Open, FileAccess.Read);
                IExcelDataReader excelReader = ExcelReaderFactory.CreateOpenXmlReader(fileStream);

                DataSet result = excelReader.AsDataSet();
                int columnCount = result.Tables[0].Columns.Count;
                int rowCount = result.Tables[0].Rows.Count;

                Type type = null;
                Type dataType = null;

                var dataName = result.Tables[0].Rows[0][0].ToString();

                var fieldName = result.Tables[0].Rows[0][1].ToString();

                if (!string.IsNullOrEmpty(dataName))
                {
                    dataType = Assembly.Load("Assembly-CSharp").GetType(dataName);
                }
                type = Assembly.Load("Assembly-CSharp").GetType(listStr);

                FieldInfo[] fields = null;
                if (dataType == null)
                {
                    fields = type.GetFields();
                }
                else
                {
                    fields = dataType.GetFields();
                }

                Dictionary<string, FieldInfo> keyFields = new Dictionary<string, FieldInfo>();

                Dictionary<string, Dictionary<string, FieldInfo>> keyChildFields = new Dictionary<string, Dictionary<string, FieldInfo>>();

                foreach (var field in fields)
                {
                    keyFields.Add(field.Name, field);
                }

                List<FieldInfo> excelFields = new List<FieldInfo>();
                Dictionary<string, List<FieldInfo>> childFieldDatas = new Dictionary<string, List<FieldInfo>>();
                for (int i = 0; i < columnCount; i++)
                {
                    var fieldTypeValue = result.Tables[0].Rows[1][i].ToString();
                    var strs = fieldTypeValue.Split(':');
                    if (strs.Length == 1)
                    {
                        var value = result.Tables[0].Rows[2][i].ToString();
                        if (keyFields.TryGetValue(value, out FieldInfo fieldInfo))
                        {
                            excelFields.Add(fieldInfo);
                        }
                    }
                    else
                    {
                        if (keyFields.TryGetValue(strs[0], out FieldInfo fieldInfo))
                        {
                            if (!keyChildFields.TryGetValue(strs[0], out Dictionary<string, FieldInfo> childFields))
                            {
                                childFields = new Dictionary<string, FieldInfo>();
                                var childType = fieldInfo.FieldType;
                                var childFieldArray = childType.GetFields();
                                foreach (var childField in childFieldArray)
                                {
                                    childFields.Add(childField.Name, childField);
                                }
                                keyChildFields.Add(strs[0], childFields);
                                childFieldDatas.Add(strs[0], new List<FieldInfo>());
                            }

                            excelFields.Add(fieldInfo);
                            var value = result.Tables[0].Rows[2][i].ToString();
                            if (childFields.TryGetValue(value, out FieldInfo childFieldInfo))
                            {
                                childFieldDatas[strs[0]].Add(childFieldInfo);
                            }
                        }
                    }
                }
                string outPath = $"{EditorDataPath.outDataPath}{result.Tables[0].TableName}";
                // string outPath = dataType != null? $"{EditorDataPath.outDataPath}{dataName}": $"{EditorDataPath.outDataPath}{listStr}";
                if (Directory.Exists(outPath))
                {
                    Directory.Delete(outPath, true);
                }
                Directory.CreateDirectory(outPath);

                object listData = null;

                Array array = null;
                if (dataType != null)
                {
                    array = Array.CreateInstance(dataType, rowCount - 3);
                    listData = Activator.CreateInstance(type);
                }

                for (int i = 3; i < rowCount; i++)
                {
                    var data = Activator.CreateInstance(dataType == null ? type : dataType);
                    Dictionary<string, object> childDatas = new Dictionary<string, object>();
                    for (int j = 0; j < columnCount; j++)
                    {
                        FieldInfo fieldInfo = excelFields[j];

                        var value = result.Tables[0].Rows[i][j];
                        LinkDataFieldValue(data, fieldInfo, value, childFieldDatas, childDatas);
                    }

                    if (dataType != null)
                    {
                        array.SetValue(data, i - 3);
                    }
                    else
                    {
                        MethodInfo meth = type.GetMethod("SetReferenceData");
                        if (meth != null)
                        {
                            meth.Invoke(data, null);
                        }
                        AssetDatabase.CreateAsset((UnityEngine.Object)data, $"{outPath}{"/"}{data}{".asset"}");
                    }
                }
                if (dataType != null)
                {
                    var Listfields = type.GetFields();
                    FieldInfo selectField = null;
                    foreach (var field in Listfields)
                    {
                        if (field.Name == fieldName)
                        {
                            selectField = field;
                            break;
                        }
                    }
                    if (selectField != null)
                    {
                        selectField.SetValue(listData, array);
                    }

                    MethodInfo meth = type.GetMethod("SetReferenceData");
                    if (meth != null)
                    {
                        meth.Invoke(listData, null);
                    }

                    UnityEngine.Object saveObj = (UnityEngine.Object)listData;
                    string objPath = $"{outPath}{"/"}{listData}{".asset"}";

                    if (AssetDatabase.Contains(saveObj))
                    {
                        EditorUtility.SetDirty(saveObj);
                        AssetDatabase.SaveAssets();
                    }
                    else
                    {
                        AssetDatabase.CreateAsset(saveObj, objPath);
                        AssetDatabase.Refresh();
                    }
                }

                MethodInfo meth1 = type.GetMethod("Clear");
                if (meth1 != null)
                {
                    meth1.Invoke(null, null);
                }
            }
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
        }
    }

    private void LinkDataFieldValue(object data, FieldInfo fieldInfo, object value,
        Dictionary<string, List<FieldInfo>> childFieldDatas = null,
         Dictionary<string, object> childDatas = null)
    {
        if (fieldInfo.FieldType.BaseType == typeof(Enum))
        {
            value = Convert.ChangeType(value, typeof(int));
            fieldInfo.SetValue(data, value);
        }
        else if (fieldInfo.FieldType == typeof(List<int>))
        {
            var valueStr = value.ToString();
            if (!string.IsNullOrEmpty(valueStr))
            {
                var strs = value.ToString().Split(',');

                List<int> _value = new List<int>();
                foreach (var str in strs)
                {
                    _value.Add(int.Parse(str));
                }
                value = _value;
                fieldInfo.SetValue(data, value);
            }
        }
        else if (fieldInfo.FieldType == typeof(List<int2>))
        {
            var valueStr = value.ToString();
            if (!string.IsNullOrEmpty(valueStr))
            {
                var strs = value.ToString().Split('|');

                List<int2> _value = new List<int2>();
                foreach (var str in strs)
                {
                    var _strs = str.Split(',');
                    int2 int3Value = new int2(int.Parse(_strs[0]), int.Parse(_strs[1]));
                    _value.Add(int3Value);
                }
                value = _value;
                fieldInfo.SetValue(data, value);
            }
        }
        else if (fieldInfo.FieldType == typeof(List<int3>))
        {
            var valueStr = value.ToString();
            if (!string.IsNullOrEmpty(valueStr))
            {
                var strs = value.ToString().Split('|');

                List<int3> _value = new List<int3>();
                foreach (var str in strs)
                {
                    var _strs = str.Split(',');
                    int3 int3Value = new int3(int.Parse(_strs[0]), int.Parse(_strs[1]), int.Parse(_strs[2]));
                    _value.Add(int3Value);
                }
                value = _value;
                fieldInfo.SetValue(data, value);
            }
        }
        else if (fieldInfo.FieldType == typeof(int2))
        {
            var valueStr = value.ToString();
            if (!string.IsNullOrEmpty(valueStr))
            {
                var strs = value.ToString().Split(',');

                int2 _value = int2.zero;
                _value.x = int.Parse(strs[0]);
                _value.y = int.Parse(strs[1]);
                value = _value;
                fieldInfo.SetValue(data, value);
            }
        }
        else if (fieldInfo.FieldType == typeof(List<string>))
        {
            var valueStr = value.ToString();
            if (!string.IsNullOrEmpty(valueStr))
            {
                var strs = value.ToString().Split(',');

                List<string> _value = new List<string>();
                foreach (var str in strs)
                {
                    _value.Add(str);
                }
                value = _value;
                fieldInfo.SetValue(data, value);
            }
        }
        else if (childFieldDatas != null && childFieldDatas.TryGetValue(fieldInfo.Name, out var childFileds))
        {
            if (!childDatas.TryGetValue(fieldInfo.Name, out var childData))
            {
                childData = Activator.CreateInstance(fieldInfo.FieldType);
                childDatas.Add(fieldInfo.Name, childData);
            }
            for (int childFieldIndex = 0; childFieldIndex < childFileds.Count; childFieldIndex++)
            {
                FieldInfo childFieldInfo = childFileds[childFieldIndex];
                LinkDataFieldValue(childData, childFieldInfo, value);
            }
            fieldInfo.SetValue(data, childData);
        }
        else
        {
            try
            {
                value = Convert.ChangeType(value, fieldInfo.FieldType);
                fieldInfo.SetValue(data, value);
            }
            catch
            {
                Debug.LogError($"{fieldInfo.Name}-{value.ToString()}");
            }
        }
    }

    public new void Show()
    {
        LoadExcelData();

        excelDataPanel = CreateInstance<CommonEditor>();
        excelDataPanel.InitData(this, null);
        excelDataPanel.SetSelectObjList(true);
    }
}