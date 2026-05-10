using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

/// <summary>
/// GameActionData 自定义 Inspector 编辑器
/// 解决问题：① 字符串易写错 ② 无法反向引用 ③ 参数填写复杂
/// </summary>
[CustomEditor(typeof(GameActionData))]
public class GameActionDataEditor : Editor
{
    // ─── 状态 ──────────────────────────────────────────────────────
    private GameActionData _data;
    private SerializedProperty _spParameters;

    // 缓存的 Action 类型列表（只扫描一次）
    private static Type[] _cachedActionTypes;
    private static string[] _cachedActionTypeNames;
    private static bool _cacheBuilt;

    // 当前选中类型
    private int _selectedIndex = -1;

    // 当前选中类型的参数字段信息（缓存）
    private struct ParamField
    {
        public string displayName;
        public Type   fieldType;
    }
    private List<ParamField> _currentParamFields = new List<ParamField>();

    // UI 状态
    private bool _showRawData;
    private string _searchFilter = "";
    private Vector2 _refScrollPos;

    // 排除的基础设施字段名
    private static readonly HashSet<string> ExcludedNames = new HashSet<string>
    {
        "setValue", "setResult", "endAction"
    };

    // ─── Unity 生命周期 ────────────────────────────────────────────
    private void OnEnable()
    {
        _data = target as GameActionData;
        _spParameters = serializedObject.FindProperty("_parameters");

        BuildTypeCache();
        SyncSelectedIndex();
        RefreshParamFields();
    }

    // ─── Inspector GUI ────────────────────────────────────────────
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        DrawTitleBar();
        DrawIdField();
        DrawTypeDropdown();
        DrawParamFields();
        DrawActions();
        DrawRawDataFoldout();
        DrawReferenceSection();

        serializedObject.ApplyModifiedProperties();
    }

    // ─── 各区域绘制 ──────────────────────────────────────────────

    private void DrawTitleBar()
    {
        EditorGUILayout.Space(4);
        var rect = EditorGUILayout.GetControlRect(false, 24);
        EditorGUI.DrawRect(rect, new Color(0.18f, 0.18f, 0.25f));
        EditorGUI.LabelField(rect, $"  GameAction  {_data.name}", EditorStyles.boldLabel);
        EditorGUILayout.Space(4);
    }

    private void DrawIdField()
    {
        var spId = serializedObject.FindProperty("id");
        EditorGUILayout.PropertyField(spId, new GUIContent("ID"));

        // 显示 asset 路径
        var path = AssetDatabase.GetAssetPath(_data);
        EditorGUILayout.LabelField("路径", EditorStyles.miniLabel);
        EditorGUILayout.SelectableLabel(path, EditorStyles.miniLabel, GUILayout.Height(16));
    }

    private void DrawTypeDropdown()
    {
        EditorGUILayout.Space(6);
        EditorGUILayout.LabelField("Action 类型", EditorStyles.boldLabel);

        using (new EditorGUILayout.HorizontalScope())
        {
            // 搜索过滤
            EditorGUILayout.LabelField("过滤", GUILayout.Width(40));
            _searchFilter = EditorGUILayout.TextField(_searchFilter);

            // 刷新按钮
            if (GUILayout.Button("↻", GUILayout.Width(24)))
            {
                _cacheBuilt = false;
                BuildTypeCache();
                SyncSelectedIndex();
            }
        }

        // 构建当前显示的列表（根据过滤）
        var displayNames = _cachedActionTypeNames;
        var displayTypes  = _cachedActionTypes;
        string curName = _data.typeName;

        if (!string.IsNullOrEmpty(_searchFilter))
        {
            var filtered = new List<(string name, Type type)>();
            for (int i = 0; i < _cachedActionTypes.Length; i++)
            {
                if (_cachedActionTypeNames[i].IndexOf(_searchFilter, StringComparison.OrdinalIgnoreCase) >= 0)
                    filtered.Add((_cachedActionTypeNames[i], _cachedActionTypes[i]));
            }
            displayNames = filtered.Select(x => x.name).ToArray();
            displayTypes  = filtered.Select(x => x.type).ToArray();
        }

        // 找到当前选中在过滤列表中的索引
        int curIdx = -1;
        for (int i = 0; i < displayTypes.Length; i++)
        {
            if (displayTypes[i].Name == curName) { curIdx = i; break; }
        }

        EditorGUI.BeginChangeCheck();
        int newIdx = EditorGUILayout.Popup("类型", curIdx, displayNames);
        if (EditorGUI.EndChangeCheck() && newIdx >= 0)
        {
            var newType = displayTypes[newIdx];
            _data.typeName = newType.Name;
            _selectedIndex = Array.IndexOf(_cachedActionTypes, newType);

            // 重置参数列表（保留已填值 → 按新类型重映射）
            RebuildParametersForType(newType);
            RefreshParamFields();
            EditorUtility.SetDirty(_data);
        }

        // 显示当前类型的完整名称和来源
        if (_selectedIndex >= 0 && _selectedIndex < _cachedActionTypes.Length)
        {
            var t = _cachedActionTypes[_selectedIndex];
            EditorGUILayout.LabelField("完整类名", t.FullName, EditorStyles.miniLabel);
            EditorGUILayout.LabelField("所在文件", t.DeclaringType == null
                ? t.Assembly.GetName().Name
                : $"{t.DeclaringType.Name} ({t.Assembly.GetName().Name})", EditorStyles.miniLabel);
        }
    }

    private void DrawParamFields()
    {
        if (_currentParamFields.Count == 0)
        {
            if (_selectedIndex >= 0)
                DrawContainerTypeParams();
            return;
        }

        EditorGUILayout.Space(6);
        EditorGUILayout.LabelField("参数列表", EditorStyles.boldLabel);

        // 确保 _parameters 数量足够
        while (_data._parameters == null)
            _data._parameters = new List<Parameter>();
        while (_data._parameters.Count < _currentParamFields.Count)
            _data._parameters.Add(new Parameter { value = "" });

        for (int i = 0; i < _currentParamFields.Count; i++)
        {
            var pf = _currentParamFields[i];
            var param = _data._parameters[i];

            string label = ObjectNames.NicifyVariableName(pf.displayName);

            // 如果此参数有嵌套子参数，可能是 WaitAction 等模式
            if (param.parameters != null && param.parameters.Count > 0)
            {
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.LabelField(label, EditorStyles.boldLabel);
                EditorGUI.indentLevel++;
                string newVal = DrawTypedField("值", pf.fieldType, param.value);
                if (newVal != param.value)
                {
                    param.value = newVal;
                    EditorUtility.SetDirty(_data);
                }
                EditorGUILayout.Space(2);
                DrawNestedActionList(param.parameters, "子动作");
                EditorGUI.indentLevel--;
                EditorGUILayout.EndVertical();
            }
            else
            {
                string newVal = DrawTypedField(label, pf.fieldType, param.value);
                if (newVal != param.value)
                {
                    param.value = newVal;
                    EditorUtility.SetDirty(_data);
                }
            }
        }
    }

    /// <summary>处理容器类型（ActionList / WaitAction 等无自身字段的 struct）</summary>
    /// <summary>处理容器类型（ActionList / WaitAction 等无自身字段的 struct）</summary>
    private void DrawContainerTypeParams()
    {
        _data._parameters ??= new List<Parameter>();

        // ── WaitAction 特殊处理：延迟值 + 子动作列表 ──
        if (_data.typeName == "WaitAction")
        {
            DrawWaitActionParams(_data._parameters, isTopLevel: true);
            return;
        }

        // ── ActionList 及其他：每项即子动作 ──
        EditorGUILayout.Space(6);
        using (new EditorGUILayout.HorizontalScope())
        {
            EditorGUILayout.LabelField("嵌套动作列表", EditorStyles.boldLabel);
            if (GUILayout.Button("+ 添加", GUILayout.Width(60)))
            {
                _data._parameters.Add(new Parameter { value = "", parameters = new List<Parameter>() });
                EditorUtility.SetDirty(_data);
            }
        }

        if (_data._parameters.Count == 0)
        {
            EditorGUILayout.HelpBox("此 Action 为容器类型（如 ActionList）。点击「+ 添加」添加子动作", MessageType.Info);
            return;
        }

        DrawNestedActionList(_data._parameters, null);
    }

    /// <summary>递归绘制嵌套子动作列表</summary>
    private void DrawNestedActionList(List<Parameter> paramList, string title)
    {
        if (title != null)
            EditorGUILayout.LabelField(title, EditorStyles.miniBoldLabel);

        for (int i = 0; i < paramList.Count; i++)
        {
            var param = paramList[i];
            DrawSingleNestedAction(i, param, paramList);
        }

        if (GUILayout.Button("+ 添加子动作", GUILayout.Height(22)))
        {
            paramList.Add(new Parameter { value = "", parameters = new List<Parameter>() });
            EditorUtility.SetDirty(_data);
        }
    }

    /// <summary>绘制单个嵌套动作（递归支持多级嵌套）</summary>
    private void DrawSingleNestedAction(int index, Parameter param, List<Parameter> parentList)
    {
        Color bg = index % 2 == 0 ? new Color(0.22f, 0.22f, 0.28f) : new Color(0.18f, 0.18f, 0.24f);
        EditorGUILayout.BeginVertical("box");
        var rect = GUILayoutUtility.GetRect(GUIContent.none, EditorStyles.miniLabel, GUILayout.ExpandWidth(true));
        EditorGUI.DrawRect(rect, bg);
        EditorGUI.LabelField(rect, $"  ┌ 子动作 {index + 1}");

        EditorGUILayout.Space(2);
        EditorGUI.indentLevel++;

        // ── 子动作类型选择 ──
        int curSubIdx = -1;
        for (int t = 0; t < _cachedActionTypes.Length; t++)
        {
            if (_cachedActionTypes[t].Name == param.value) { curSubIdx = t; break; }
        }

        EditorGUI.BeginChangeCheck();
        int newSubIdx = EditorGUILayout.Popup("类型", curSubIdx, _cachedActionTypeNames);
        if (EditorGUI.EndChangeCheck() && newSubIdx >= 0)
        {
            param.value = _cachedActionTypes[newSubIdx].Name;
            EditorUtility.SetDirty(_data);
        }

        // 显示完整类名
        if (curSubIdx >= 0)
        {
            EditorGUILayout.LabelField(_cachedActionTypes[curSubIdx].FullName, EditorStyles.miniLabel);
        }

        // ── 子动作的参数字段（按类型分发） ──
        if (curSubIdx >= 0)
        {
            var subType = _cachedActionTypes[curSubIdx];

            // WaitAction 特殊处理：第一项是延迟值，其子参数才是动作列表
            if (subType.Name == "WaitAction")
            {
                DrawWaitActionParams(param.parameters, isTopLevel: false);
            }
            else
            {
                var subFields = GetParamFieldsForType(subType);

                param.parameters ??= new List<Parameter>();
                while (param.parameters.Count < subFields.Count)
                    param.parameters.Add(new Parameter { value = "" });

                if (subFields.Count > 0)
                {
                    EditorGUILayout.LabelField("参数:", EditorStyles.miniBoldLabel);
                    for (int f = 0; f < subFields.Count; f++)
                    {
                        var sf = subFields[f];
                        var subParam = param.parameters[f];

                        string label = $"  {ObjectNames.NicifyVariableName(sf.displayName)}";
                        string newVal = DrawTypedField(label, sf.fieldType, subParam.value);
                        if (newVal != subParam.value)
                        {
                            subParam.value = newVal;
                            EditorUtility.SetDirty(_data);
                        }
                    }
                }
                else if (subFields.Count == 0)
                {
                    // 子类型也是容器类型（嵌套 ActionList 等）
                    EditorGUILayout.Space(2);
                    DrawNestedActionList(param.parameters, "子级动作:");
                }
            }
        }

        EditorGUI.indentLevel--;
        EditorGUILayout.Space(2);

        // ── 操作按钮：移除 / 上移 / 下移 ──
        using (new EditorGUILayout.HorizontalScope())
        {
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("🗑 移除", GUILayout.Width(60), GUILayout.Height(20)))
            {
                parentList.RemoveAt(index);
                EditorUtility.SetDirty(_data);
            }
            if (GUILayout.Button("▲", GUILayout.Width(24), GUILayout.Height(20)) && index > 0)
            {
                (parentList[index], parentList[index - 1]) = (parentList[index - 1], parentList[index]);
                EditorUtility.SetDirty(_data);
            }
            if (GUILayout.Button("▼", GUILayout.Width(24), GUILayout.Height(20)) && index < parentList.Count - 1)
            {
                (parentList[index], parentList[index + 1]) = (parentList[index + 1], parentList[index]);
                EditorUtility.SetDirty(_data);
            }
        }

        EditorGUILayout.EndVertical();
    }

    /// <summary>获取指定类型的参数字段（不依赖 _selectedIndex）</summary>
    private List<ParamField> GetParamFieldsForType(Type type)
    {
        var result = new List<ParamField>();
        var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
        foreach (var f in fields)
        {
            if (ExcludedNames.Contains(f.Name)) continue;
            if (f.FieldType == typeof(Action) || typeof(Delegate).IsAssignableFrom(f.FieldType)) continue;

            result.Add(new ParamField
            {
                displayName = f.Name,
                fieldType = f.FieldType
            });
        }
        return result;
    }

    /// <summary>WaitAction 专属渲染：延迟值 + 子动作列表</summary>
    private void DrawWaitActionParams(List<Parameter> parameters, bool isTopLevel)
    {
        parameters ??= new List<Parameter>();
        while (parameters.Count < 1)
            parameters.Add(new Parameter { value = "0", parameters = new List<Parameter>() });

        var delayParam = parameters[0];

        EditorGUILayout.Space(4);
        using (new EditorGUILayout.VerticalScope("box"))
        {
            EditorGUILayout.LabelField("Waiting 配置", EditorStyles.boldLabel);

            // 延迟值
            int delay = 0;
            int.TryParse(delayParam.value, out delay);
            EditorGUI.BeginChangeCheck();
            delay = EditorGUILayout.IntField("延迟 (帧)", delay);
            if (EditorGUI.EndChangeCheck())
            {
                delayParam.value = delay.ToString();
                EditorUtility.SetDirty(_data);
            }

            // 子动作列表
            delayParam.parameters ??= new List<Parameter>();
            EditorGUILayout.Space(4);
            DrawNestedActionList(delayParam.parameters, "延迟后执行的子动作:");
        }
    }

    private void DrawActions()
    {
        EditorGUILayout.Space(10);

        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("▶ 执行测试 (编辑模式)", GUILayout.Height(28)))
            {
                ExecuteActionInEditor();
            }

            if (GUILayout.Button("📋 复制为代码片段", GUILayout.Height(28)))
            {
                CopyAsCodeSnippet();
            }
        }
    }

    private void DrawRawDataFoldout()
    {
        EditorGUILayout.Space(4);
        _showRawData = EditorGUILayout.Foldout(_showRawData, "原始数据 (Debug)", true);
        if (_showRawData)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(serializedObject.FindProperty("typeName"),
                new GUIContent("typeName (字符串)"));
            EditorGUILayout.PropertyField(_spParameters, new GUIContent("_parameters (原始)"), true);
            EditorGUI.indentLevel--;
        }
    }

    private void DrawReferenceSection()
    {
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("引用查找", EditorStyles.boldLabel);

        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("🔍 查找谁引用了此 ActionData 数据文件"))
                FindWhoReferencesThisAsset();

            if (GUILayout.Button("🔍 查找哪些 Data 使用了此 Action 类型"))
                FindByActionType(_data.typeName);
        }

        // 如果当前 asset 被引用，显示引用信息
        var refs = AssetDatabase.GetDependencies(AssetDatabase.GetAssetPath(_data), false);
        if (refs.Length > 0)
        {
            EditorGUILayout.LabelField("此 Asset 依赖的资源:", EditorStyles.miniLabel);
            foreach (var r in refs)
            {
                if (!r.EndsWith(".asset")) continue;
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.ObjectField(ObjectNames.NicifyVariableName(
                    System.IO.Path.GetFileNameWithoutExtension(r)),
                    AssetDatabase.LoadAssetAtPath<Object>(r), typeof(Object), false);
                EditorGUILayout.EndHorizontal();
            }
        }
    }

    // ─── 字段绘制 ──────────────────────────────────────────────

    /// <summary>根据类型绘制对应的编辑器字段，返回字符串值</summary>
    private static string DrawTypedField(string label, Type type, string currentValue)
    {
        if (type == typeof(int))
        {
            int v = 0; int.TryParse(currentValue, out v);
            v = EditorGUILayout.IntField(label, v);
            return v.ToString();
        }
        if (type == typeof(float))
        {
            float v = 0f; float.TryParse(currentValue, out v);
            v = EditorGUILayout.FloatField(label, v);
            return v.ToString();
        }
        if (type == typeof(bool))
        {
            bool v = false; bool.TryParse(currentValue, out v);
            v = EditorGUILayout.Toggle(label, v);
            return v.ToString();
        }
        if (type == typeof(string))
        {
            return EditorGUILayout.TextField(label, currentValue ?? "");
        }
        if (type == typeof(Vector2))
        {
            Vector2 v = Vector2.zero;
            TryParseVector2(currentValue, out v);
            v = EditorGUILayout.Vector2Field(label, v);
            return $"{v.x},{v.y}";
        }
        if (type == typeof(Vector3))
        {
            Vector3 v = Vector3.zero;
            TryParseVector3(currentValue, out v);
            v = EditorGUILayout.Vector3Field(label, v);
            return $"{v.x},{v.y},{v.z}";
        }
        if (type == typeof(Vector2Int))
        {
            Vector2Int v = Vector2Int.zero;
            TryParseVector2Int(currentValue, out v);
            v = EditorGUILayout.Vector2IntField(label, v);
            return $"{v.x},{v.y}";
        }
        if (type == typeof(Vector3Int))
        {
            Vector3Int v = Vector3Int.zero;
            TryParseVector3Int(currentValue, out v);
            v = EditorGUILayout.Vector3IntField(label, v);
            return $"{v.x},{v.y},{v.z}";
        }

        // 未知类型 → 文本编辑
        return EditorGUILayout.TextField(label, currentValue ?? "");
    }

    private static bool TryParseVector2(string s, out Vector2 v)
    {
        v = Vector2.zero;
        if (string.IsNullOrEmpty(s)) return false;
        var parts = s.Split(',');
        if (parts.Length < 2) return false;
        float x, y;
        float.TryParse(parts[0], out x);
        float.TryParse(parts[1], out y);
        v = new Vector2(x, y);
        return true;
    }

    private static bool TryParseVector3(string s, out Vector3 v)
    {
        v = Vector3.zero;
        if (string.IsNullOrEmpty(s)) return false;
        var parts = s.Split(',');
        if (parts.Length < 3) return false;
        float x, y, z;
        float.TryParse(parts[0], out x);
        float.TryParse(parts[1], out y);
        float.TryParse(parts[2], out z);
        v = new Vector3(x, y, z);
        return true;
    }

    private static bool TryParseVector2Int(string s, out Vector2Int v)
    {
        v = Vector2Int.zero;
        if (string.IsNullOrEmpty(s)) return false;
        var parts = s.Split(',');
        if (parts.Length < 2) return false;
        int x, y;
        int.TryParse(parts[0], out x);
        int.TryParse(parts[1], out y);
        v = new Vector2Int(x, y);
        return true;
    }

    private static bool TryParseVector3Int(string s, out Vector3Int v)
    {
        v = Vector3Int.zero;
        if (string.IsNullOrEmpty(s)) return false;
        var parts = s.Split(',');
        if (parts.Length < 3) return false;
        int x, y, z;
        int.TryParse(parts[0], out x);
        int.TryParse(parts[1], out y);
        int.TryParse(parts[2], out z);
        v = new Vector3Int(x, y, z);
        return true;
    }

    // ─── 逻辑 ──────────────────────────────────────────────────

    private void BuildTypeCache()
    {
        if (_cacheBuilt && _cachedActionTypes != null) return;

        var types = new List<Type>();
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        foreach (var asm in assemblies)
        {
            if (asm.GetName().Name.StartsWith("Unity") ||
                asm.GetName().Name.StartsWith("System") ||
                asm.GetName().Name.StartsWith("mscorlib") ||
                asm.GetName().Name.StartsWith("Mono") ||
                asm.GetName().Name.StartsWith("JetBrains"))
                continue;

            try
            {
                var interfaceType = typeof(GameAction);
                foreach (var t in asm.GetTypes())
                {
                    if (t.IsValueType && !t.IsAbstract && interfaceType.IsAssignableFrom(t))
                        types.Add(t);
                }
            }
            catch (ReflectionTypeLoadException)
            {
                // 跳过无法加载的程序集
            }
        }

        // 去重并按名字排序
        types = types.Distinct().OrderBy(t => t.Name).ToList();
        _cachedActionTypes = types.ToArray();
        _cachedActionTypeNames = types.Select(t =>
        {
            var source = t.DeclaringType != null ? t.DeclaringType.Name : t.Namespace;
            var category = source?.Replace("Action", "") ?? "";
            return $"{t.Name} ({category})";
        }).ToArray();

        _cacheBuilt = true;
        Debug.Log($"[GameActionDataEditor] 扫描到 {types.Count} 个 GameAction 类型");
    }

    private void SyncSelectedIndex()
    {
        _selectedIndex = -1;
        if (string.IsNullOrEmpty(_data.typeName)) return;

        for (int i = 0; i < _cachedActionTypes.Length; i++)
        {
            if (_cachedActionTypes[i].Name == _data.typeName)
            {
                _selectedIndex = i;
                return;
            }
        }

        // 未找到 → 标记为未知类型
        Debug.LogWarning($"[GameActionDataEditor] typeName '{_data.typeName}' 未找到对应的 struct，可能已被删除或重命名");
    }

    private void RefreshParamFields()
    {
        _currentParamFields.Clear();
        if (_selectedIndex < 0 || _selectedIndex >= _cachedActionTypes.Length) return;

        var type = _cachedActionTypes[_selectedIndex];
        // 获取 struct 的公共字段，排除基础设施字段和属性（属性不会被 GetFields 返回，安全）
        var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
        foreach (var f in fields)
        {
            if (ExcludedNames.Contains(f.Name)) continue;
            if (f.FieldType == typeof(Delegate) || typeof(Delegate).IsAssignableFrom(f.FieldType))
                continue;
            if (f.FieldType == typeof(Action))
                continue;

            _currentParamFields.Add(new ParamField
            {
                displayName = f.Name,
                fieldType = f.FieldType
            });
        }
    }

    private void RebuildParametersForType(Type newType)
    {
        if (_data._parameters == null)
            _data._parameters = new List<Parameter>();

        // 获取新类型的参数字段
        var fields = newType.GetFields(BindingFlags.Public | BindingFlags.Instance)
            .Where(f => !ExcludedNames.Contains(f.Name)
                        && f.FieldType != typeof(Action)
                        && !typeof(Delegate).IsAssignableFrom(f.FieldType))
            .ToList();

        // 调整 _parameters 列表长度
        while (_data._parameters.Count < fields.Count)
            _data._parameters.Add(new Parameter { value = "" });
        if (_data._parameters.Count > fields.Count)
            _data._parameters.RemoveRange(fields.Count, _data._parameters.Count - fields.Count);
    }

    /// <summary>在编辑模式下执行此 Action（仅调试用，会触发 AddListener 注册的事件）</summary>
    private void ExecuteActionInEditor()
    {
        if (string.IsNullOrEmpty(_data.typeName))
        {
            Debug.LogWarning("请先选择 Action 类型");
            return;
        }

        _data.Action();
        Debug.Log($"[GameActionDataEditor] 已执行: {_data.name} ({_data.typeName})");
    }

    /// <summary>复制为代码片段方便在脚本中调用</summary>
    private void CopyAsCodeSnippet()
    {
        var type = _selectedIndex >= 0 ? _cachedActionTypes[_selectedIndex] : null;
        if (type == null)
        {
            Debug.LogWarning("请先选择 Action 类型");
            return;
        }

        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"// GameAction: {_data.name} (ID: {_data.id})");
        sb.Append($"new {type.Name}\n{{");
        if (_currentParamFields.Count > 0)
        {
            sb.AppendLine();
            for (int i = 0; i < _currentParamFields.Count && i < _data._parameters.Count; i++)
            {
                var pf = _currentParamFields[i];
                var val = _data._parameters[i].value;
                sb.AppendLine($"    {pf.displayName} = {FormatLiteral(val, pf.fieldType)},");
            }
        }
        sb.AppendLine("}};");

        var snippet = sb.ToString();
        EditorGUIUtility.systemCopyBuffer = snippet;
        Debug.Log($"已复制代码片段到剪贴板:\n{snippet}");
    }

    private static string FormatLiteral(string value, Type type)
    {
        if (type == typeof(string)) return $"\"{value}\"";
        if (type == typeof(bool))   return value.ToLower();
        if (type == typeof(float))
        {
            float.TryParse(value, out var f);
            return f.ToString("0.0####") + "f";
        }
        if (type == typeof(Vector2) || type == typeof(Vector3) ||
            type == typeof(Vector2Int) || type == typeof(Vector3Int))
            return $"new {type.Name}(\"{value}\")";

        return value; // int 和其他
    }

    // ─── 引用查找 ──────────────────────────────────────────────

    /// <summary>查找所有引用了此 GameActionData asset 的其他数据</summary>
    private void FindWhoReferencesThisAsset()
    {
        var path = AssetDatabase.GetAssetPath(_data);
        var guid = AssetDatabase.AssetPathToGUID(path);

        var results = new List<string>();
        var allAssetPaths = AssetDatabase.FindAssets("t:ScriptableObject")
            .Select(AssetDatabase.GUIDToAssetPath)
            .Where(p => p.EndsWith(".asset") && p != path);

        foreach (var ap in allAssetPaths)
        {
            var text = System.IO.File.ReadAllText(ap);
            if (text.Contains(guid))
                results.Add(ap);
        }

        if (results.Count == 0)
        {
            Debug.Log($"[引用查找] 没有其他数据引用 \"{_data.name}\"");
        }
        else
        {
            Debug.Log($"[引用查找] 找到 {results.Count} 个引用 \"{_data.name}\":\n" +
                      string.Join("\n", results.Select(r => "  • " + r)));
            // 在 Project 窗口高亮
            Selection.objects = results.Select(AssetDatabase.LoadAssetAtPath<Object>).ToArray();
        }
    }

    /// <summary>按 Action 类型名查找所有 GameActionData</summary>
    private void FindByActionType(string typeName)
    {
        if (string.IsNullOrEmpty(typeName))
        {
            Debug.LogWarning("请先选择 Action 类型");
            return;
        }

        var guids = AssetDatabase.FindAssets("t:GameActionData");
        var matches = new List<string>();

        foreach (var guid in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var asset = AssetDatabase.LoadAssetAtPath<GameActionData>(path);
            if (asset != null && asset.typeName == typeName)
                matches.Add(path);
        }

        if (matches.Count == 0)
        {
            Debug.Log($"[类型引用] 没有 GameActionData 使用 \"{typeName}\" 类型");
        }
        else
        {
            Debug.Log($"[类型引用] 找到 {matches.Count} 个使用 \"{typeName}\" 的 GameActionData:\n" +
                      string.Join("\n", matches.Select(r => $"  • {r}")));
            Selection.objects = matches.Select(AssetDatabase.LoadAssetAtPath<Object>).ToArray();
        }
    }
}
