//#if ENABLE_INPUT_SYSTEM && UNITY_INPUT_SYSTEM_EXISTS
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class NoSensorPanel : MonoBehaviour
{
    string[] lines;

    void Start() => Refresh();

    void Refresh()
    {
        var devs = InputSystem.devices
            .Select(d => d.GetType().Name + " - " + d.displayName)
            .OrderBy(s => s)
            .ToArray();
        lines = devs.Length == 0 ? new[] {"<no devices>"} : devs;
    }

    void OnEnable()  { InputSystem.onDeviceChange += (_,__) => Refresh(); }
    void OnDisable() { InputSystem.onDeviceChange -= (_,__) => Refresh(); }

    void OnGUI()
    {
        GUI.Box(new Rect(10,10,560,280), "Input Devices (runtime)");
        var area = new Rect(20,40,540,240);
        GUILayout.BeginArea(area);
        foreach (var l in lines) GUILayout.Label(l);
        GUILayout.EndArea();
    }
}
//#endif