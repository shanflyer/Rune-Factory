using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class GamePanel<V> : BaseReference where V:IReferenceData
{
    GraphicRaycaster graphicRaycaster;

    public Dictionary<string, Transform> objectDatas = new Dictionary<string, Transform>();
    public virtual void OnEnable()
    {
        
    }
    public virtual void OnDisable()
    {
        Close();
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
                Debug.Log($"{gameObject.name}:{e}");
            } 
        }
       
       
    }
    public override void SetPanelUISerializeObj()
    {
        base.SetPanelUISerializeObj();
        InitChildObjData();
        var uiObjReferences = gameObject.GetComponentsInChildren<BaseReference>(true);
        foreach(var uiObj in uiObjReferences)
        {
            if (uiObj == this)
            {
                continue;
            }
            uiObj.SetPanelUISerializeObj();
        }

    }
    
    protected virtual void Awake() 
    {
        canvas = gameObject.GetComponent<Canvas>();
        graphicRaycaster=gameObject.GetComponent<GraphicRaycaster>();
        canvas.worldCamera = CameraManager.instance.uiCamera;
        
    }
    protected V data;
    public virtual void InitReferenceData(V v) 
    {
        data = v;
    } 
    public override void Show(int layer = -1)
    {
        UIManager.instance.UIAudioForTag(tag);
        base.Show();
        if (changeInputModel) 
        {
            InputManager.instance.SwitchInputMap(true);
        }

        if (layer != -1)
        {
            canvas.sortingOrder = layer;
        }
       
        gameObject.layer = UILayer;
        if (graphicRaycaster)
        {
            graphicRaycaster.enabled = true;
        }
        enabled = true;
    }
    public override void Close()
    {
        /*HidePanel hidePanel = new HidePanel
        {
            hide = false,
            type = typeof(TalkPanel)
        };
        GameActionManager.instance.QueueAction(hidePanel,true);*/

        base.Close();
        if (SingletonType.Cleared)
        {
            Destroy(this);
        }
        if (!SingletonType.Cleared)
            UIManager.instance.RemoveGamePanel(this.GetType());
        if (changeInputModel)
        {
            if (!SingletonType.Cleared)
                InputManager.instance.SwitchInputMap(false);
        } 
        if (UIManager.IsPluralUI(this.GetType()))
        {
            if (this!=null&&gameObject!=null)
            {
                Destroy(gameObject);
            }
            
        }
        else
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
} 