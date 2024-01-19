using UnityEditor;
using UnityEngine;

public class UIToolEditor
{
    [MenuItem("Assets/UI填充")]
    public static void SetPanelUISerializeObj()
    {
        Object[] selection = Selection.GetFiltered(typeof(Object), SelectionMode.DeepAssets);
        string[] resources = new string[selection.Length];

        try
        {
            AssetDatabase.StartAssetEditing();
            for (int i = 0; i < selection.Length; i++)
            {
                resources[i] = AssetDatabase.GetAssetPath(selection[i]);

                if (selection[i].GetType() == typeof(GameObject))
                {
                    BaseReference gamePanel;
                    if (((GameObject)selection[i]).TryGetComponent(out gamePanel))
                    {
                        gamePanel.SetPanelUISerializeObj();

                        EditorUtility.SetDirty(gamePanel);
                        AssetDatabase.SaveAssets();
                    }
                }
            }
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
        }
    }
}