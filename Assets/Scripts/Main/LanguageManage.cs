using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI; 
public class LanguageManage : Singleton<LanguageManage>
{  
    static List<SystemLanguage> Languages=new List<SystemLanguage> 
    {
        SystemLanguage.Chinese,
        SystemLanguage.Japanese,
        SystemLanguage.Korean,
        SystemLanguage.English
    }; 

    public static SystemLanguage nowLanguage;
    public LanguageSwitchDataList LanguageSwitchDataList { get; private set; }
    public override async void Init()
    {
        base.Init();
        LanguageSwitchDataList =await  GameDataManager.instance.GetAsyncData<LanguageSwitchDataList>("LanguageSwitchDataList");
        TMP_Text.SwitchString = SwitchStr;
    }
    protected override void Clear()
    {
        base.Clear();
        TMP_Text.SwitchString = null;
    }
    public static void TextFanyi(Text text)
    {
        text.text = SwitchStr(text.text);
    }

    public void SystemLanguageMatch(bool SetLanguage, SystemLanguage SetSystemLanguage)
    {
        bool isMatch = false;
        if (!SetLanguage)
        { 
            nowLanguage = Application.systemLanguage;
            if (nowLanguage == SystemLanguage.ChineseSimplified || nowLanguage == SystemLanguage.ChineseTraditional ||
                nowLanguage == SystemLanguage.Chinese)
            {
                nowLanguage = SystemLanguage.Chinese;
            }
            foreach (var systemLanguage in Languages)
            {
                if (systemLanguage == nowLanguage)
                {
                    isMatch = true;
                    break;
                }
            }
            if (!isMatch)
            {
                nowLanguage=SystemLanguage.English;
            }
        }
        else
        {
            nowLanguage = SetSystemLanguage;
            foreach (var systemLanguage in Languages)
            {
                if (systemLanguage == nowLanguage)
                {
                    isMatch = true;
                    break;
                }
            }
            if (!isMatch)
            {
                nowLanguage = SystemLanguage.English;
            }
        }
        
    }
    public static string SwitchStr(object source, params object[] args)
    {

        source = SwitchStr(source.ToString());

        if (args != null)
        {
            for (int i = 0; i < args.Length; i++)
            {
                args[i] = SwitchStr(args[i].ToString());
            }
        }

        if (args != null)
        {
            var builder = new StringBuilder(source.ToString());
            for (int i = 0; i < args.Length; i++)
            {
                var span = args[i].ToString().AsSpan();
                builder.Append(span);
            }
           return builder.ToString();
        }
        else
        {
            return source.ToString();
        }

    }
    public static string SwitchStr(string s)
    {
        if (instance == null|| instance.LanguageSwitchDataList==null)
        {
            return s;
        }
        s = instance.LanguageSwitchDataList.GetValue(nowLanguage, s);
        return s;
    }
    public static string SwitchStr(object obj)
    {
        string s = obj.ToString();
        s = instance.LanguageSwitchDataList.GetValue(nowLanguage, s);
        return s;
    }  
    public string GameTimeToString(int year,Season season,int day)
    {
        string result = "";
        if(nowLanguage == SystemLanguage.English)
        {

        }
        else
        {
            result = $"{year}年 {season} {day}日";
        }

        return result;
    }
}
