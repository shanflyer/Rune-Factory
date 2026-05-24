using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class LanguageData : ScriptableObject, IGameData,IReferenceData
{
    public string LanguageName;
    public List<string> localName;
    public string FieldName;
    public string ShowName;
    public bool rightStart;
    public MyLanguage languageType;
    public string timeStr;
    public float lineSpacing;
    public float characterSpacing;
    public string GetKey()
    {
        return languageType.ToString();
    }
    public override string ToString()
    {
        return languageType.ToString();
    }
    public void SetReferenceData()
    {
    }


}
