#if UNITY_EDITOR
using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

public class SensorUsageScannerWindow : EditorWindow
{
    [MenuItem("Tools/Sensor Usage Scanner")]
    public static void Open() => GetWindow<SensorUsageScannerWindow>("Sensor Scanner");

    [Serializable]
    public class Hit
    {
        public string path;
        public int line;
        public string pattern;
        public string snippet;
    }

    Vector2 _scroll;
    List<Hit> _hits = new List<Hit>();
    bool _includePackages = true;
    bool _includeProjectSettings = true;
    bool _includePlugins = true;
    bool _includeIosAndroidRoots = true;

    static readonly string[] SearchRootsDefault = new[]
    {
        "Assets",
        "Packages",
        "ProjectSettings",
        "Plugins",
        "Android",
        "iOS"
    };

    static readonly string[] Extensions = new[]
    {
        ".cs",".java",".kt",".xml",".gradle",".plist",".mm",".m",".swift",".json"
    };

    // 关键词集合（可改）
    static readonly (string label, string[] patterns)[] Buckets = new (string, string[])[]
    {
        ("Unity Old Input System",
            new[]{
                @"\bInput\.acceleration\b",
                @"\bInput\.gyro\b|\bGyroscope\b",
                @"\bInput\.compass\b|\bCompass\b",
                @"\bInput\.location\b|\bLocationService\b",
                @"\bWebCamTexture\b",
                @"\bMicrophone\.Start\b",
                @"\bHandheld\.Vibrate\b"
            }),
        ("Unity New Input System",
            new[]{
                @"\bUnityEngine\.InputSystem\b",
                @"\b(Accelerometer|Gyroscope|Compass|AttitudeSensor|GravitySensor|LinearAccelerationSensor|OrientationSensor|Magnetometer|Barometer|ProximitySensor|LightSensor|StepCounter|Pedometer)\b"
            }),
        ("Android Native / Java 调用",
            new[]{
                @"\bSensorManager\b|\bSENSOR_SERVICE\b|\bandroid\.hardware\.Sensor\b|\bgetDefaultSensor\b|\bregisterListener\b",
                @"\bTYPE_(ACCELEROMETER|GYROSCOPE|MAGNETIC_FIELD|GRAVITY|LINEAR_ACCELERATION|ROTATION_VECTOR|PROXIMITY|LIGHT|STEP_(COUNTER|DETECTOR))\b",
                @"\bgetSystemService\(\s*Context\.SENSOR_SERVICE",
                @"\bFusedLocationProvider\b|\brequestLocationUpdates\b",
                @"\bAndroidJava(Object|Class)\b.*SensorManager"
            }),
        ("Android 权限 (AndroidManifest*.xml)",
            new[]{
                @"android\.permission\.(ACCESS_(FINE|COARSE)_LOCATION|BODY_SENSORS|ACTIVITY_RECOGNITION|RECORD_AUDIO|CAMERA)"
            }),
        ("iOS 权限/框架 (Info.plist / 源码)",
            new[]{
                @"NS(Microphone|Camera|Location(When|Always)|Motion)UsageDescription",
                @"\bCoreMotion\b|\bCoreLocation\b|\bAVFoundation\b|\bCMMotionManager\b"
            }),
        ("AR/XR/地图类（可能间接用传感器）",
            new[]{
                @"\bARKit\b|\bARCore\b|\bUnity\.XR\b|\bVuforia\b|\bMapbox\b|\bOpenCV\b"
            })
    };

    List<(Regex rx,string label)> _compiled;

    void OnEnable()
    {
        _compiled = new List<(Regex,string)>();
        foreach (var (label, pats) in Buckets)
        {
            foreach (var p in pats)
            {
                var rx = new Regex(p, RegexOptions.Compiled | RegexOptions.IgnoreCase);
                _compiled.Add((rx,label));
            }
        }
    }

    void OnGUI()
    {
        EditorGUILayout.LabelField("传感器调用扫描器", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("点击 Scan 扫描项目中可能调用传感器的所有可疑位置。双击结果或点 Open 可直接打开到对应行。", MessageType.Info);

        using (new EditorGUILayout.VerticalScope("box"))
        {
            _includePackages = EditorGUILayout.ToggleLeft("包含 Packages/", _includePackages);
            _includeProjectSettings = EditorGUILayout.ToggleLeft("包含 ProjectSettings/", _includeProjectSettings);
            _includePlugins = EditorGUILayout.ToggleLeft("包含 Plugins/", _includePlugins);
            _includeIosAndroidRoots = EditorGUILayout.ToggleLeft("包含 iOS/ 和 Android/ 根目录", _includeIosAndroidRoots);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Scan", GUILayout.Height(26)))
                {
                    Scan();
                }
                if (GUILayout.Button("Clear", GUILayout.Height(26)))
                {
                    _hits.Clear();
                }
                if (GUILayout.Button("Export CSV", GUILayout.Height(26)))
                {
                    ExportCsv();
                }
            }
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField($"结果：{_hits.Count} 项", EditorStyles.boldLabel);

        _scroll = EditorGUILayout.BeginScrollView(_scroll);
        for (int i = 0; i < _hits.Count; i++)
        {
            var h = _hits[i];
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Open", GUILayout.Width(60)))
                    OpenAtLine(h.path, h.line);

                GUILayout.Label($"{i+1,3}. {h.path}:{h.line}", GUILayout.MinWidth(400));
                GUILayout.FlexibleSpace();
                GUILayout.Label(h.pattern, GUILayout.Width(260));
            }
            EditorGUILayout.LabelField(h.snippet, EditorStyles.miniLabel);
            EditorGUILayout.Space(6);
        }
        EditorGUILayout.EndScrollView();
    }

    void Scan()
    {
        _hits.Clear();
        var roots = new List<string> { "Assets" };
        if (_includePackages) roots.Add("Packages");
        if (_includeProjectSettings) roots.Add("ProjectSettings");
        if (_includePlugins) roots.Add("Plugins");
        if (_includeIosAndroidRoots) { roots.Add("Android"); roots.Add("iOS"); }

        var toScan = new List<string>();
        foreach (var r in roots)
        {
            var full = Path.GetFullPath(r);
            if (Directory.Exists(full)) toScan.Add(full);
        }

        int fileCount = 0, hitCount = 0;
        try
        {
            foreach (var root in toScan)
            {
                var files = Directory.GetFiles(root, "*.*", SearchOption.AllDirectories);
                foreach (var f in files)
                {
                    var ext = Path.GetExtension(f);
                    if (Array.IndexOf(Extensions, ext) < 0) continue;

                    fileCount++;
                    if (fileCount % 200 == 0)
                        EditorUtility.DisplayProgressBar("Scanning...", f, (hitCount % 1000) / 1000f);

                    ScanFile(f, ref hitCount);
                }
            }
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }

        Debug.Log($"[SensorScanner] 扫描完成：文件 {fileCount} 个，命中 {_hits.Count} 项。");
        Repaint();
    }

    void ScanFile(string path, ref int hitCount)
    {
        string[] lines;
        try
        {
            lines = File.ReadAllLines(path);
        }
        catch { return; }

        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i];
            foreach (var (rx,label) in _compiled)
            {
                var m = rx.Match(line);
                if (m.Success)
                {
                    var snippet = line.Trim();
                    if (snippet.Length > 240) snippet = snippet.Substring(0, 240) + " …";
                    _hits.Add(new Hit
                    {
                        path = ToProjectRelative(path),
                        line = i + 1,
                        pattern = label + " : " + rx.ToString(),
                        snippet = snippet
                    });
                    hitCount++;
                    break; // 本行已命中，避免重复加入
                }
            }
        }
    }

    static string ToProjectRelative(string fullPath)
    {
        var proj = Path.GetFullPath(Application.dataPath + "/..")
                       .Replace('\\','/');
        fullPath = fullPath.Replace('\\','/');
        if (fullPath.StartsWith(proj))
            return fullPath.Substring(proj.Length+1);
        return fullPath;
    }

    void OpenAtLine(string relPath, int line)
    {
        var full = Path.GetFullPath(relPath);
        // 首选内部打开到行号
        if (InternalEditorUtility.OpenFileAtLineExternal(full, line))
            return;
        // 兜底用 AssetDatabase（可能不支持非 Assets 下）
        var asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(relPath);
        if (asset != null) AssetDatabase.OpenAsset(asset, line);
    }

    void ExportCsv()
    {
        var outPath = "Assets/SensorScanReport.csv";
        var sb = new StringBuilder();
        sb.AppendLine("Path,Line,Pattern,Snippet");
        foreach (var h in _hits)
        {
            // 简单转义
            string esc(string s) => "\"" + s.Replace("\"","\"\"") + "\"";
            sb.AppendLine($"{esc(h.path)},{h.line},{esc(h.pattern)},{esc(h.snippet)}");
        }
        File.WriteAllText(outPath, sb.ToString(), new UTF8Encoding(false));
        AssetDatabase.Refresh();
        EditorUtility.RevealInFinder(outPath);
        Debug.Log($"[SensorScanner] CSV 导出到 {outPath}");
    }
}
#endif
