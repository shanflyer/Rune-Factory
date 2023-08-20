using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class CommonItem 
{
    public static Texture eye
    {
        get
        {
            if (_eye == null)
            {
                _eye = AssetDatabase.LoadAssetAtPath<Texture>("Assets/Codes/Editor/Source/eye.png");
            }
            return _eye;
        }
    }
    private static Texture _eye;

    public static Texture eye1
    {
        get
        {
            if (_eye1 == null)
            {
                _eye1 = AssetDatabase.LoadAssetAtPath<Texture>("Assets/Codes/Editor/Source/eye1.png");
            }
            return _eye1;
        }
    }
    private static Texture _eye1;
    public static GUIStyle selectToggle
    {
        get
        {
            if (_selectToggle == null)
            {
                _selectToggle = mySkin.customStyles[4];

            }
            return _selectToggle;
        }
    }
    private static GUIStyle _selectToggle;

    public static GUIStyle selectButton
    {
        get
        {
            if (_selectButton == null)
            {
                _selectButton = mySkin.toggle;

            }
            return _selectButton;
        }
    }
    private static GUIStyle _selectButton;
    public static GUISkin mySkin
    {
        get
        {
            if (_mySkin == null)
            {
                _mySkin = AssetDatabase.LoadAssetAtPath<GUISkin>("Assets/Editor/Source/MyGUISkin.guiskin");
            }
            return _mySkin;
        }
    }
    private static GUISkin _mySkin;
}
