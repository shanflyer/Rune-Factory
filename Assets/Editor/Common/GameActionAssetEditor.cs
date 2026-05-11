using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GameActionAsset))]
public class GameActionAssetEditor : Editor
{
    private GameActionAsset _asset;
    private static Type[] _nodeTypes;
    private static string[] _nodeTypeNames;
    private static bool _cacheBuilt;
    private string _filter = "";

    private void OnEnable() { _asset = target as GameActionAsset; BuildCache(); }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.PropertyField(serializedObject.FindProperty("id"));
        EditorGUILayout.Space(6);
        EditorGUILayout.LabelField("Action Type Tree", EditorStyles.boldLabel);
        _filter = EditorGUILayout.TextField("Filter Types", _filter);
        DrawNode(_asset.root, n => { _asset.root = n; EditorUtility.SetDirty(_asset); }, "Root", 0);
        serializedObject.ApplyModifiedProperties();
    }

    void DrawNode(ActionNode node, Action<ActionNode> set, string label, int depth)
{
    EditorGUI.indentLevel = depth;

    EditorGUILayout.BeginVertical("box");

    using (new EditorGUILayout.HorizontalScope())
    {
        string currentName = "Select Type";

        if (node != null)
        {
            int currentIndex = Array.IndexOf(_nodeTypes, node.GetType());
            currentName = currentIndex >= 0
                ? _nodeTypeNames[currentIndex]
                : node.GetType().Name;
        }

        EditorGUILayout.PrefixLabel(label);

        if (GUILayout.Button(currentName, EditorStyles.popup))
        {
            var menu = new GenericMenu();
            bool hasAny = false;

            for (int i = 0; i < _nodeTypes.Length; i++)
            {
                string typeName = _nodeTypeNames[i];

                // 筛选只影响“打开下拉菜单后的候选项”
                if (!string.IsNullOrEmpty(_filter) &&
                    typeName.IndexOf(_filter, StringComparison.OrdinalIgnoreCase) < 0)
                {
                    continue;
                }

                hasAny = true;

                Type type = _nodeTypes[i];
                bool selected = node != null && node.GetType() == type;

                menu.AddItem(
                    new GUIContent(typeName),
                    selected,
                    () =>
                    {
                        var newNode = (ActionNode)Activator.CreateInstance(type);
                        set(newNode);
                        EditorUtility.SetDirty(_asset);
                    });
            }

            if (!hasAny)
            {
                menu.AddDisabledItem(new GUIContent("No matched node type"));
            }

            menu.ShowAsContext();
        }

        if (node != null && GUILayout.Button("x", GUILayout.Width(22)))
        {
            set(null);
            EditorUtility.SetDirty(_asset);
        }
    }

    if (node == null)
    {
        EditorGUILayout.HelpBox("Select a type", MessageType.Info);
        EditorGUILayout.EndVertical();
        return;
    }

    var fs = node.GetType()
        .GetFields(BindingFlags.Public | BindingFlags.Instance)
        .Where(f =>
            f.DeclaringType != typeof(ActionNode) &&
            f.DeclaringType != typeof(ContainerNode))
        .ToList();

    if (fs.Count > 0)
    {
        EditorGUI.indentLevel = depth + 1;

        foreach (var f in fs)
        {
            var v = f.GetValue(node);
            var nv = DrawField(ObjectNames.NicifyVariableName(f.Name), f.FieldType, v);

            if (!Equals(v, nv))
            {
                f.SetValue(node, nv);
                EditorUtility.SetDirty(_asset);
            }
        }

        EditorGUI.indentLevel = depth;
    }

    if (node is ContainerNode c)
    {
        EditorGUILayout.Space(4);
        EditorGUILayout.LabelField("Children:", EditorStyles.miniBoldLabel);

        EditorGUI.indentLevel = depth + 1;

        if (c.children == null)
        {
            c.children = new List<ActionEntry>();
            EditorUtility.SetDirty(_asset);
        }

        for (int i = 0; i < c.children.Count; i++)
        {
            if (c.children[i] == null)
            {
                c.children[i] = new ActionEntry();
                EditorUtility.SetDirty(_asset);
            }

            int idx = i;

            DrawNode(
                c.children[i].node,
                n =>
                {
                    c.children[idx].node = n;
                    EditorUtility.SetDirty(_asset);
                },
                "#" + (i + 1),
                depth + 1);
        }

        EditorGUI.indentLevel = depth;

        if (GUILayout.Button("+ Add Child"))
        {
            c.children.Add(new ActionEntry());
            EditorUtility.SetDirty(_asset);
        }
    }

    EditorGUILayout.EndVertical();
}

    static object DrawField(string l, Type t, object v) { if (t == typeof(int)) return EditorGUILayout.IntField(l, v==null?0:(int)v); if (t == typeof(float)) return EditorGUILayout.FloatField(l, v==null?0f:(float)v); if (t == typeof(bool)) return EditorGUILayout.Toggle(l, v!=null&&(bool)v); if (t == typeof(string)) return EditorGUILayout.TextField(l, v as string??""); if (t == typeof(Vector2)) return EditorGUILayout.Vector2Field(l, v is Vector2 x?x:Vector2.zero); if (t == typeof(Vector3)) return EditorGUILayout.Vector3Field(l, v is Vector3 x?x:Vector3.zero); if (t == typeof(Vector2Int)) return EditorGUILayout.Vector2IntField(l, v is Vector2Int x?x:Vector2Int.zero); if (t == typeof(Vector3Int)) return EditorGUILayout.Vector3IntField(l, v is Vector3Int x?x:Vector3Int.zero); EditorGUILayout.LabelField(l, v?.ToString()??"null"); return v; }

    static void BuildCache() { if (_cacheBuilt) return; var ts = new List<Type>(); foreach (var a in AppDomain.CurrentDomain.GetAssemblies()) { if (a.GetName().Name.StartsWith("Unity")||a.GetName().Name.StartsWith("System")||a.GetName().Name=="mscorlib") continue; try { foreach (var t in a.GetTypes()) if (t.IsSubclassOf(typeof(ActionNode))&&!t.IsAbstract) ts.Add(t); } catch { } } ts = ts.OrderBy(t => t.Name).ToList(); _nodeTypes = ts.ToArray(); _nodeTypeNames = ts.Select(t => t.Name).ToArray(); _cacheBuilt = true; }
}
