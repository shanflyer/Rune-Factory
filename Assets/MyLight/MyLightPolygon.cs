using System.Collections;
using System.Collections.Generic;
using UnityEngine; 
using Unity.Mathematics;
using System;  

[RequireComponent(typeof(PolygonCollider2D))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
 [ExecuteAlways]
public class MyLightPolygon : MyLightBase
{
    [SerializeField]
    PolygonCollider2D polygonCollider;
    [SerializeField]
    MeshFilter meshFilter; 
    MeshRenderer meshRenderer=>renderer as MeshRenderer; 
    [SerializeField]
    Vector2 lerpLength;
    [SerializeField]
    Vector3 lerpOffset;
    [SerializeField]
    [Range(-10,1)]
    float lerpValue;  
    [SerializeField]
    bool PiovotCenter = false;
    [SerializeField]
    bool LerpPiovotCenter = true;
    [SerializeField]
    Vector3 normalOffset = Vector3.zero;
    public PolygonCollider2D PolygonCollider => polygonCollider;

    public override void OnEnable()
    {
        if (meshRenderer == null || meshRenderer.sharedMaterial == null)
        {
            renderer = GetComponent<Renderer>();
            polygonCollider = GetComponent<PolygonCollider2D>();
            meshFilter = GetComponent<MeshFilter>(); 
           // meshRenderer.sharedMaterial = Resources.Load<Material>("MyShadow");
        }
        //
    }

 
    private void Awake()
    {
        
       
    }
 
    private void Start()
    {
      
    }
    private void OnTransformParentChanged()
    {
        CreateMesh(); 
    }
    public override void RefreshColor()
    {
        base.RefreshColor();
        if (meshFilter.sharedMesh == null)
        {
            CreateMesh();
        }
        var mesh = meshFilter.sharedMesh;
        Color[] colors = mesh.colors;
        for (int i = 0; i < colors.Length; i++)
        {
            colors[i] = lightColor;
        }
        mesh.colors = colors;
    } 
    public void CreateMesh()
    {
        Vector3 localScale = transform.localScale;
        Vector3 localEulerAngle = transform.localEulerAngles;
        transform.localScale = Vector3.one;
        transform.localEulerAngles = Vector3.zero;
        try
        { 

            var mesh = polygonCollider.CreateMesh(false, false);
            var points= polygonCollider.points;

          
           // string pointStr = "point:";
            Dictionary<Vector3, int> pointIndex = new Dictionary<Vector3, int>(); ;
            for(int i = 0; i < points.Length; i++)
            {
               //pointStr += points[i];
                points[i].x = math.ceil(points[i].x*1000)*0.001f;
                points[i].y = math.ceil(points[i].y * 1000) * 0.001f;
                pointIndex.Add(points[i], i);
               
            }
           // string meshStr = "mesh";

            int[] meshIndexes = new int[points.Length];
            Vector3[] vertices = mesh.vertices;
            for (int i = 0; i < vertices.Length; i++)
            {// meshStr += mesh.vertices[i];
                vertices[i] = vertices[i] - transform.position;
                Vector2 key = vertices[i];
                key.x = math.ceil(key.x * 1000) * 0.001f;
                key.y = math.ceil(key.y * 1000) * 0.001f;
                if (pointIndex.TryGetValue(key, out var index))
                {
                    meshIndexes[index] = i;
                }
            }

            
           
            Color[] colors=new Color[points.Length * 5];
            int[] ts = mesh.triangles; 
            Vector2[] uv=new Vector2[points.Length*5];
            Vector2[] uv2 = new Vector2[points.Length * 5];
            int v_start = vertices.Length;
            int t_start = ts.Length;
             
            Array.Resize<Vector3>(ref vertices, vertices.Length*5); 
            Array.Resize<int>(ref ts, ts.Length + points.Length * 6);

            var center = LerpPiovotCenter ? mesh.bounds.center - transform.position : Vector3.zero;
            Vector2 uv2center =center+ normalOffset;
            //center = center - transform.position;
            for (int i = 0; i < meshIndexes.Length; i++)
            {
                uv2[i] = uv2center;
                colors[i] = lightColor;
                uv[i] = new Vector2(1, 1);

                int index = meshIndexes[i];
                Vector3 normalized = (vertices[index]- center).normalized;

                Vector3 offset = vertices[index] - center;

                vertices[v_start +i*2] = vertices[index];
                uv[v_start + i * 2] = new Vector2(1, 1);
                uv2[v_start + i * 2] = uv2center;

                vertices[v_start + i * 2+1] = vertices[index];
                uv[v_start + i * 2 + 1] = new Vector2(1, 1);
                uv2[v_start + i * 2 + 1] = uv2center;

                Vector3 offSetLength = new Vector3(offset.x * lerpLength.x+normalized.x*lerpOffset.x, 
                    offset.y * lerpLength.y+lerpOffset.y*normalized.y, offset.z);

                vertices[v_start+ meshIndexes.Length * 2 + i * 2] = vertices[index] + offSetLength;
                uv[v_start + meshIndexes.Length * 2 + i * 2] = new Vector2(lerpValue, lerpValue);
                uv2[v_start + meshIndexes.Length * 2 + i * 2] = uv2center;// vertices[index];

                vertices[v_start + meshIndexes.Length * 2 + i * 2 + 1] = vertices[index] + offSetLength;
                uv[v_start + meshIndexes.Length * 2 + i * 2 + 1] = new Vector2(lerpValue, lerpValue);
                uv2[v_start + meshIndexes.Length * 2 + i * 2 + 1] = uv2center;// vertices[index];

                colors[v_start + i * 2] = lightColor;
                colors[v_start + meshIndexes.Length * 2 + i * 2] = lightColor;
                colors[v_start + i * 2+1] = lightColor;
                colors[v_start + meshIndexes.Length * 2 + i * 2 + 1] = lightColor;

            }
            for (int i = 0; i < points.Length; i++)
            {
                int offsetIndex = i % 2;

                int index0 = v_start*3 + i*2+ offsetIndex;
                int index1 = index0 + 2;
                if (i >= points.Length - 1)
                {
                    index1 = v_start*3 + offsetIndex;
                }
                int index2 = v_start + i * 2 + offsetIndex;
                int index3 = index2 + 2;
                if (i >= points.Length - 1)
                {
                    index3 = v_start + offsetIndex;
                }
                ts[t_start + i * 6] = index0;
                ts[t_start + i * 6 + 1] = index3;
                ts[t_start + i * 6 + 2] = index1;
                ts[t_start + i * 6 + 3] = index3;
                ts[t_start + i * 6 + 4] = index0;
                ts[t_start + i * 6 + 5] = index2;
            }


                /*
                for (int i = 0; i < meshIndexes.Length; i++)
                {
                    Vector3 normalized = vertices[meshIndexes[i]].normalized;
                    Vector3 newPoint = vertices[meshIndexes[i]]+ lerpLength* normalized;
                    vertices[v_start+i] = newPoint; 
                    uv[meshIndexes[i]] = new Vector2(1, 1); 
                    uv[v_start + i] = new Vector2(lerpValue, lerpValue);
                    uv2[meshIndexes[i]] = Vector3.zero;
                    uv2[v_start + i] = vertices[meshIndexes[i]];
                    colors[i] = color;
                    colors[v_start + i] = color;
                }
                for(int i = 0; i < points.Length; i++)
                {
                    int index0 = v_start + i;
                    int index1 = index0 + 1;
                    if(i>=points.Length-1)
                    {
                        index1 = v_start;
                    }
                    int index2 = meshIndexes[i];
                    int index3= meshIndexes[i<points.Length-1? i+1:0];

                    ts[t_start + i * 6] = index0;
                    ts[t_start + i * 6 + 1] = index3;
                    ts[t_start + i * 6 + 2] = index1;
                    ts[t_start + i * 6 + 3] = index3;
                    ts[t_start + i * 6 + 4] = index0;
                    ts[t_start + i * 6 + 5] = index2;
                }*/
            var newMesh = new Mesh();
            newMesh.Clear();
            newMesh.vertices = vertices;
            newMesh.colors = colors; 
            newMesh.triangles = ts;
            newMesh.uv = uv;
            newMesh.uv2 = uv2;
            newMesh.RecalculateBounds();
            meshFilter.mesh = newMesh;
            
             
        }
        catch(Exception e)
        {
            Debug.Log(e);
        }
        finally
        {
            transform.localScale = localScale;
            transform.localEulerAngles = localEulerAngle;
        }
       
    }

    public override void Test()
    {
        base.Test();
        CreateMesh();
    }
} 