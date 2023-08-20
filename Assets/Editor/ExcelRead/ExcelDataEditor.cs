using Excel;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public class ExcelDataEditor : MyEditor
{
    public static ExcelDataEditor Instance;

    [MenuItem("工具/Excel数据管理")]
    public static void WindowShow()
    {
        ExcelDataEditor excelDataEditor = CreateWindow<ExcelDataEditor>("Excel数据管理");
        excelDataEditor.minSize = excelDataEditor.maxSize = new Vector2(240, 480);
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
        excelDataPanel.DisplayCommonObjList<ExcelDataObj>(240, 400, commonObjs, 2, false);
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
                foreach (var field in fields)
                {
                    keyFields.Add(field.Name, field);
                }

                List<FieldInfo> excelFields = new List<FieldInfo>();
                for (int i = 0; i < columnCount; i++)
                {
                    var value = result.Tables[0].Rows[2][i].ToString();
                    if (keyFields.TryGetValue(value, out FieldInfo fieldInfo))
                    {
                        excelFields.Add(fieldInfo);
                    }
                }

                string outPath = dataType != null? $"{EditorDataPath.outDataPath}{dataName}": $"{EditorDataPath.outDataPath}{listStr}";
                if (Directory.Exists(outPath))
                {
                    Directory.Delete(outPath, true);
                }
                Directory.CreateDirectory(outPath);

                object listData = null;
                Array array=null;
                if (dataType != null)
                {
                    array = Array.CreateInstance(dataType, rowCount - 3);
                    listData = Activator.CreateInstance(type);
                }

                for (int i = 3; i < rowCount; i++)
                {
                    var data = Activator.CreateInstance(dataType == null ? type : dataType);
                    for (int j = 0; j < columnCount; j++)
                    {
                        FieldInfo fieldInfo = excelFields[j];

                        var value = result.Tables[0].Rows[i][j];
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
                        else
                        {
                            value = Convert.ChangeType(value, fieldInfo.FieldType);
                            fieldInfo.SetValue(data, value);
                        }
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
            }
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
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