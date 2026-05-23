using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;

public enum MyLanguage
{
    NULL = -1,
    英语 = 0,
    简体中文 = 1,
    繁体中文 = 2,
    日语 = 3,
    韩语 = 4,
    法语 = 5,
    德语 = 6,
    意大利语 = 7,
    西班牙语 = 8,
    葡萄牙语 = 9,
    土耳其语 = 10,
    越南语 = 11,
    泰语 = 12,
    波兰语 = 13,
    荷兰语 = 14,
    希腊语 = 15,
    阿拉伯语 = 16,
    印地语 = 17,
    乌尔都语 = 18,
    马来语 = 19,
    印尼语 = 20,
    俄语 = 21,
    希伯来语 = 22,
    瑞典语 = 23,
    捷克语 = 24,
    乌克兰语 = 25,
    罗马尼亚语 = 26,
    挪威语 = 27,
    匈牙利语 = 28,
    斯瓦希里语 = 29,
    塞尔维亚语 = 30,
    芬兰语 = 31,
    丹麦语 = 32
}
public class LanguageManage : Singleton<LanguageManage>
{
    private LanguageSwitchDataList LanguageSwitchDataList;

    private Dictionary<string, MyLanguage> LocalLanguages = new Dictionary<string, MyLanguage>();
    private Dictionary<MyLanguage,LanguageData> allLanguages=new Dictionary<MyLanguage, LanguageData>();
    private Dictionary<string,FieldInfo> languageFields=new Dictionary<string, FieldInfo>();
     
    public static MyLanguage nowLanguage;
    FieldInfo nowFieldInfo;
    public bool isRTL = false;
    private Task initializationTask = Task.CompletedTask;
    public override Task InitializationTask => initializationTask;

    public bool IsRTL()
    {
        return isRTL;
    }
    public List<LanguageData> languageDatas => allLanguages.Values.ToList();
    public override void Init()
    {
        base.Init();
        initializationTask = InitAsync();
    }

    private async Task InitAsync()
    {
        TMPTextLocalization.SwitchString = SwitchStr;
        TMPTextLocalization.NowLineSpacing = GetLineSpacing;
        TMPTextLocalization.NowCharacterSpacing = GetCharacterSpacing;
        TMPTextLocalization.IsRTL = IsRTL;

        LanguageSwitchDataList =GameDataManager.instance.GetData<LanguageSwitchDataList>("LanguageSwitchDataList");
        if (LanguageSwitchDataList == null)
        {
            Debug.LogError("LanguageManage init failed: missing LanguageSwitchDataList.");
        }
        languageFields.Clear();
        Type type = typeof(LanguageSwitchData);
        var fields = type.GetFields();
        for(int i = 0; i < fields.Length; i++)
        {
            languageFields[fields[i].Name] = fields[i];
        }

        allLanguages = new Dictionary<MyLanguage, LanguageData>();
        var Languages = await GameDataManager.instance.GetAllAsyncData<LanguageData>();
        for(int i = 0; i < Languages.Count; i++)
        {
            var languageData = Languages[i];
            allLanguages[languageData.languageType] = languageData;
            for(int j = 0; j < languageData.localName.Count; j++)
            {
                LocalLanguages.Add(languageData.localName[j], languageData.languageType);
            }
        }
    }
    protected override void Clear()
    {
        base.Clear();
        initializationTask = Task.CompletedTask;
        TMPTextLocalization.SwitchString = null;
    }

    static Dictionary<string, string> replacements = new Dictionary<string, string>
    {
        //{"\\n","\n" },
        {
            "\r\n","\n"
        }
    };
    public string SwitchString(string source)
    {
        if (string.IsNullOrEmpty(source))
        {
            return source;
        }
        source = SpanStringReplacer.ReplaceMultipleStrings(source, replacements);
        if (LanguageSwitchDataList != null && LanguageSwitchDataList.languageDatas.TryGetValue(source,out var languageSwitchData) && nowFieldInfo!=null)
        {
          return  nowFieldInfo.GetValue(languageSwitchData).ToString();
        }
        return source;
    } 
    public void SetLanguage(MyLanguage myLanguage)
    {
        PlayerPrefs.SetInt("MyLanguage", (int)myLanguage);
        GetLocalLanguage(myLanguage);

        // 切换语言只需要遍历文本组件，不依赖对象排序。
        var allTMP_Text = Object.FindObjectsByType<TMP_Text>();
        for(int i = 0; i < allTMP_Text.Length; i++)
        {
            allTMP_Text[i].FixedSwitchString();
        }

        var languageSwitchImages=Object.FindObjectsByType<LanguageSwitchImage>();
        for(int i = 0; i < languageSwitchImages.Length; i++)
        {
            languageSwitchImages[i].SetImage(myLanguage);
        }
    }

    static float nowLineSpacing = 0,nowCharacterSpacing;

    private void GetLocalLanguage(MyLanguage overrideLanguage = MyLanguage.NULL)
    {
        if (overrideLanguage == MyLanguage.NULL)
        {
            int value = PlayerPrefs.GetInt("MyLanguage");
            if (value > 0)
            {
                nowLanguage = (MyLanguage)value;
            }
            else
            {
                // 获取系统文化信息
                var culture = CultureInfo.CurrentUICulture;
                if (!LocalLanguages.TryGetValue(culture.Name, out nowLanguage))
                {
                    nowLanguage = MyLanguage.英语;
                }
            }
        }
        else
        {
            nowLanguage = overrideLanguage;
        }
        isRTL = false;
        if(allLanguages.TryGetValue(nowLanguage,out var languageData))
        {
            nowLineSpacing = languageData.lineSpacing;
            if (languageFields.TryGetValue(languageData.FieldName, out nowFieldInfo))
            {
                isRTL = languageData.rightStart;
                return;
            }
        }
        languageFields.TryGetValue("en", out nowFieldInfo);
    }

    public void SystemLanguageMatch(MyLanguage SetSystemLanguage = MyLanguage.NULL)
    {
        GetLocalLanguage(SetSystemLanguage); 
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

    public static string SwitchFormatStr(object source, params object[] args)
    {
        source = SwitchStr(source.ToString());

        if (args != null)
            for (var i = 0; i < args.Length; i++)
                args[i] = SwitchStr(args[i].ToString());

        if (args != null) return string.Format(source.ToString(), args);

        return source.ToString();
    }
    public static float GetLineSpacing()
    {
        return nowLineSpacing;
    }
    public static float GetCharacterSpacing()
    {
        return nowCharacterSpacing;
    }
    public static string SwitchStr(string s)
    {
        if (instance == null)
        {
            return s;
        }
        s = instance.SwitchString(s);
        return s;
    }
    public static string SwitchStr(object obj)
    {
        string s = obj.ToString();
        s = instance.SwitchString(s);
        return s;
    }  
    public string GameTimeToString(int year,Season season,int day)
    {
        string result = "";
        if(allLanguages.TryGetValue(nowLanguage,out var languageData))
        {
            result=string.Format(languageData.timeStr,year, SwitchStr(season), day);
        } 
        return result;
    }
    public string GameTimeToString(int year, Season season)
    {
        string result = "";
        if (allLanguages.TryGetValue(nowLanguage, out var languageData))
        {
            result = string.Format(languageData.timeStr, year,SwitchStr(season),"");
        }
        return result;
    }
}
