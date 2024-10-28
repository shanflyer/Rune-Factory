
using UnityEngine;
# if UNITY_EDITOR
using UnityEditor;
#endif

public class LanguageSwitchDataList : ScriptableObject, IGameData
{
#if UNITY_EDITOR
    LanguageSwitchEditorData[] datas;

    public void SetReferenceData()
    {
        languageDatas.Clear();
        for (int i = 0; i < datas.Length; i++)
        {
            languageDatas[datas[i].cn] = new LanguageSwitchData
            {
                en = datas[i].en,
                jp = datas[i].jp,
                ko = datas[i].ko
            };
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
[SerializeField]
public struct LanguageSwitchData
{
    public string en, jp, ko;
}
public struct LanguageSwitchEditorData
{
    public string cn, en, jp, ko;
}