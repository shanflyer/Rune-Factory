using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;
#if UNITY_EDITOR
using System;
using UnityEditor;
#endif
public class ScreenShotAction : MonoBehaviour
{
#if UNITY_EDITOR
    public bool autoChangeLanguage;
    public static int shotnum;
    // Use this for initialization
    void Start()
    {

    }
    public void ScreenShot()
    {
        shotnum++;
        if (Application.isPlaying)
        {
            if (!autoChangeLanguage)
            {
                var myLanguage = GameController.instance.SetSystemLanguage;
                if (!Directory.Exists(myLanguage.ToString())) Directory.CreateDirectory(myLanguage.ToString());

                ScreenCapture.CaptureScreenshot(myLanguage + "/" + DateTime.Now.Day + DateTime.Now.Hour +
                                                DateTime.Now.Minute + DateTime.Now.Second + shotnum + ".png");
            }
            else
            {
                StartCoroutine(AutoScreenShot());
            }
       
        }
        else
        {
            if (!Directory.Exists("Editor_Image"))
            {
                Directory.CreateDirectory("Editor_Image");
            }

            ScreenCapture.CaptureScreenshot("Editor_Image/" + DateTime.Now.Day + DateTime.Now.Hour +
                                            DateTime.Now.Minute + DateTime.Now.Second + shotnum + ".png");
        }
       
    }

    private IEnumerator AutoScreenShot()
    {
        float timeValue = 0;
        var myLanguages = new List<MyLanguage>();
        foreach (var value in Enum.GetValues(typeof(MyLanguage)))
        {
            var myLanguage = (MyLanguage)value;
            myLanguages.Add(myLanguage);
        }

        var index = 0;
        var setIndex = false;
        while (index < myLanguages.Count)
        {
            timeValue += Time.deltaTime;
            yield return 0;
            if (timeValue >= 0.25f && !setIndex)
            {
                GameController.instance.SetSystemLanguage = myLanguages[index];
                setIndex = true;
                index++;
            }

            if (timeValue >= 0.5f)
            {
                if (!Directory.Exists(GameController.instance.SetSystemLanguage.ToString()))
                    Directory.CreateDirectory(GameController.instance.SetSystemLanguage.ToString());

                ScreenCapture.CaptureScreenshot(GameController.instance.SetSystemLanguage + "/" + DateTime.Now.Day +
                                                DateTime.Now.Hour +
                                                DateTime.Now.Minute + DateTime.Now.Second + shotnum + ".png");

                setIndex = false;
                timeValue = 0;
            }
        }

        Debug.Log("auto 输出完毕！！");
    }
    // Update is called once per frame
    void Update()
    { 
        if (Keyboard.current.f1Key.wasPressedThisFrame)
        {
            ScreenShot();
        }; 
    }
#endif

}
#if UNITY_EDITOR
[CustomEditor(typeof(ScreenShotAction))]
public class ScreenShotActionEditor : Editor
{
    public ScreenShotAction ScreenShotAction => target as ScreenShotAction;
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (GUILayout.Button("导出"))
        {
            ScreenShotAction.ScreenShot();
        }
    }
}
#endif