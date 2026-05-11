using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 带搜索的弹出选择器 — 每个节点独立使用，互不影响
/// </summary>
public class SearchablePopup : PopupWindowContent
{
    private string _search = "";
    private Vector2 _scroll;
    private string[] _allNames;
    private Type[] _allTypes;
    private Action<Type> _onSelect;
    private int _hoverIdx = -1;

    public static void Show(Type[] types, string[] names, Rect activatorRect, Action<Type> onSelect)
    {
        var popup = new SearchablePopup
        {
            _allTypes = types,
            _allNames = names,
            _onSelect = onSelect
        };
        PopupWindow.Show(activatorRect, popup);
    }

    public override Vector2 GetWindowSize()
    {
        return new Vector2(300, 350);
    }

    public override void OnGUI(Rect rect)
    {
        // Search field
        EditorGUI.BeginChangeCheck();
        _search = EditorGUILayout.TextField(_search, EditorStyles.toolbarSearchField);
        if (EditorGUI.EndChangeCheck())
            _hoverIdx = -1;

        // Filtered list
        var filtered = new List<int>();
        for (int i = 0; i < _allNames.Length; i++)
        {
            if (string.IsNullOrEmpty(_search) ||
                _allNames[i].IndexOf(_search, StringComparison.OrdinalIgnoreCase) >= 0)
                filtered.Add(i);
        }

        _scroll = EditorGUILayout.BeginScrollView(_scroll, GUILayout.Height(300));
        for (int i = 0; i < filtered.Count; i++)
        {
            int idx = filtered[i];
            var style = i == _hoverIdx ? "WhiteBoldLabel" : "Label";
            var r = EditorGUILayout.GetControlRect();
            if (r.Contains(Event.current.mousePosition))
                _hoverIdx = i;

            if (GUI.Button(r, _allNames[idx], EditorStyles.label))
            {
                _onSelect?.Invoke(_allTypes[idx]);
                editorWindow.Close();
                return;
            }
        }
        EditorGUILayout.EndScrollView();

        if (filtered.Count == 0)
            EditorGUILayout.LabelField("No matches");
    }
}
