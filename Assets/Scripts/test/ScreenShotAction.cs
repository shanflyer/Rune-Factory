using System.Collections;
using System.Collections.Generic;
using System.IO;
 
using UnityEngine;
using UnityEngine.InputSystem;
#if UNITY_EDITOR
using UnityEditor;
#endif
public class ScreenShotAction : MonoBehaviour
{
#if UNITY_EDITOR
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
            MyLanguage myLanguage = GameController.instance.SetSystemLanguage;
            if (!Directory.Exists(myLanguage.ToString()))
            {
                Directory.CreateDirectory(myLanguage.ToString());
            }

            ScreenCapture.CaptureScreenshot(myLanguage.ToString() + "/" + System.DateTime.Now.Day + System.DateTime.Now.Hour + System.DateTime.Now.Minute + System.DateTime.Now.Second + shotnum + ".png");
        }
        else
        {
            if (!Directory.Exists("Editor_Image"))
            {
                Directory.CreateDirectory("Editor_Image");
            }

            ScreenCapture.CaptureScreenshot("Editor_Image/" + System.DateTime.Now.Day + System.DateTime.Now.Hour + System.DateTime.Now.Minute + System.DateTime.Now.Second + shotnum + ".png");
        }
       
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