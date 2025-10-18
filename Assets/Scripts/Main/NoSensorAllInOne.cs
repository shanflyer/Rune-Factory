using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
#endif

/// 一体化：
/// 1) 关闭非输入系统传感器（陀螺/指南针/定位）+ 固定方向
/// 2) 真机构建时，仅保留触屏（可选手柄）；移除 FastKeyboard/FastMouse 等映射与所有“传感器嫌疑”设备
///    （Editor 中不移除键盘/鼠标，避免调试时报错）
/// 3) 屏幕叠加监视（尺寸/位置/透明度可调）
[DefaultExecutionOrder(-10000)]
public class NoSensorAllInOne : MonoBehaviour
{
    [Header("方向/旋转")]
    public bool disableAutoRotation = true;
    public ScreenOrientation fixedOrientation = ScreenOrientation.Portrait;

    [Header("旧API复查间隔（秒）")]
    public float legacyRecheckInterval = 2.5f;

    [Header("输入系统白名单")]
    public bool allowTouchscreen = true; // 建议保持 true
    public bool allowGamepad = false;    // 需要手柄再开

    [Header("监视UI")]
    public bool showOverlay = true;
    public float overlayRefreshSec = 1f;

    [Header("叠加层尺寸/位置")]
    public int   overlayFontSize = 11;      // 小字号
    public float overlayWidthRatio = 0.50f; // 盒子宽度占屏宽
    public float overlayBgAlpha = 0.45f;    // 背景透明度
    public float overlayX = 10f;            // 左上角X
    public float overlayY = 10f;            // 左上角Y

    // 内部
    float _uiTimer;
    string[] _lines = Array.Empty<string>();
    bool _danger;

#if ENABLE_INPUT_SYSTEM
    float _aggressiveCleanLeft = 3.0f; // 启动3秒内每帧清理一次，兜住迟到设备（仅真机启用）
#endif

    // 传感器关键词（字符串匹配，避免类型依赖）
    static readonly string[] SensorKeywords = {
        "Accelerometer","Gyroscope","Compass","Magnetometer",
        "Attitude","Orientation","Gravity","LinearAcceleration",
        "Barometer","LightSensor","Proximity","Step","Pedometer","RotationVector"
    };

    void Awake()
    {
        // 固定方向，避免自动旋转触发传感器
        if (disableAutoRotation)
        {
            Screen.autorotateToPortrait = false;
            Screen.autorotateToPortraitUpsideDown = false;
            Screen.autorotateToLandscapeLeft = false;
            Screen.autorotateToLandscapeRight = false;
        }
        Screen.orientation = fixedOrientation;

        // 旧API一次关 + 定时复查
        KillLegacySensorsOnce();
        if (legacyRecheckInterval > 0f)
            InvokeRepeating(nameof(KillLegacySensorsOnce), 1f, legacyRecheckInterval);

#if ENABLE_INPUT_SYSTEM
        // 改善触屏支持（安全 try）
        try { EnhancedTouchSupport.Enable(); } catch {}

        // —— 设备白名单/清理：只在“真机 Player”里做，不在 Editor 做 —— //
        #if !UNITY_EDITOR
        foreach (var d in InputSystem.devices.ToArray())
            RemoveIfNotAllowed(d);
        InputSystem.onDeviceChange += OnDeviceChange;
        #endif
#endif
    }

    void OnDestroy()
    {
#if ENABLE_INPUT_SYSTEM
        #if !UNITY_EDITOR
        InputSystem.onDeviceChange -= OnDeviceChange;
        #endif
        try { EnhancedTouchSupport.Disable(); } catch {}
#endif
    }

    // —— 非输入系统传感器关闭 —— //
    void KillLegacySensorsOnce()
    {
        if (Input.gyro.enabled)        Input.gyro.enabled = false;
        if (Input.compass.enabled)     Input.compass.enabled = false;
        if (Input.location.status == LocationServiceStatus.Running)
            Input.location.Stop();
    }

#if ENABLE_INPUT_SYSTEM && !UNITY_EDITOR
    // —— 输入系统：白名单 & 传感器嫌疑移除（仅真机）—— //
    void OnDeviceChange(InputDevice d, InputDeviceChange c)
    {
        if (c == InputDeviceChange.Added) RemoveIfNotAllowed(d);
    }

    static bool LooksLikeTouchscreen(InputDevice d)
    {
        var tn = d.GetType().Name;                 // Touchscreen / Pointer / AndroidTouch...
        var layout = d.layout ?? string.Empty;     // Touchscreen
        var name = d.displayName ?? string.Empty;  // 可能含 Touch
        var prod = d.description.product ?? string.Empty;

        if (layout.Equals("Touchscreen", StringComparison.OrdinalIgnoreCase)) return true;
        if (tn.IndexOf("Touch", StringComparison.OrdinalIgnoreCase)   >= 0)   return true;
        if (name.IndexOf("Touch", StringComparison.OrdinalIgnoreCase) >= 0)   return true;
        if (prod.IndexOf("Touch", StringComparison.OrdinalIgnoreCase) >= 0)   return true;

        // 有些系统先创建 Pointer，依然视为触摸入口
        if (d.usages.Any(u => {
            var s = u.ToString();
            return s == "PrimaryPointer" || s == "Pointer";
        })) return true;

        return false;
    }

    static bool LooksLikeSensor(InputDevice d)
    {
        var tn = d.GetType().Name;
        var name = d.displayName ?? "";
        var prod = d.description.product ?? "";
        var inf  = d.description.interfaceName ?? "";
        return SensorKeywords.Any(k =>
            tn.IndexOf(k, StringComparison.OrdinalIgnoreCase) >= 0 ||
            name.IndexOf(k, StringComparison.OrdinalIgnoreCase) >= 0 ||
            prod.IndexOf(k, StringComparison.OrdinalIgnoreCase) >= 0 ||
            inf.IndexOf(k, StringComparison.OrdinalIgnoreCase) >= 0);
    }

    void RemoveIfNotAllowed(InputDevice d)
    {
        bool allowTouch = allowTouchscreen && LooksLikeTouchscreen(d);
        bool allowPad   = allowGamepad     && (d.layout == "Gamepad" ||
                          d.GetType().Name.IndexOf("Gamepad", StringComparison.OrdinalIgnoreCase) >= 0);

        bool allowed = allowTouch || allowPad;
        bool sensor  = LooksLikeSensor(d);

        if (!allowed || sensor)
        {
            Debug.LogWarning($"[NoSensor] Removing device: {d.GetType().Name} ({d.displayName})");
            InputSystem.RemoveDevice(d);
        }
    }
#endif

    void Update()
    {
#if ENABLE_INPUT_SYSTEM && !UNITY_EDITOR
        // 启动头3秒更激进地清理（厂商驱动迟到映射）
        if (_aggressiveCleanLeft > 0f)
        {
            _aggressiveCleanLeft -= Time.unscaledDeltaTime;
            foreach (var d in InputSystem.devices.ToArray())
                RemoveIfNotAllowed(d);
        }
#endif

        if (!showOverlay) return;
        _uiTimer += Time.unscaledDeltaTime;
        if (_uiTimer >= Mathf.Max(0.2f, overlayRefreshSec))
        {
            _uiTimer = 0f;
            RebuildOverlay();
        }
    }

    // —— 叠加监视 —— //
    void RebuildOverlay()
    {
        var list = new List<string>();
        _danger = false;

#if ENABLE_INPUT_SYSTEM
        var devs = InputSystem.devices
            .Select(d => new {
                Type = d.GetType().Name,
                Name = d.displayName ?? "",
                Layout = d.layout ?? ""
            })
            .OrderBy(x => x.Type)
            .ToArray();

        list.Add("[Devices]");
        if (devs.Length == 0) list.Add("<no devices>");
        foreach (var d in devs)
        {
            bool looksSensor = SensorKeywords.Any(k =>
                d.Type.IndexOf(k, StringComparison.OrdinalIgnoreCase) >= 0 ||
                d.Name.IndexOf(k, StringComparison.OrdinalIgnoreCase) >= 0);
            if (looksSensor) _danger = true;
            list.Add($"- {d.Type}  [{d.Name}]  <{d.Layout}>");
        }
#else
        list.Add("[Devices]（新输入系统未启用：仅显示旧API状态）");
#endif

        list.Add("");
        list.Add("[LegacySensors]");
        list.Add($"- gyro.enabled: {Input.gyro.enabled}");
        list.Add($"- compass.enabled: {Input.compass.enabled}");
        list.Add($"- location.status: {Input.location.status}");

        bool autoRot =
            Screen.autorotateToPortrait ||
            Screen.autorotateToPortraitUpsideDown ||
            Screen.autorotateToLandscapeLeft ||
            Screen.autorotateToLandscapeRight;
        list.Add($"- AutoRotation: {(autoRot ? "ON" : "OFF")} / Orientation: {Screen.orientation}");

        if (Input.location.status == LocationServiceStatus.Running ||
            Input.compass.enabled || Input.gyro.enabled)
            _danger = true;

        list.Add(_danger ? "[ALERT] 发现疑似传感器或定位运行！"
                         : "[OK] 未发现传感器设备或定位运行。");

        _lines = list.ToArray();
    }

    void OnGUI()
    {
        if (!showOverlay || _lines == null || _lines.Length == 0) return;

        // 紧凑样式
        var style = new GUIStyle(GUI.skin.label) {
            fontSize = overlayFontSize,
            wordWrap = false,
            richText = false
        };
        int lineH = overlayFontSize + 2;
        int pad   = 6;

        float boxW = Mathf.Clamp(Screen.width * overlayWidthRatio, 160f, Screen.width - 20f);
        float boxH = pad * 2 + _lines.Length * lineH;

        var oldColor = GUI.color;
        GUI.color = new Color(0f, 0f, 0f, overlayBgAlpha);
        GUI.Box(new Rect(overlayX, overlayY, boxW, boxH), GUIContent.none);

        GUI.color = _danger ? Color.red : oldColor;
        float x = overlayX + pad;
        float y = overlayY + pad;
        for (int i = 0; i < _lines.Length; i++, y += lineH)
            GUI.Label(new Rect(x, y, boxW - pad * 2, lineH), _lines[i], style);

        GUI.color = oldColor;
    }
}
