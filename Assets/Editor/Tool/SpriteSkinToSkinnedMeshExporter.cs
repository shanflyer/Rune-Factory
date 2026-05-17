using UnityEngine;
using UnityEditor;
using UnityEngine.U2D.Animation;
using UnityEngine.U2D;
using System;
using System.IO;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine.Rendering;
using static UnityEditor.Recorder.OutputPath;

public class CombinedSpriteSkinExporter
{
    [MenuItem("Tools/Export SkinnedMeshes Per Child (Unity 6.1+)")]
    static void ExportPerChild()
    {
        GameObject prefab = Selection.activeGameObject;
        if (prefab == null)
        {
            Debug.LogError("请选中一个包含 SpriteSkin 的 prefab！");
            return;
        }

        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        instance.name = prefab.name + "_TempInstance";

        string folder = "Assets/ExportedSkinnedMeshes";
        if (!AssetDatabase.IsValidFolder(folder))
            AssetDatabase.CreateFolder("Assets", "ExportedSkinnedMeshes");

        for (int index = instance.transform.childCount - 1; index >= 0; index--)
        {
            Transform child = instance.transform.GetChild(index);

            var spriteSkins = child.GetComponentsInChildren<SpriteSkin>(true);
            if (spriteSkins.Length == 0) continue;

            List<Vector3> allVertices = new();
            List<Vector2> allUVs = new();
            List<BoneWeight> allBoneWeights = new();
            List<Matrix4x4> allBindposes = new();
            List<Transform> allBones = new();
            List<Material> allMaterials = new();
            List<int[]> allTriangles = new();

            int vertexOffset = 0;
            int boneOffset = 0;

            // 排序：按 sortingOrder 升序排列部件
            Array.Sort(spriteSkins, (a, b) =>
                a.GetComponent<SpriteRenderer>().sortingOrder.CompareTo(
                b.GetComponent<SpriteRenderer>().sortingOrder));

            foreach (var spriteSkin in spriteSkins)
            {
                var spriteRenderer = spriteSkin.GetComponent<SpriteRenderer>();

                if (spriteSkin.boneTransforms == null || spriteSkin.boneTransforms.Length == 0)
                    continue;

                var sprite = spriteRenderer.sprite;
                if (sprite == null)
                    continue;

                var positions = CopyNativeSlice(sprite.GetVertexAttribute<Vector3>(VertexAttribute.Position));
                var uvs = sprite.uv;
                var triangles = sprite.triangles;
                var weights = CopyNativeSlice(sprite.GetVertexAttribute<BoneWeight>(VertexAttribute.BlendWeight));
                var bindposes = CopyNativeArray(sprite.GetBindPoses());

                for (int i = 0; i < positions.Length; i++)
                {
                    allVertices.Add(positions[i]);
                    allUVs.Add(uvs[i]);
                }

                int[] tri = new int[triangles.Length];
                for (int i = 0; i < triangles.Length; i++)
                    tri[i] = triangles[i] + vertexOffset;
                allTriangles.Add(tri);

                for (int i = 0; i < weights.Length; i++)
                {
                    var bw = weights[i];
                    bw.boneIndex0 += boneOffset;
                    bw.boneIndex1 += boneOffset;
                    bw.boneIndex2 += boneOffset;
                    bw.boneIndex3 += boneOffset;
                    allBoneWeights.Add(bw);
                }

                allBindposes.AddRange(bindposes);
                allBones.AddRange(spriteSkin.boneTransforms);

                // ✅ 保存材质文件
                string matPath = $"{folder}/{prefab.name}_{spriteRenderer.name}_Material.mat";
                Material matAsset = AssetDatabase.LoadAssetAtPath<Material>(matPath);
                if (matAsset == null)
                {
                    Material mat = new Material(Shader.Find("Sprites/Default"));
                    mat.mainTexture = sprite.texture;
                    mat.renderQueue = 3000 + spriteRenderer.sortingOrder;

                    SecondarySpriteTexture[] secondarySpriteTextures = new SecondarySpriteTexture[sprite.GetSecondaryTextureCount()];
                    sprite.GetSecondaryTextures(secondarySpriteTextures);
                    foreach (var sst in secondarySpriteTextures)
                    {
                        mat.SetTexture(sst.name, sst.texture);
                    }

                    AssetDatabase.CreateAsset(mat, matPath);
                    matAsset = mat;
                }

                allMaterials.Add(matAsset);

                vertexOffset += positions.Length;
                boneOffset += spriteSkin.boneTransforms.Length;
            }

            allBones.RemoveAll(b => b == null);
            if (allVertices.Count == 0 || allBones.Count == 0) continue;

            Mesh mesh = new Mesh();
            mesh.name = child.name + "_MergedMesh";
            mesh.SetVertices(allVertices);
            mesh.SetUVs(0, allUVs);
            mesh.boneWeights = allBoneWeights.ToArray();
            mesh.bindposes = allBindposes.ToArray();
            mesh.subMeshCount = allTriangles.Count;

            for (int i = 0; i < allTriangles.Count; i++)
                mesh.SetTriangles(allTriangles[i], i);

            string meshPath = $"{folder}/{child.name}_MergedMesh.asset";
            AssetDatabase.CreateAsset(mesh, meshPath);
            Mesh meshAsset = AssetDatabase.LoadAssetAtPath<Mesh>(meshPath);

            GameObject resultChild = new GameObject(child.name);
            resultChild.transform.SetParent(instance.transform);
            resultChild.transform.localPosition = child.localPosition;
            resultChild.transform.localRotation = child.localRotation;
            resultChild.transform.localScale = child.localScale;
            resultChild.transform.SetSiblingIndex(child.GetSiblingIndex());
            GameObject.DestroyImmediate(child.gameObject);

            var smr = resultChild.AddComponent<SkinnedMeshRenderer>();
            smr.sharedMesh = meshAsset;
            smr.bones = allBones.ToArray();
            smr.rootBone = spriteSkins[0].rootBone;
            smr.materials = allMaterials.ToArray();
        }

        string prefabPath = $"{folder}/{prefab.name}_Merged.prefab";
        PrefabUtility.SaveAsPrefabAsset(instance, prefabPath);
        Debug.Log("✅ 合并导出成功：" + prefabPath);

        GameObject.DestroyImmediate(instance);
    }

    static T[] CopyNativeSlice<T>(NativeSlice<T> slice) where T : struct
    {
        T[] arr = new T[slice.Length];
        for (int i = 0; i < slice.Length; i++)
            arr[i] = slice[i];
        return arr;
    }

    static T[] CopyNativeArray<T>(NativeArray<T> array) where T : struct
    {
        T[] arr = new T[array.Length];
        for (int i = 0; i < array.Length; i++)
            arr[i] = array[i];
        return arr;
    }
}
