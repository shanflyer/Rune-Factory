using Excel;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
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

    public struct ChildFieldData
    {
        public Type type;
        public string fieldName;
        public FieldInfo fieldInfo;
        public int2 Range;
        public Dictionary<string, FieldInfo> fields;
        public bool isList;
    }

    private (List<ChildFieldData>, HashSet<int>) GetChildFieldDatas(DataSet result, Dictionary<string, FieldInfo> keyFields)
    {
        HashSet<int> childCell = new HashSet<int>();
        int columnCount = result.Tables[0].Columns.Count;
        Dictionary<string, ChildFieldData> childFieldDatas = new Dictionary<string, ChildFieldData>();
        for (int x = 0; x < columnCount; x++)
        {
            var str = result.Tables[0].Rows[1][x].ToString();
            if (!string.IsNullOrEmpty(str))
            {
                if (childFieldDatas.TryGetValue(str, out var childFieldData))
                {
                    childFieldData.Range.y = x;
                    childFieldDatas[str] = childFieldData;
                    for (int i = childFieldData.Range.x; i <= childFieldData.Range.y; i++)
                    {
                        childCell.Add(i);
                    }
                }
                else
                {
                    var strs = str.Split(":");
                    Type type = Assembly.Load("Assembly-CSharp").GetType(strs[0]);
                    if (type == null)
                    {
                        Debug.LogError($"{str} 类型转换失败");
                    }
                    else
                    {
                        childFieldData = new ChildFieldData
                        {
                            type = type,
                            fieldName = strs[1],
                            Range = new int2(x, 0),
                            fields = new Dictionary<string, FieldInfo>(),
                            isList = strs.Length >= 3
                        };
                        FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
                        for (int i = 0; i < fields.Length; i++)
                        {
                            childFieldData.fields.Add(fields[i].Name, fields[i]);
                        }
                        if (keyFields.TryGetValue(childFieldData.fieldName, out var keyFieldData))
                        {
                            childFieldData.fieldInfo = keyFieldData;
                        }

                        childFieldDatas.Add(str, childFieldData);
                    }
                }
            }
        }
        var dataList = childFieldDatas.Values.ToList();
        dataList.RemoveAll(d => d.Range.y == 0);
        dataList.Sort((d0, d1) => d0.Range.x.CompareTo(d1.Range.x));

        return (dataList, childCell);
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

                var v0 = result.Tables[0].Rows[1][0].ToString();
                var v1 = result.Tables[0].Rows[2][0].ToString();
                if (string.IsNullOrEmpty(v0) && !string.IsNullOrEmpty(v1))
                {
                    Type type = Assembly.Load("Assembly-CSharp").GetType(listStr);
                    var keyMeth = type.GetMethod("GetKey");
                    FieldInfo[] fields = null;
                    fields = type.GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
                    Dictionary<string, FieldInfo> keyFields = new Dictionary<string, FieldInfo>();
                    foreach (var field in fields)
                    {
                        keyFields.Add(field.Name, field);
                    }

                    var datas = GetChildFieldDatas(result, keyFields);
                    var childFieldData = datas.Item1;
                    var childCell = datas.Item2;

                    List<FieldInfo> excelFields = new List<FieldInfo>();
                    for (int i = 0; i < columnCount; i++)
                    {
                        if (childCell.Contains(i))
                        {
                            continue;
                        }
                        var value = result.Tables[0].Rows[3][i].ToString();
                        if (keyFields.TryGetValue(value, out FieldInfo fieldInfo))
                        {
                            excelFields.Add(fieldInfo);
                        }
                    }

                    string outPath = $"{EditorDataPath.outDataPath}{result.Tables[0].TableName}";
                    if (Directory.Exists(outPath))
                    {
                        Directory.Delete(outPath, true);
                    }
                    Directory.CreateDirectory(outPath);

                    Dictionary<string, object> dataDic = new Dictionary<string, object>();
                    Dictionary<string, Dictionary<FieldInfo, (Type, List<object>)>> arrayDatasDic =
                        new Dictionary<string, Dictionary<FieldInfo, (Type, List<object>)>>();
                    for (int i = 4; i < rowCount; i++)
                    {
                        var data = Activator.CreateInstance(type);

                        int fieldIndex = 0;
                        Dictionary<Type, (FieldInfo, object, bool)> childDatas = new Dictionary<Type, (FieldInfo, object, bool)>();

                        for (int j = 0; j < columnCount; j++)
                        {
                            var value = result.Tables[0].Rows[i][j];
                            if (childCell.Contains(j))
                            {
                                for (int index = 0; index < childFieldData.Count; index++)
                                {
                                    if (childFieldData[index].Range.x <= j && childFieldData[index].Range.y >= j)
                                    {
                                        if (!childDatas.TryGetValue(childFieldData[index].type, out var childData))
                                        {
                                            childData = (childFieldData[index].fieldInfo, Activator.CreateInstance(childFieldData[index].type), childFieldData[index].isList);
                                            childDatas.Add(childFieldData[index].type, childData);
                                        }
                                        var fieldName = result.Tables[0].Rows[3][j].ToString();
                                        if (childFieldData[index].fields.TryGetValue(fieldName, out var field))
                                        {
                                            LinkDataFieldValue(childData.Item2, field, value);
                                        }
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                FieldInfo fieldInfo = excelFields[fieldIndex];

                                fieldIndex++;
                                LinkDataFieldValue(data, fieldInfo, value);
                            }
                        }
                        string key = (string)keyMeth.Invoke(data, null);
                        if (!dataDic.ContainsKey(key))
                        {
                            dataDic.Add(key, data);
                        }
                        else
                        {
                            data = dataDic[key];
                        }
                        if (!arrayDatasDic.TryGetValue(key, out var arrayDatas))
                        {
                            arrayDatas = new Dictionary<FieldInfo, (Type, List<object>)>();
                            arrayDatasDic.Add(key, arrayDatas);
                        }

                        foreach (var childData in childDatas)
                        {
                            if (childData.Value.Item3)
                            {
                                if (!arrayDatas.TryGetValue(childData.Value.Item1, out var list))
                                {
                                    list = (childData.Key, new List<object>());
                                    arrayDatas.Add(childData.Value.Item1, list);
                                }
                                list.Item2.Add(childData.Value.Item2);
                            }
                            else
                            {
                                LinkDataFieldValue(data, childData.Value.Item1, childData.Value.Item2);
                            }
                        }
                    }

                    foreach (var data in dataDic)
                    {
                        if (arrayDatasDic.TryGetValue(data.Key, out var arrayDatas))
                        {
                            foreach (var arrayData in arrayDatas)
                            {
                                Array array = Array.CreateInstance(arrayData.Value.Item1, arrayData.Value.Item2.Count);
                                for (int i = 0; i < arrayData.Value.Item2.Count; i++)
                                {
                                    array.SetValue(arrayData.Value.Item2[i], i);
                                }
                                LinkDataFieldValue(data.Value, arrayData.Key, array);
                            }
                        }

                        MethodInfo meth = type.GetMethod("SetReferenceData");
                        if (meth != null)
                        {
                            meth.Invoke(data.Value, null);
                        }
                        AssetDatabase.CreateAsset((UnityEngine.Object)data.Value, $"{outPath}{"/"}{data.Value}{".asset"}");
                    }
                }
                else
                {
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
                        fields = type.GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
                    }
                    else
                    {
                        fields = dataType.GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
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
                                    var childFieldArray = childType.GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
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
                            if (string.IsNullOrEmpty(result.Tables[0].Rows[2][j].ToString()))
                            {
                                break;
                            }
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

                    MethodInfo methGroup = type.GetMethod("isSingleGroup");
                    if (methGroup != null && (bool)methGroup.Invoke(listData, null))
                    {
                        var keyMeth = dataType.GetMethod("GetKey");
                        Dictionary<string, List<object>> objGroups = new Dictionary<string, List<object>>();
                        foreach (var a in array)
                        {
                            string key = (string)keyMeth.Invoke(a, null);
                            if (!objGroups.TryGetValue(key, out var list))
                            {
                                list = new List<object>();
                                objGroups.Add(key, list);
                            }
                            list.Add(a);
                        }
                        foreach (var obj in objGroups)
                        {
                            listData = Activator.CreateInstance(type);
                            MethodInfo methSetObjList = type.GetMethod("SetObjList");

                            if (methSetObjList != null)
                            {
                                methSetObjList.Invoke(listData, new object[] { obj.Value });
                            }
                            var setkeyMeth = type.GetMethod("SetKey");
                            setkeyMeth.Invoke(listData, new object[1] { obj.Key });

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
                    }
                    else
                    if (dataType != null)
                    {
                        var Listfields = type.GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
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
        try
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
                        if (string.IsNullOrEmpty(str))
                        {
                            continue;
                        }
                        _value.Add(int.Parse(str));
                    }
                    value = _value;
                    fieldInfo.SetValue(data, value);
                }
            }
            else if (fieldInfo.FieldType == typeof(int[]))
            {
                var valueStr = value.ToString();
                if (!string.IsNullOrEmpty(valueStr))
                {
                    var strs = value.ToString().Split(',');

                    List<int> _value = new List<int>();
                    foreach (var str in strs)
                    {
                        if (string.IsNullOrEmpty(str))
                        {
                            continue;
                        }
                        _value.Add(int.Parse(str));
                    }
                    value = _value.ToArray();
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
            else if (fieldInfo.FieldType == typeof(List<float3>))
            {
                var valueStr = value.ToString();
                if (!string.IsNullOrEmpty(valueStr))
                {
                    var strs = value.ToString().Split('|');

                    List<float3> _value = new List<float3>();
                    foreach (var str in strs)
                    {
                        var _strs = str.Split(',');
                        float3 int3Value = new float3(float.Parse(_strs[0]), float.Parse(_strs[1]), float.Parse(_strs[2]));
                        _value.Add(int3Value);
                    }
                    value = _value;
                    fieldInfo.SetValue(data, value);
                }
            }
            else if (fieldInfo.FieldType == typeof(List<float2>))
            {
                var valueStr = value.ToString();
                if (!string.IsNullOrEmpty(valueStr))
                {
                    var strs = value.ToString().Split('|');

                    List<float2> _value = new List<float2>();
                    foreach (var str in strs)
                    {
                        var _strs = str.Split(',');
                        float2 int3Value = new float2(float.Parse(_strs[0]), float.Parse(_strs[1]));
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
            else if (fieldInfo.FieldType == typeof(IntIntDictionary))
            {
                if (string.IsNullOrEmpty(value.ToString()))
                {
                    return;
                }
                var strs = value.ToString().Split('|');

                IntIntDictionary dicValue = new IntIntDictionary();
                foreach (var str in strs)
                {
                    var _strs = str.Split(',');
                    dicValue[int.Parse(_strs[0])] = int.Parse(_strs[1]);
                }
                value = dicValue;
                fieldInfo.SetValue(data, value);
            }
            else if (fieldInfo.FieldType == typeof(float2))
            {
                var valueStr = value.ToString();
                if (!string.IsNullOrEmpty(valueStr))
                {
                    var strs = value.ToString().Split(',');

                    float2 _value = float2.zero;
                    _value.x = float.Parse(strs[0]);
                    _value.y = float.Parse(strs[1]);
                    value = _value;
                    fieldInfo.SetValue(data, value);
                }
            }
            else if (fieldInfo.FieldType == typeof(float3))
            {
                var valueStr = value.ToString();
                if (!string.IsNullOrEmpty(valueStr))
                {
                    var strs = value.ToString().Split(',');

                    float3 _value = float3.zero;
                    _value.x = float.Parse(strs[0]);
                    _value.y = float.Parse(strs[1]);
                    _value.z = float.Parse(strs[2]);
                    value = _value;
                    fieldInfo.SetValue(data, value);
                }
            }
            else if (fieldInfo.FieldType == typeof(float4))
            {
                var valueStr = value.ToString();
                if (!string.IsNullOrEmpty(valueStr))
                {
                    var strs = value.ToString().Split(',');

                    float4 _value = float4.zero;
                    _value.x = float.Parse(strs[0]);
                    _value.y = float.Parse(strs[1]);
                    _value.z = float.Parse(strs[2]);
                    _value.w = float.Parse(strs[3]);
                    value = _value;
                    fieldInfo.SetValue(data, value);
                }
            }
            else if (fieldInfo.FieldType == typeof(Vector2))
            {
                var valueStr = value.ToString();
                if (!string.IsNullOrEmpty(valueStr))
                {
                    var strs = value.ToString().Split(',');

                    Vector2 _value = Vector2.zero;
                    _value.x = float.Parse(strs[0]);
                    _value.y = float.Parse(strs[1]);
                    value = _value;
                    fieldInfo.SetValue(data, value);
                }
            }
            else if (fieldInfo.FieldType == typeof(Vector3))
            {
                var valueStr = value.ToString();
                if (!string.IsNullOrEmpty(valueStr))
                {
                    var strs = value.ToString().Split(',');

                    Vector3 _value = Vector3.zero;
                    _value.x = float.Parse(strs[0]);
                    _value.y = float.Parse(strs[1]);
                    _value.z = float.Parse(strs[2]);
                    value = _value;
                    fieldInfo.SetValue(data, value);
                }
            }
            else if (fieldInfo.FieldType == typeof(List<string>))
            {
                var valueStr = value.ToString();
                if (!string.IsNullOrEmpty(valueStr))
                {
                    var strs = value.ToString().Split('|');

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
                    var valueStr = value.ToString();
                    if (!string.IsNullOrEmpty(valueStr))
                    {
                        value = Convert.ChangeType(value, fieldInfo.FieldType);
                        fieldInfo.SetValue(data, value);
                    }
                }
                catch
                {
                    Debug.LogError($"{fieldInfo.Name}-{value.ToString()}");
                }
            }
        }
        catch
        {
            Debug.LogError($"{fieldInfo.Name}-{value.ToString()}");
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