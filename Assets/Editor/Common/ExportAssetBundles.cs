using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


public class ExportAssetBundles
{
    [MenuItem("Assets/Bundle/Build (iOS)AssetBundle From selection - Track dependencies")]
    static void ExportIosResource()
    {
        string path = "Assets/Ios";
        if (path.Length != 0)
        {
            Object[] selection = Selection.GetFiltered(typeof(Object), SelectionMode.DeepAssets);
            string[] resources =new string[selection.Length];
            AssetBundleBuild[] buildMap = new AssetBundleBuild[1];

            for (int i = 0; i < selection.Length; i++)
            {
                resources[i] = AssetDatabase.GetAssetPath(selection[i]);
            }
           
            buildMap[0].assetBundleName = Selection.activeObject.name;
            buildMap[0].assetNames = resources;
            BuildPipeline.BuildAssetBundles(path, buildMap, BuildAssetBundleOptions.ChunkBasedCompression, BuildTarget.iOS);
        }
    }
    [MenuItem("Assets/Bundle/Build (Android)AssetBundle From selection - Track dependencies")]
    static void ExportAndriodResource()
    {
        string path = "Assets/Android";
        if (path.Length != 0)
        {
            Object[] selection = Selection.GetFiltered(typeof(Object), SelectionMode.DeepAssets);
            string[] resources = new string[selection.Length];
            AssetBundleBuild[] buildMap = new AssetBundleBuild[1];

            for (int i = 0; i < selection.Length; i++)
            {
                resources[i] = AssetDatabase.GetAssetPath(selection[i]);
            }

            buildMap[0].assetBundleName = Selection.activeObject.name;
            buildMap[0].assetNames = resources;
            BuildPipeline.BuildAssetBundles(path, buildMap, BuildAssetBundleOptions.ChunkBasedCompression, BuildTarget.Android);
        }
    }
    [MenuItem("Assets/Bundle/Build (PC)AssetBundle From selection - Track dependencies")]
    static void ExportPCdResource()
    {
        string path = "Assets/PC";
        if (path.Length != 0)
        {
            Object[] selection = Selection.GetFiltered(typeof(Object), SelectionMode.DeepAssets);
            string[] resources = new string[selection.Length];
            AssetBundleBuild[] buildMap = new AssetBundleBuild[1];

            for (int i = 0; i < selection.Length; i++)
            {
                resources[i] = AssetDatabase.GetAssetPath(selection[i]);
            }

            buildMap[0].assetBundleName = Selection.activeObject.name;
            buildMap[0].assetNames = resources;
            BuildPipeline.BuildAssetBundles(path, buildMap, BuildAssetBundleOptions.ChunkBasedCompression, BuildTarget.StandaloneWindows);
        }
    }

}

