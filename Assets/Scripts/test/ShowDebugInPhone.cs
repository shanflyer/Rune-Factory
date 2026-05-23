using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class logdata
{
    public string output = "";
    public string stack = "";
    public static logdata Init(string o, string s)
    {
        logdata log = new logdata();
        log.output = o;
        log.stack = s;
        return log;
    }
    public void Show(/*bool showstack*/)
    {
        GUILayout.Label(output);
        //if (showstack)  
        GUILayout.Label(stack);
    }
}
/// <summary>  
/// 手机调试脚本  
/// 本脚本挂在一个空对象或转换场景时不删除的对象即可  
/// 错误和异常输出日记路径 Application.persistentDataPath  
/// </summary>  
public class ShowDebugInPhone : MonoBehaviour
{
    List<logdata> logDatas = new List<logdata>();//log链表  
    List<logdata> errorDatas = new List<logdata>();//错误和异常链表  
    List<logdata> warningDatas = new List<logdata>();//警告链表  
    static List<string> mWriteLogTxt = new List<string>();
    static List<string> mWriteErrorTxt = new List<string>();
    static List<string> mWriteWarningTxt = new List<string>();
    Vector2 uiLog;
    Vector2 uiError;
    Vector2 uiWarning;
    public bool open = false;
    public bool showLog = true;
    public bool showError = true;
    public bool showWarning = false;
    private string outpathLog;
    private string outpathError;
    private string outpathWarning;
    void Start()
    {
        //Application.persistentDataPath Unity中只有这个路径是既可以读也可以写的。  
        //Debug.Log(Application.persistentDataPath);
        outpathLog = Application.persistentDataPath + @"/outLog.txt";
        outpathError = Application.persistentDataPath + @"/outLogError.txt";
        outpathWarning = Application.persistentDataPath + @"/outLogWarining.txt";

        //每次启动客户端删除之前保存的Log  
        if (File.Exists(outpathLog))
        {
            File.Delete(outpathLog);
        }
        if (File.Exists(outpathError))
        {
            File.Delete(outpathError);
        }
        if (File.Exists(outpathWarning))
        {
            File.Delete(outpathWarning);
        }
        //转换场景不删除  
        Application.DontDestroyOnLoad(gameObject);
    }
    async void OnEnable()
    {
#if UNITY_EDITOR
        Application.RegisterLogCallback(HangleLog);
#else
        if (await CloudRemoteConfig.instance.GetConfigBool("LogShow"))
        {
            Application.RegisterLogCallback(HangleLog);
        }
#endif 
        //注册log监听  

    }
    void OnDisable()
    {
        // Remove callback when object goes out of scope  
        //当对象超出范围,删除回调。  
        Application.RegisterLogCallback(null);
    }
    void HangleLog(string logString, string stackTrace, LogType type)
    {
        switch (type)
        {
            case LogType.Log:
                logDatas.Add(logdata.Init(logString, stackTrace));
                mWriteLogTxt.Add(logString);
                mWriteLogTxt.Add(stackTrace);
                break;
            case LogType.Error:
            case LogType.Exception:
                errorDatas.Add(logdata.Init(logString, stackTrace));
                mWriteErrorTxt.Add(logString);
                mWriteErrorTxt.Add(stackTrace);
                break;
            case LogType.Warning:
                warningDatas.Add(logdata.Init(logString, stackTrace));
                mWriteWarningTxt.Add(logString);
                mWriteWarningTxt.Add(stackTrace);
                break;
        }
    }

    public void SetLogData()
    {
        if (logDatas.Count > 0)
        {
            string[] temp = mWriteLogTxt.ToArray();

            FileStream fileStream = new FileStream(outpathLog, FileMode.Append);
            StreamWriter sw = new StreamWriter(fileStream);
            foreach (string t in temp)
            {

                sw.WriteLine(t);
                mWriteLogTxt.Remove(t);
            }
            sw.Close();
        }
        if (errorDatas.Count > 0)
        {
            string[] temp = mWriteErrorTxt.ToArray();

            FileStream fileStream = new FileStream(outpathError, FileMode.Append);
            StreamWriter sw = new StreamWriter(fileStream);
            foreach (string t in temp)
            {

                sw.WriteLine(t);
                mWriteErrorTxt.Remove(t);
            }
            sw.Close();
        }
        if (warningDatas.Count > 0)
        {
            string[] temp = mWriteWarningTxt.ToArray();

            FileStream fileStream = new FileStream(outpathWarning, FileMode.Append);
            StreamWriter sw = new StreamWriter(fileStream);
            foreach (string t in temp)
            {

                sw.WriteLine(t);
                mWriteWarningTxt.Remove(t);
            }
            sw.Close();
        }
    }

    void Update()
    { 
        //因为写入文件的操作必须在主线程中完成,所以在Update中才给你写入文件。  
        
    }
    void OnGUI()
    {
        /*
        GUILayout.BeginHorizontal();
        if (GUILayout.Button(">>Open", GUILayout.Height(100), GUILayout.Width(100)))
            open = !open;
        if (open)
        {
            if (GUILayout.Button("清理", GUILayout.Height(100), GUILayout.Width(100)))
            {
                logDatas = new List<logdata>();
                errorDatas = new List<logdata>();
                warningDatas = new List<logdata>();
            }
            if (GUILayout.Button("显示log日志:" + showLog, GUILayout.Height(50), GUILayout.Width(100)))
            {
                showLog = !showLog;
                if (open == true)
                    open = !open;
            }
            if (GUILayout.Button("显示error日志:" + showError, GUILayout.Height(50), GUILayout.Width(100)))
            {
                showError = !showError;
                if (open == true)
                    open = !open;
            }
            if (GUILayout.Button("显示warning日志:" + showWarning, GUILayout.Height(50), GUILayout.Width(100)))
            {
                showWarning = !showWarning;
                if (open == true)
                    open = !open;
            }
            if (GUILayout.Button("保存" + showWarning, GUILayout.Height(50), GUILayout.Width(100)))
            {
               SetLogData();
            }
        }
        GUILayout.EndHorizontal();
        */
        if (showLog)
        {
            GUI.color = Color.white;
            uiLog = GUILayout.BeginScrollView(uiLog);
            foreach (var va in logDatas)
            {
                va.Show();
            }
            GUILayout.EndScrollView();
        }
        
        if (showError)
        {
            GUI.color = Color.red;
            uiError = GUILayout.BeginScrollView(uiError);
            foreach (var va in errorDatas)
            {
                va.Show();
            }
            GUILayout.EndScrollView();
        }
        if (showWarning)
        {
            GUI.color = Color.yellow;
            uiWarning = GUILayout.BeginScrollView(uiWarning);
            foreach (var va in warningDatas)
            {
                va.Show();
            }
            GUILayout.EndScrollView();
        }
        
    }
}
