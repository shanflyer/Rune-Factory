using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InformationPanel : GamePanel
{
    [SerializeField]
    Text info;
    [SerializeField]
    Transform infoParent;
    public override void SetPanelUISerializeObj()
    {
        base.SetPanelUISerializeObj();
        infoParent = FindChildGameObject("Parent");
        info=FindChildGameObject<Text>("Info");
        info.transform.localScale = Vector3.zero;
    }
    public void RefreshInformations(List<string> strs,int startIndex)
    {
        int index = 0;
        for(int i = startIndex; i < strs.Count; i++)
        {
            if (infoParent.childCount > index)
            {
                Transform child=infoParent.GetChild(index);
                child.GetComponent<Text>().text = strs[i];
                child.localScale = Vector3.one;
            }
            else
            {
                Text infoObj = Instantiate(info, infoParent, false);
                infoObj.text = strs[i];
                infoObj.transform.localScale = Vector3.one;
            }
            index++;
        }
        if (index > 0)
        {
            for(int i=0;i<startIndex;i++)
            {
                if (infoParent.childCount > index)
                {
                    Transform child = infoParent.GetChild(index);
                    child.GetComponent<Text>().text = strs[i];
                    child.localScale = Vector3.one;
                }
                else
                {
                    Text infoObj = Instantiate(info, infoParent, false);
                    infoObj.text = strs[i];
                    infoObj.transform.localScale = Vector3.one;
                }
                index++;
            }
        }

        if(infoParent.childCount > strs.Count)
        {
            for(int i = strs.Count - 1; i < infoParent.childCount; i++)
            {
                infoParent.GetChild(i).localScale = Vector3.zero;
            }
        }
    }
    public void AddInfo(string information,bool cycle=false)
    {
        if (cycle&&infoParent.childCount>0)
        {
            Transform child = infoParent.GetChild(0);
            child.GetComponent<Text>().text = information;
            child.SetAsLastSibling();
        }
        else
        {
            Text infoObj = Instantiate(info, infoParent, false);
            infoObj.text = information;
            infoObj.transform.localScale = Vector3.one;
        }
    }
}
