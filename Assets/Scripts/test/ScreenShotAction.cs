using System.Collections;
using System.Collections.Generic;
using System.IO;
 
using UnityEngine;
using UnityEngine.InputSystem; 

public class ScreenShotAction : MonoBehaviour
{
#if UNITY_EDITOR
    public static int shotnum;
    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    { 
        if (Keyboard.current.f1Key.wasPressedThisFrame)
        {
            shotnum++;
            MyLanguage myLanguage = GameController.instance.SetSystemLanguage;
            if (!Directory.Exists(myLanguage.ToString()))
            {
                Directory.CreateDirectory(myLanguage.ToString());
            }
            
            ScreenCapture.CaptureScreenshot(myLanguage.ToString()+"/" + System.DateTime.Now.Day + System.DateTime.Now.Hour + System.DateTime.Now.Minute + System.DateTime.Now.Second + shotnum + ".png");
        }; 
    }
#endif

}
