using System;
using UnityEngine;
#if UNITY_EDITOR
#endif

public class LanguageSwitchDataList : ScriptableObject, IGameData
{
#if UNITY_EDITOR
    LanguageSwitchData[] datas;

    public void SetReferenceData()
    {
        languageDatas.Clear();
        for (int i = 0; i < datas.Length; i++)
        {
            languageDatas[datas[i].cn] = datas[i];
        }
    }
#endif
    public override string ToString()
    {
        return "LanguageSwitchDataList";
    }
    public string GetKey()
    {
        return "LanguageSwitchDataList";
    }

    public StringLanguageSwitchDataDictionary languageDatas = new StringLanguageSwitchDataDictionary();


}
[Serializable]
public class LanguageSwitchData
{
    public string cn, en, tw, ja, ko, fr, de, ru, es, pt, it, tr, vi, th, pl, nl, el, ar, hi, ur, ms, id,
        he,
        sv,
        cs,
        uk,
        ro,
        no,
        hu,
        sw,
        sr,
        fi,
        da;
}
