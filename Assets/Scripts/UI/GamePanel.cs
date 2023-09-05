using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class GamePanel : MonoBehaviour
{
    public static LayerMask UILayer;
    public static LayerMask HideLayer;

    Canvas canvas;
    GraphicRaycaster graphicRaycaster;
    public Dictionary<string, Transform> objectDatas = new Dictionary<string, Transform>();
    public virtual void OnEnable()
    {

    }
    public virtual void OnDisable()
    {

    }
    public T FindChildGameObject<T>(string childName) where T : Component
    {
        if (objectDatas.ContainsKey(childName))
        {
            var t= objectDatas[childName].GetComponent<T>();
            if (t == null)
            {
                t =objectDatas[childName].gameObject.AddComponent<T>();
            }
            return t;
        }
        return default(T);
    }
    public Transform FindChildGameObject(string childName)
    {
        if (objectDatas.ContainsKey(childName))
        {
            return objectDatas[childName];
        }
        return null;
    }

    public virtual void InitChildObjData()
    {
        objectDatas.Clear();
        var children = gameObject.GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < children.Length; i++)
        {
            try
            {
                objectDatas.Add(children[i].name, children[i]);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"{gameObject.name}:{e}");
            } 
        }
       
       
    }
    public virtual void SetPanelUISerializeObj()
    {
        InitChildObjData();
        var uiObjReferences = gameObject.GetComponentsInChildren<BaseReference>(true);
        foreach(var uiObj in uiObjReferences)
        {
            uiObj.SetPanelUISerializeObj();
        }

    }
    
    protected virtual void Awake() 
    {
        canvas = gameObject.GetComponent<Canvas>();
        graphicRaycaster=gameObject.GetComponent<GraphicRaycaster>();
        canvas.worldCamera = CameraController.instance.uiCamera;
        
    }
    public virtual async Task InitData<V>(V v)where V:IReferenceData { }
    public virtual async Task InitData(string dataKey) { }
    public virtual void Show(int layer = -1)
    {
        canvas.sortingOrder = layer;
        gameObject.layer = UILayer;
        if (graphicRaycaster)
        {
            graphicRaycaster.enabled = true;
        }
        enabled = true;
    }
    public virtual void Close()
    { 
        gameObject.layer = HideLayer;
        if (graphicRaycaster)
        {
            graphicRaycaster.enabled = false;
        }
        objectDatas.Clear();
        enabled = false;
    }
} 