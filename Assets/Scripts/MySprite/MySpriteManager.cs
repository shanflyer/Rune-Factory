using System.Collections.Generic;
using System.Drawing;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public class MySpriteMeshManager : Singleton<MySpriteMeshManager>
{
    #region Singleton


    #endregion Singleton

    #region Data Structures

    [BurstCompile]
    private struct MeshGenerationJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<float2> SourceVertices;
        [ReadOnly] public NativeArray<float2> SourceUVs;
        [ReadOnly] public NativeArray<ushort> SourceTriangles;
        [ReadOnly] public float2 PivotOffset; // 枢轴偏移量（像素单位）

        [WriteOnly] public NativeArray<float3> OutputVertices;
        [WriteOnly] public NativeArray<float2> OutputUVs;
        [WriteOnly] public NativeArray<int> OutputTriangles;

        public void Execute(int index)
        {
            // 处理顶点坐标（转换到以枢轴为中心）
            if (index < SourceVertices.Length)
            {
                float2 vertex = SourceVertices[index];
                OutputVertices[index] = new float3(
                    (vertex.x - PivotOffset.x), // 已转换为以枢轴为原点
                    (vertex.y - PivotOffset.y),
                    0
                );
                OutputUVs[index] = SourceUVs[index];
            }

            // 处理三角形索引
            if (index < SourceTriangles.Length)
            {
                OutputTriangles[index] = SourceTriangles[index];
            }
        }
    }

    #endregion Data Structures

    #region Member Variables

    // Sprite 缓存键使用 Unity 6 推荐的 EntityId，避免 int InstanceID 迁移风险。
    private Dictionary<EntityId, Mesh> _meshCache = new Dictionary<EntityId, Mesh>();
    private Dictionary<string, Material> _materialCache = new Dictionary<string, Material>();

    #endregion Member Variables

    #region Public Methods

    public (Mesh mesh, Material material) GetSpriteMesh(Sprite sprite, Material materialTemplate)
    {
        if (sprite == null || materialTemplate == null)
        {
            //Debug.LogError("Invalid input parameters");
            return (null, null);
        }

        EntityId spriteId = sprite.GetEntityId();
        string materialKey = GenerateMaterialKey(materialTemplate, sprite.texture);

        Mesh mesh = GetCachedMesh(spriteId, sprite);
        Material material = GetCachedMaterial(materialKey, materialTemplate, sprite);

        return (mesh, material);
    }

    #endregion Public Methods

    #region Mesh Generation

    private Mesh GetCachedMesh(EntityId spriteId, Sprite sprite)
    {
        if (!_meshCache.TryGetValue(spriteId, out Mesh mesh)||mesh==null)
        {
            mesh = GenerateSpriteMesh(sprite);
            _meshCache[spriteId]=( mesh);
        }
        return mesh;
    }

    private Mesh GenerateSpriteMesh(Sprite sprite)
    {
        // 获取原始数据（顶点已包含正确的pivot偏移）
        Vector2[] spriteVertices = sprite.vertices;
        Vector2[] spriteUV = sprite.uv;
        ushort[] spriteTriangles = sprite.triangles;

        // 计算枢轴偏移量（转换为像素坐标）
        float2 pivotOffset = new float2(
            sprite.pivot.x/ sprite.pixelsPerUnit,
            sprite.pivot.y/ sprite.pixelsPerUnit
        );


        // 准备Native数据
        NativeArray<float2> inputVertices = new NativeArray<float2>(spriteVertices.Length, Allocator.TempJob);
        for (int i = 0; i < spriteVertices.Length; i++)
        {
            // 转换顶点到像素坐标
            inputVertices[i] = new float2(
                spriteVertices[i].x ,
                spriteVertices[i].y
            );
        }

        NativeArray<float2> inputUVs = new NativeArray<float2>(spriteUV.Length, Allocator.TempJob);
        for (int i = 0; i < spriteUV.Length; i++)
        {
            inputUVs[i] = new float2(spriteUV[i].x, spriteUV[i].y);
        }

        NativeArray<ushort> inputTriangles = new NativeArray<ushort>(spriteTriangles, Allocator.TempJob);
        NativeArray<float3> outputVertices = new NativeArray<float3>(spriteVertices.Length, Allocator.TempJob);
        NativeArray<float2> outputUVs = new NativeArray<float2>(spriteUV.Length, Allocator.TempJob);
        NativeArray<int> outputTriangles = new NativeArray<int>(spriteTriangles.Length, Allocator.TempJob);

        // 设置Job参数
        var job = new MeshGenerationJob
        {
            SourceVertices = inputVertices,
            SourceUVs = inputUVs,
            SourceTriangles = inputTriangles,
            OutputVertices = outputVertices,
            OutputUVs = outputUVs,
            OutputTriangles = outputTriangles
        };

        // 调度Job
        int totalElements = Mathf.Max(spriteVertices.Length, spriteTriangles.Length);
        JobHandle handle = job.Schedule(totalElements, 64);
        handle.Complete();

        // 转换回Unity类型
        Vector3[] finalVertices = new Vector3[outputVertices.Length];
        for (int i = 0; i < outputVertices.Length; i++)
            finalVertices[i] = outputVertices[i];

        Vector2[] finalUVs = new Vector2[outputUVs.Length];
        for (int i = 0; i < outputUVs.Length; i++)
            finalUVs[i] = outputUVs[i];

        // 创建Mesh
        Mesh mesh = new Mesh();
        mesh.name = $"SpriteMesh_{sprite.GetEntityId()}";
        mesh.SetVertices(finalVertices);
        mesh.SetUVs(0, finalUVs);
        mesh.SetTriangles(outputTriangles.ToArray(), 0);
        mesh.bounds = CalculatePivotCenteredBounds(finalVertices, sprite.pixelsPerUnit);
        if (mesh.normals.Length == 0)
        {
            mesh.RecalculateNormals();
        }

        // 2. 生成切线
        mesh.RecalculateTangents();

        // 清理Native内存
        inputVertices.Dispose();
        inputUVs.Dispose();
        inputTriangles.Dispose();
        outputVertices.Dispose();
        outputUVs.Dispose();
        outputTriangles.Dispose();

        return mesh;
    }

    private Bounds CalculatePivotCenteredBounds(Vector3[] vertices, float pixelsPerUnit)
    {
        if (vertices.Length == 0) return new Bounds();

        // 计算各轴最大偏移量
        float maxX = 0, maxY = 0;
        foreach (Vector3 vertex in vertices)
        {
            maxX = Mathf.Max(maxX, Mathf.Abs(vertex.x));
            maxY = Mathf.Max(maxY, Mathf.Abs(vertex.y));
        }

        // 创建以枢轴为中心的包围盒
        return new Bounds(
            Vector3.zero, // 中心点始终为原点
            new Vector3(
                maxX * 2, // 转换为Unity单位
                maxY * 2,
                0.1f
            )
        );
    }

    #endregion Mesh Generation

    #region Material Management

    private Material GetCachedMaterial(string materialKey, Material template, Sprite sprite)
    {
        if (!_materialCache.TryGetValue(materialKey, out Material material)||material==null)
        {
            material = CreateMaterialInstance(template, sprite);
            _materialCache[materialKey]=( material);
        }

        return material;
    }

    private string GenerateMaterialKey(Material template, Texture2D mainTexture)
    {
        return $"{template.GetEntityId()}_{mainTexture.GetEntityId()}";
    }

    private Material CreateMaterialInstance(Material template, Sprite sprite)
    {
        Material mat = new Material(template)
        {
            name = $"{template.name}_Inst_{sprite.texture.name}",
            enableInstancing = true
        };

        ApplySpriteTextures(mat, sprite);
        return mat;
    }

    private void ApplySpriteTextures(Material material, Sprite sprite)
    {
        material.SetTexture("_MainTex", sprite.texture);
        if (sprite == null)
        {
            return;
        }
        SecondarySpriteTexture[] secondarySpriteTextures = new SecondarySpriteTexture[sprite.GetSecondaryTextureCount()];
        if (secondarySpriteTextures.Length == 0)
        {
            return;
        }
        try
        {
            sprite.GetSecondaryTextures(secondarySpriteTextures);

            for (int i = 0; i < secondarySpriteTextures.Length; i++)
            {
                material.SetTexture(secondarySpriteTextures[i].name, secondarySpriteTextures[i].texture);
            }
        }
        catch
        {

        }

    }

    #endregion Material Management

    #region Cleanup
    protected override void Clear()
    {
        base.Clear();
        foreach (var mesh in _meshCache.Values)
            if (mesh != null) GameObject.Destroy(mesh);

        foreach (var mat in _materialCache.Values)
            if (mat != null) GameObject.Destroy(mat);

        _materialCache.Clear();
        _meshCache.Clear();
    }


    #endregion Cleanup
}
