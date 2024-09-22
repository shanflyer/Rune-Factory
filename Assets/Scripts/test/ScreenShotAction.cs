using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        /*
        if (Input.GetKeyDown(KeyCode.S))
        {
            shotnum++;
            ScreenCapture.CaptureScreenshot("Shot" + System.DateTime.Now.Day + System.DateTime.Now.Hour + System.DateTime.Now.Minute + System.DateTime.Now.Second + shotnum + ".png");
        };*/
    }
#endif

}
