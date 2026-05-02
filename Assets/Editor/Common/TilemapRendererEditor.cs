using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

[CustomEditor(typeof(TilemapRenderer))]
public class TilemapRendererEditor : Editor
{
    private const string FoldoutKeyPrefix = "TilemapRendererEditor.ShowChunkDebug.";
    private const string ShowUsedRangeKeyPrefix = "TilemapRendererEditor.ShowUsedRange.";
    private const string ShowChunkRangeKeyPrefix = "TilemapRendererEditor.ShowChunkRange.";
    private const string ShowCullingRangeKeyPrefix = "TilemapRendererEditor.ShowCullingRange.";
    private static readonly BindingFlags BindingFlagsAll = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
    private static Type builtInEditorType;
    private static bool builtInEditorTypeResolved;

    private TilemapRenderer tilemapRenderer => target as TilemapRenderer;
    private Editor builtInEditor;

    private void OnEnable()
    {
        CreateBuiltInEditor();
    }

    private void OnDisable()
    {
        if (builtInEditor != null)
        {
            DestroyImmediate(builtInEditor);
            builtInEditor = null;
        }
    }

    public override void OnInspectorGUI()
    {
        TilemapRenderer renderer = tilemapRenderer;
        if (renderer == null)
        {
            return;
        }

        if (builtInEditor == null)
        {
            CreateBuiltInEditor();
        }

        if (builtInEditor != null)
        {
            builtInEditor.OnInspectorGUI();
        }
        else
        {
            DrawDefaultInspector();
        }

        Tilemap tilemap = renderer.GetComponent<Tilemap>();

        EditorGUILayout.Space();
        bool expanded = EditorGUILayout.BeginFoldoutHeaderGroup(GetChunkDebugFoldout(renderer), "Chunk Debug");
        SetChunkDebugFoldout(renderer, expanded);
        if (!expanded)
        {
            EditorGUILayout.EndFoldoutHeaderGroup();
            return;
        }

        using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
        {
            EditorGUI.BeginChangeCheck();
            Vector3Int newChunkSize = EditorGUILayout.Vector3IntField("Chunk Size", renderer.chunkSize);
            if (EditorGUI.EndChangeCheck())
            {
                newChunkSize.x = Mathf.Max(1, newChunkSize.x);
                newChunkSize.y = Mathf.Max(1, newChunkSize.y);
                newChunkSize.z = Mathf.Max(1, newChunkSize.z);

                Undo.RecordObject(renderer, "Change Tilemap Chunk Size");
                renderer.chunkSize = newChunkSize;
                EditorUtility.SetDirty(renderer);
                SceneView.RepaintAll();
            }

            if (tilemap != null)
            {
                BoundsInt occupiedBounds = GetOccupiedCellBounds(tilemap);
                List<Vector2Int> occupiedChunks = GetOccupiedChunks(tilemap, renderer.chunkSize);
                EditorGUILayout.LabelField("Used Tile Bounds", FormatBounds(occupiedBounds));
                EditorGUILayout.LabelField("Occupied Chunk Count", occupiedChunks.Count.ToString());
                EditorGUILayout.LabelField("Culling Mode", renderer.detectChunkCullingBounds.ToString());

                bool showUsedRange = GetSessionBool(renderer, ShowUsedRangeKeyPrefix, true);
                bool newShowUsedRange = EditorGUILayout.ToggleLeft("Show Used Tile Range In Scene", showUsedRange);
                if (newShowUsedRange != showUsedRange)
                {
                    SetSessionBool(renderer, ShowUsedRangeKeyPrefix, newShowUsedRange);
                    SceneView.RepaintAll();
                }

                bool showChunkRange = GetSessionBool(renderer, ShowChunkRangeKeyPrefix, true);
                bool newShowChunkRange = EditorGUILayout.ToggleLeft("Show Occupied Chunks In Scene", showChunkRange);
                if (newShowChunkRange != showChunkRange)
                {
                    SetSessionBool(renderer, ShowChunkRangeKeyPrefix, newShowChunkRange);
                    SceneView.RepaintAll();
                }

                string cullingLabel = renderer.detectChunkCullingBounds == TilemapRenderer.DetectChunkCullingBounds.Manual
                    ? "Show Culling Range In Scene"
                    : "Show Renderer Bounds In Scene (Auto Approx)";
                bool showCullingRange = GetSessionBool(renderer, ShowCullingRangeKeyPrefix, true);
                bool newShowCullingRange = EditorGUILayout.ToggleLeft(cullingLabel, showCullingRange);
                if (newShowCullingRange != showCullingRange)
                {
                    SetSessionBool(renderer, ShowCullingRangeKeyPrefix, newShowCullingRange);
                    SceneView.RepaintAll();
                }

                if (renderer.detectChunkCullingBounds != TilemapRenderer.DetectChunkCullingBounds.Manual)
                {
                    EditorGUILayout.HelpBox("Auto mode does not expose Unity's exact chunk culling expansion. Scene preview uses Renderer.bounds as an approximation.", MessageType.Info);
                }
            }
            else
            {
                EditorGUILayout.HelpBox("Missing Tilemap on the same GameObject.", MessageType.Warning);
            }
        }

        EditorGUILayout.EndFoldoutHeaderGroup();
    }

    [DrawGizmo(GizmoType.Selected | GizmoType.NonSelected | GizmoType.Active)]
    private static void DrawTilemapRendererGizmos(TilemapRenderer renderer, GizmoType gizmoType)
    {
        if (renderer == null)
        {
            return;
        }

        Tilemap tilemap = renderer.GetComponent<Tilemap>();
        if (tilemap == null)
        {
            return;
        }

        DrawSceneDebug(tilemap, renderer);
    }

    private static void DrawSceneDebug(Tilemap tilemap, TilemapRenderer renderer)
    {
        BoundsInt usedBounds = GetOccupiedCellBounds(tilemap);
        GridLayout grid = tilemap.layoutGrid;

        if (grid == null || usedBounds.size.x <= 0 || usedBounds.size.y <= 0)
        {
            return;
        }

        if (GetSessionBool(renderer, ShowUsedRangeKeyPrefix, true))
        {
            using (new Handles.DrawingScope(new Color(1f, 0.45f, 0.1f, 0.95f)))
            {
                DrawChunkRect(grid, usedBounds.xMin, usedBounds.yMin, usedBounds.xMax, usedBounds.yMax, usedBounds.zMin);
            }
        }

        List<Vector2Int> occupiedChunks = GetOccupiedChunks(tilemap, renderer.chunkSize);
        if (GetSessionBool(renderer, ShowChunkRangeKeyPrefix, true))
        {
            using (new Handles.DrawingScope(new Color(1f, 0.8f, 0.05f, 0.95f)))
            {
                foreach (Vector2Int chunk in occupiedChunks)
                {
                    int minX = chunk.x * renderer.chunkSize.x;
                    int minY = chunk.y * renderer.chunkSize.y;
                    DrawChunkRect(grid, minX, minY, minX + renderer.chunkSize.x, minY + renderer.chunkSize.y, usedBounds.zMin);
                }
            }
        }

        if (!GetSessionBool(renderer, ShowCullingRangeKeyPrefix, true))
        {
            return;
        }

        if (renderer.detectChunkCullingBounds == TilemapRenderer.DetectChunkCullingBounds.Manual)
        {
            DrawManualCullingRanges(renderer, usedBounds, occupiedChunks);
        }
        else
        {
            using (new Handles.DrawingScope(new Color(0.2f, 1f, 1f, 0.95f)))
            {
                DrawWorldBounds(renderer.bounds);
            }
        }
    }

    private static void DrawChunkRect(GridLayout grid, int minX, int minY, int maxX, int maxY, int z)
    {
        Vector3 p0 = CellToWorld(grid, minX, minY, z);
        Vector3 p1 = CellToWorld(grid, minX, maxY, z);
        Vector3 p2 = CellToWorld(grid, maxX, maxY, z);
        Vector3 p3 = CellToWorld(grid, maxX, minY, z);

        Handles.DrawAAPolyLine(2f, p0, p1, p2, p3, p0);
    }

    private static Vector3 CellToWorld(GridLayout grid, int x, int y, int z)
    {
        return grid.LocalToWorld(grid.CellToLocalInterpolated(new Vector3(x, y, z)));
    }

    private static Vector2Int CalculateChunkCount(BoundsInt cellBounds, Vector3Int chunkSize)
    {
        if (chunkSize.x <= 0 || chunkSize.y <= 0 || cellBounds.size.x <= 0 || cellBounds.size.y <= 0)
        {
            return Vector2Int.zero;
        }

        int minChunkX = FloorDiv(cellBounds.xMin, chunkSize.x);
        int maxChunkX = FloorDiv(cellBounds.xMax - 1, chunkSize.x);
        int minChunkY = FloorDiv(cellBounds.yMin, chunkSize.y);
        int maxChunkY = FloorDiv(cellBounds.yMax - 1, chunkSize.y);

        return new Vector2Int(maxChunkX - minChunkX + 1, maxChunkY - minChunkY + 1);
    }

    private static List<Vector2Int> GetOccupiedChunks(Tilemap tilemap, Vector3Int chunkSize)
    {
        List<Vector2Int> chunks = new List<Vector2Int>();
        if (chunkSize.x <= 0 || chunkSize.y <= 0)
        {
            return chunks;
        }

        HashSet<Vector2Int> chunkSet = new HashSet<Vector2Int>();
        BoundsInt cellBounds = tilemap.cellBounds;
        foreach (Vector3Int position in cellBounds.allPositionsWithin)
        {
            if (!tilemap.HasTile(position))
            {
                continue;
            }

            Vector2Int chunk = new Vector2Int(FloorDiv(position.x, chunkSize.x), FloorDiv(position.y, chunkSize.y));
            if (chunkSet.Add(chunk))
            {
                chunks.Add(chunk);
            }
        }

        return chunks;
    }

    private static void DrawManualCullingRanges(TilemapRenderer renderer, BoundsInt usedBounds, List<Vector2Int> occupiedChunks)
    {
        GridLayout grid = renderer.GetComponent<Tilemap>()?.layoutGrid;
        if (grid == null)
        {
            return;
        }

        Vector3 expansion = renderer.transform.TransformVector(renderer.chunkCullingBounds);
        Vector3 expansionAbs = new Vector3(Mathf.Abs(expansion.x), Mathf.Abs(expansion.y), Mathf.Abs(expansion.z));

        using (new Handles.DrawingScope(new Color(1f, 0.2f, 0.65f, 0.95f)))
        {
            foreach (Vector2Int chunk in occupiedChunks)
            {
                int minX = chunk.x * renderer.chunkSize.x;
                int minY = chunk.y * renderer.chunkSize.y;
                DrawExpandedChunkRect(grid, minX, minY, minX + renderer.chunkSize.x, minY + renderer.chunkSize.y, usedBounds.zMin, expansionAbs);
            }
        }
    }

    private static void DrawExpandedChunkRect(GridLayout grid, int minX, int minY, int maxX, int maxY, int z, Vector3 expansion)
    {
        Vector3 p0 = CellToWorld(grid, minX, minY, z) - new Vector3(expansion.x, expansion.y, 0f);
        Vector3 p1 = CellToWorld(grid, minX, maxY, z) + new Vector3(-expansion.x, expansion.y, 0f);
        Vector3 p2 = CellToWorld(grid, maxX, maxY, z) + new Vector3(expansion.x, expansion.y, 0f);
        Vector3 p3 = CellToWorld(grid, maxX, minY, z) + new Vector3(expansion.x, -expansion.y, 0f);

        Handles.DrawAAPolyLine(2f, p0, p1, p2, p3, p0);
    }

    private static void DrawWorldBounds(Bounds bounds)
    {
        Vector3 min = bounds.min;
        Vector3 max = bounds.max;

        Vector3 p0 = new Vector3(min.x, min.y, min.z);
        Vector3 p1 = new Vector3(min.x, max.y, min.z);
        Vector3 p2 = new Vector3(max.x, max.y, min.z);
        Vector3 p3 = new Vector3(max.x, min.y, min.z);

        Handles.DrawAAPolyLine(2f, p0, p1, p2, p3, p0);
    }

    private static BoundsInt GetOccupiedCellBounds(Tilemap tilemap)
    {
        BoundsInt cellBounds = tilemap.cellBounds;
        bool foundTile = false;
        int minX = 0;
        int minY = 0;
        int minZ = 0;
        int maxX = 0;
        int maxY = 0;
        int maxZ = 0;

        foreach (Vector3Int position in cellBounds.allPositionsWithin)
        {
            if (!tilemap.HasTile(position))
            {
                continue;
            }

            if (!foundTile)
            {
                foundTile = true;
                minX = position.x;
                minY = position.y;
                minZ = position.z;
                maxX = position.x + 1;
                maxY = position.y + 1;
                maxZ = position.z + 1;
                continue;
            }

            if (position.x < minX) minX = position.x;
            if (position.y < minY) minY = position.y;
            if (position.z < minZ) minZ = position.z;
            if (position.x + 1 > maxX) maxX = position.x + 1;
            if (position.y + 1 > maxY) maxY = position.y + 1;
            if (position.z + 1 > maxZ) maxZ = position.z + 1;
        }

        if (!foundTile)
        {
            return new BoundsInt(cellBounds.position, Vector3Int.zero);
        }

        return new BoundsInt(
            new Vector3Int(minX, minY, minZ),
            new Vector3Int(maxX - minX, maxY - minY, maxZ - minZ));
    }

    private static int AlignDown(int value, int step)
    {
        return FloorDiv(value, step) * step;
    }

    private static int AlignUp(int value, int step)
    {
        return CeilDiv(value, step) * step;
    }

    private static int FloorDiv(int value, int divisor)
    {
        int quotient = value / divisor;
        int remainder = value % divisor;
        if (remainder != 0 && ((remainder < 0) != (divisor < 0)))
        {
            quotient--;
        }

        return quotient;
    }

    private static int CeilDiv(int value, int divisor)
    {
        return -FloorDiv(-value, divisor);
    }

    private static bool GetSessionBool(TilemapRenderer renderer, string prefix, bool defaultValue)
    {
        return SessionState.GetBool(GetSessionKey(renderer, prefix), defaultValue);
    }

    private static bool GetChunkDebugFoldout(TilemapRenderer renderer)
    {
        return SessionState.GetBool(GetFoldoutKey(renderer), false);
    }

    private static void SetChunkDebugFoldout(TilemapRenderer renderer, bool value)
    {
        SessionState.SetBool(GetFoldoutKey(renderer), value);
    }

    private static void SetSessionBool(TilemapRenderer renderer, string prefix, bool value)
    {
        SessionState.SetBool(GetSessionKey(renderer, prefix), value);
    }

    private static string FormatChunkSize(Vector3Int chunkSize)
    {
        return $"{chunkSize.x} x {chunkSize.y} x {chunkSize.z}";
    }

    private static string FormatBounds(BoundsInt bounds)
    {
        return $"Min ({bounds.xMin}, {bounds.yMin}, {bounds.zMin})  Size ({bounds.size.x}, {bounds.size.y}, {bounds.size.z})";
    }

    private static string GetFoldoutKey(TilemapRenderer renderer)
    {
        GlobalObjectId id = GlobalObjectId.GetGlobalObjectIdSlow(renderer);
        return FoldoutKeyPrefix + id;
    }

    private static string GetSessionKey(TilemapRenderer renderer, string prefix)
    {
        GlobalObjectId id = GlobalObjectId.GetGlobalObjectIdSlow(renderer);
        return prefix + id;
    }

    private void CreateBuiltInEditor()
    {
        if (builtInEditor != null)
        {
            return;
        }

        Type editorType = GetBuiltInEditorType();
        if (editorType != null)
        {
            builtInEditor = CreateEditor(targets, editorType);
        }
    }

    private static Type GetBuiltInEditorType()
    {
        if (builtInEditorTypeResolved)
        {
            return builtInEditorType;
        }

        builtInEditorTypeResolved = true;
        Type customEditorType = typeof(TilemapRendererEditor);
        FieldInfo inspectedTypeField = typeof(CustomEditor).GetField("m_InspectedType", BindingFlagsAll);

        foreach (Type editorType in TypeCache.GetTypesDerivedFrom<Editor>())
        {
            if (editorType == null || editorType == customEditorType || editorType.IsAbstract)
            {
                continue;
            }

            object[] attributes = editorType.GetCustomAttributes(typeof(CustomEditor), true);
            foreach (object attribute in attributes)
            {
                if (inspectedTypeField == null)
                {
                    continue;
                }

                Type inspectedType = inspectedTypeField.GetValue(attribute) as Type;
                if (inspectedType == typeof(TilemapRenderer))
                {
                    builtInEditorType = editorType;
                    return builtInEditorType;
                }
            }
        }

        return null;
    }
}
