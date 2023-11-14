using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InformationPanel : GamePanel<IReferenceData>
{
    [SerializeField]
    TextMeshProUGUI info;
    [SerializeField]
    Transform infoParent;
    [SerializeField]
    Button close;
    public override void SetPanelUISerializeObj()
    {
        base.SetPanelUISerializeObj();
        infoParent = FindChildGameObject("Content");
        info=FindChildGameObject<TextMeshProUGUI>("Info");
        info.transform.localScale = Vector3.zero;
        close = FindChildGameObject<Button>("Close");
        close.onClick.AddListener(Close);
    }
    public void RefreshInformations(List<string> strs,int startIndex)
    {
        int index = 0;
        for(int i = startIndex; i < strs.Count; i++)
        {
            if (infoParent.childCount > index)
            {
                Transform child=infoParent.GetChild(index);
                child.GetComponent<TextMeshProUGUI>().text = strs[i];
                child.localScale = Vector3.one;
            }
            else
            {
                TextMeshProUGUI infoObj = Instantiate(info, infoParent, false);
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
                    child.GetComponent<TextMeshProUGUI>().text = strs[i];
                    child.localScale = Vector3.one;
                }
                else
                {
                    TextMeshProUGUI infoObj = Instantiate(info, infoParent, false);
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
            child.GetComponent<TextMeshProUGUI>().text = information;
            child.SetAsLastSibling();
        }
        else
        {
            TextMeshProUGUI infoObj = Instantiate(info, infoParent, false);
            infoObj.text = information;
            infoObj.transform.localScale = Vector3.one;
        }
    }
}
