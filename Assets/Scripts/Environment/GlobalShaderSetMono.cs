using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class GlobalShaderSetMono : MonoBehaviour
{
    public List<GlobalData> GlobalDatas = new List<GlobalData>();
    public void SetGlobalShaderValue()
    {
        for(int i=0;i<GlobalDatas.Count; i++)
        {
            GlobalDatas[i].InitGlobalShader();
        }
    }
    private void Awake()
    {
        SetGlobalShaderValue();
    }

} 
public enum ShaderDataType
{
    INT,FLOAT,VECTOR,COLOR
}
[Serializable]
public struct GlobalData
{
    public string dataName;
    public ShaderDataType shaderDataType;
    public int intValue;
    public float floatValue;
    public Vector4 vectorValue;
    public Color colorValue;

    public void InitGlobalShader()
    {
        switch (shaderDataType)
        {
            case ShaderDataType.INT:
                Shader.SetGlobalInt(dataName, intValue);
                break;
            case ShaderDataType.FLOAT:
                Shader.SetGlobalFloat(dataName, floatValue);
                break;
            case ShaderDataType.VECTOR:
                Shader.SetGlobalVector(dataName, vectorValue);
                break;
            case ShaderDataType.COLOR:
                Shader.SetGlobalColor(dataName, colorValue* floatValue);
                break;
        }
    }
}
#if UNITY_EDITOR
[CustomEditor(typeof(GlobalShaderSetMono))]
public class GlobalShaderSetMonoEditor : Editor
{
    public GlobalShaderSetMono globalShaderSetMono
    {
        get
        {
            return target as GlobalShaderSetMono;
        }
    }
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (GUILayout.Button("ÉèÖÃ"))
        {
            globalShaderSetMono.SetGlobalShaderValue();
        }
    }
}
#endif