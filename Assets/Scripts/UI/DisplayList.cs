using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class DisplayList<T, V> where T : UIObjReference<V>
{
    public Transform parent { get; private set; }
    private T listPrefab;
    private List<T> list;

    public int dataCount => _dataCount;
    private int _dataCount;
    public DisplayList(T listPrefab, Transform parent)
    {
        this.parent = parent;
        this.listPrefab = listPrefab;
        list = new List<T>();
    }
    public void SelectIndex(int index)
    {
        if (index < dataCount)
        {
            list[index].Selected();
        }
    }
    public virtual void ClearSelect()
    {
        for (int i = 0; i < list.Count; i++)
        {
            list[i].ClearSelect();
        }
    }

    public virtual void ClearSelect(V v)
    {
        for (int i = 0; i < list.Count; i++)
        {
            if (!list[i].t.Equals(v))
            {
                list[i].ClearSelect();
            }
        }
    }

    public void Select(V v)
    {
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i].t.Equals(v))
            {
                list[i].SelectDefault();
            }
        }
    }

    public void SelectDefault()
    {
        if (list.Count > 0)
        {
            list[0].SelectDefault();
        }
    }
    public T GetReference(V v)
    {
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i].t.Equals(v))
            {
                return list[i];
            }
        }
        return null;
    }
    public T GetReference(int index)
    {
        if (index < dataCount)
        {
            return list[index];
        }
        return null;
    }
    public void SetSelectData(V v, SelectAction<V> SelectAction = null, ToggleGroup toggleGroup = null)
    {
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i].t.Equals(v))
            {
                // 同步选中刷新不能 await，引用刷新异常走统一异步日志。
                AsyncTaskRunner.Run(list[i].InitData(v, SelectAction, toggleGroup), nameof(SetSelectData));
            }
        }
    }
    static Vector3 zero = new Vector3(0, 1, 1);
    public async Task InitListData(V[] componentData, SelectAction<V> SelectAction = null, ToggleGroup toggleGroup = null, bool Async = true)
    {

        if (componentData == null || componentData.Length == 0)
        {
            for (int i = 0; i < list.Count; i++)
            {
                list[i].ClearData();
                list[i].enabled = false;
                list[i].transform.localScale = zero;
            }
            _dataCount = 0;
            return;
        }
        _dataCount = componentData.Length;
        for (int i = list.Count - 1; i > componentData.Length - 1; i--)
        {
            list[i].ClearData();
            list[i].enabled = false;
            list[i].transform.localScale = zero;
        }

        for (int i = 0; i < componentData.Length; i++)
        {
            if (list.Count > i)
            {
                list[i].enabled = true;
                list[i].transform.localScale = Vector3.one;
                await list[i].InitData(componentData[i], SelectAction, toggleGroup);
            }
            else
            {
                T t;
                /*
                if (Async)
                {
                    var async = GameObject.InstantiateAsync(listPrefab);
                    await async;
                    t = async.Result[0];
                }
                else*/
                {
                    t = GameObject.Instantiate(listPrefab);
                }
                try
                {

                    t.enabled = true;
                    t.transform.SetParent(parent);
                    t.transform.localScale = Vector3.one;
                    t.index = i;
                    await t.InitData(componentData[i], SelectAction, toggleGroup);
                    list.Add(t);
                }
                catch (Exception e)
                {
                    Debug.LogError($"i:{i}-count:{componentData.Length}--{e}");
                }

            }
        }
        LayoutRebuilder.MarkLayoutForRebuild(parent as RectTransform);
    }
    public void RemoveIndex(int index)
    {
        if (list.Count > index)
        {
            list[index].enabled = false;
            list[index].transform.localScale = Vector3.zero;
            list[index].ClearData();
            if (index != _dataCount - 1)
            {
                var removeItem = list[index];
                list[index] = list[_dataCount - 1];
                list[_dataCount - 1] = removeItem;
            }
            _dataCount--;
        }
    }
    public T RemoveIndexFromParent(int index, Transform newParent)
    {
        if (list.Count > index)
        {
            var removeItem = list[index];
            if (index != _dataCount - 1)
            {
                list[index] = list[_dataCount - 1];
            }
            removeItem.transform.SetParent(newParent);
            _dataCount--;
            return removeItem;
        }
        return null;
    }
    public void AddListItem(T t)
    {
        t.transform.SetParent(parent);
        list.Add(t);
    }

    public void AddListData(V componentData, SelectAction<V> SelectAction = null, ToggleGroup toggleGroup = null)
    {

        if (componentData == null)
        {
            for (int i = 0; i < list.Count; i++)
            {
                list[i].ClearData();
                list[i].enabled = false;
                list[i].transform.localScale = zero;
            }
            _dataCount = 0;
            return;
        }
        if (list.Count > _dataCount)
        {
            list[_dataCount].ClearData();
            list[_dataCount].enabled = true;
            list[_dataCount].transform.localScale = Vector3.one;
            // AddListData 是同步增量接口，单项初始化异常统一记录。
            AsyncTaskRunner.Run(list[_dataCount].InitData(componentData, SelectAction, toggleGroup), nameof(AddListData));
            _dataCount++;
        }
        else
        {
            T t;
            /*
            if (Async)
            {
                var async = GameObject.InstantiateAsync(listPrefab);
                await async;
                t = async.Result[0];
            }
            else*/
            {
                t = GameObject.Instantiate(listPrefab);
            }
            try
            {

                t.enabled = true;
                t.transform.SetParent(parent);
                t.transform.localScale = Vector3.one;
                t.index = _dataCount;
                AsyncTaskRunner.Run(t.InitData(componentData, SelectAction, toggleGroup), nameof(AddListData));
                list.Add(t);
                _dataCount++;
            }
            catch (Exception e)
            {

            }

        }

        LayoutRebuilder.MarkLayoutForRebuild(parent as RectTransform);
    }
    public async Task InitListData(List<V> componentData, SelectAction<V> SelectAction = null, ToggleGroup toggleGroup = null, bool Async = true)
    {

        if (componentData == null || componentData.Count == 0)
        {
            for (int i = 0; i < list.Count; i++)
            {
                list[i].ClearData();
                list[i].enabled = false;
                list[i].transform.localScale = Vector3.zero;
            }
            _dataCount = 0;
            return;
        }
        _dataCount = componentData.Count;
        for (int i = list.Count - 1; i > componentData.Count - 1; i--)
        {
            list[i].ClearData();
            list[i].enabled = false;
            list[i].transform.localScale = Vector3.zero;
        }

        for (int i = 0; i < componentData.Count; i++)
        {
            if (list.Count > i)
            {
                list[i].enabled = true;
                list[i].transform.localScale = Vector3.one;
                await list[i].InitData(componentData[i], SelectAction, toggleGroup);
            }
            else
            {
                T t;
                /*
                if (Async)
                {
                    var async = GameObject.InstantiateAsync(listPrefab);
                    await async;
                    t = async.Result[0];
                }
                else*/
                {
                    t = GameObject.Instantiate(listPrefab);
                }
                try
                {
                    t.index = i;
                    t.enabled = true;
                    t.transform.SetParent(parent);
                    t.transform.localScale = Vector3.one;
                    await t.InitData(componentData[i], SelectAction, toggleGroup);
                    list.Add(t);
                    t.name = i.ToString();
                }
                catch (Exception e)
                {
                    Debug.LogError($"i:{i}-count:{componentData.Count}--{e}");
                }

            }
        }
        LayoutRebuilder.MarkLayoutForRebuild(parent as RectTransform);
    }
    public void ClearAll()
    {
        for (int i = 0; i < list.Count; i++)
        {
            list[i].ClearData();
            list[i].enabled = false;
            list[i].transform.localScale = Vector3.zero;
        }
        _dataCount = 0;
    }
}
