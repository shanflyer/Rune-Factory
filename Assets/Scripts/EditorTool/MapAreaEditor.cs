#if UNITY_EDITOR
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

[ExecuteAlways]
public class MapAreaEditor : MonoBehaviour
{
    public int2 pos;
    public BehaviorAreaType behaviorAreaType;
    public TilemapRenderer tilemapRenderer;
    public int areaLinkData;
    public Direction direction;
    [SerializeField]
    private Tilemap tilemap;

    [SerializeField]
    private TextMeshPro text;

    private Vector3 oldPos;
     
    private void Awake()
    {
        tilemap = GetComponentInChildren<Tilemap>();
        text = GetComponentInChildren<TextMeshPro>();
    }
    public void SetData(int name, BehaviorAreaType behaviorAreaType)
    {
        this.name = name.ToString();
        text.text=text.name = name.ToString();
        this.behaviorAreaType = behaviorAreaType;
         
    }
    public void SetData(SpecialNpcBehaviorArea SpecialNpcBehaviorArea)
    { 
        var gridCount = SpecialNpcBehaviorArea.grids.Count / 4;
        for (int j = 0; j < gridCount; j++)
        {
            int minX = SpecialNpcBehaviorArea.grids[j * 4];
            int minY = SpecialNpcBehaviorArea.grids[j * 4 + 1];
            int maxX = SpecialNpcBehaviorArea.grids[j * 4 + 2];
            int maxY = SpecialNpcBehaviorArea.grids[j * 4 + 3];

            List<Vector3Int> poses = new List<Vector3Int>();
            List<TileBase> tileBases = new List<TileBase>();
            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    poses.Add(new Vector3Int(x, y));
                    tileBases.Add(MapInstanceEditor.defaultTile);
                }
            }
            tilemap.SetTiles(poses.ToArray(), tileBases.ToArray());
        }
        gameObject.name = text.text = SpecialNpcBehaviorArea.Name.ToString();
        transform.position = GameCommon.GetZeroMapPos(SpecialNpcBehaviorArea.pos);
        this.behaviorAreaType = BehaviorAreaType.特殊;
        direction = SpecialNpcBehaviorArea.fixedDirection;
        areaLinkData = SpecialNpcBehaviorArea.tempCreatId;
    }
    public void SetData(NpcBehaviorArea npcBehaviorArea)
    {
        var areaData = npcBehaviorArea;
        var gridCount = areaData.grids.Count / 4;
        for (int j = 0; j < gridCount; j++)
        {
            int minX = areaData.grids[j * 4];
            int minY = areaData.grids[j * 4 + 1];
            int maxX = areaData.grids[j * 4 + 2];
            int maxY = areaData.grids[j * 4 + 3];

            List<Vector3Int> poses = new List<Vector3Int>();
            List<TileBase> tileBases = new List<TileBase>();
            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    poses.Add(new Vector3Int(x, y));
                    tileBases.Add(MapInstanceEditor.defaultTile);
                }
            }
            tilemap.SetTiles(poses.ToArray(), tileBases.ToArray());
        }
        gameObject.name = text.text = areaData.Name.ToString(); 
        transform.position = GameCommon.GetZeroMapPos(areaData.pos);
        this.behaviorAreaType = npcBehaviorArea.behaviorAreaType;
      
    }

    public SpecialNpcBehaviorArea GetSpecialData()
    {
        if (behaviorAreaType != BehaviorAreaType.特殊)
        {
            return null;
        }
        SpecialNpcBehaviorArea SpecialNpcBehaviorArea = new SpecialNpcBehaviorArea();
        SpecialNpcBehaviorArea.Name = int.Parse(text.name);
        SpecialNpcBehaviorArea.fixedDirection = direction;
        SpecialNpcBehaviorArea.tempCreatId = areaLinkData;
        tilemap = GetComponentInChildren<Tilemap>();
        text = GetComponentInChildren<TextMeshPro>();
        List<int2> cells = new List<int2>();
        for (int x = tilemap.cellBounds.xMin; x < tilemap.cellBounds.xMax; x++)
        {
            for (int y = tilemap.cellBounds.yMin; y < tilemap.cellBounds.yMax; y++)
            {
                var tile = tilemap.GetTile(new Vector3Int(x, y, 0));
                if (tile != null)
                {
                    cells.Add(new int2(x, y));
                }
            }
        }
        SpecialNpcBehaviorArea.grids = GameCommon.CellToGrid(cells);
        SpecialNpcBehaviorArea.pos = pos;
        return SpecialNpcBehaviorArea;
    }
    public NpcBehaviorArea GetAreaData()
    {
        if (behaviorAreaType == BehaviorAreaType.特殊)
        {
            return null;
        }

        tilemap = GetComponentInChildren<Tilemap>();
        text = GetComponentInChildren<TextMeshPro>();
        NpcBehaviorArea areaData = new NpcBehaviorArea();
        areaData.pos = pos;
        areaData.Name = int.Parse(text.text);
        List<int2> cells = new List<int2>();
        for (int x = tilemap.cellBounds.xMin; x < tilemap.cellBounds.xMax; x++)
        {
            for (int y = tilemap.cellBounds.yMin; y < tilemap.cellBounds.yMax; y++)
            {
                var tile = tilemap.GetTile(new Vector3Int(x, y, 0));
                if (tile != null)
                {
                    cells.Add(new int2(x, y));
                }
            }
        }
        areaData.grids = GameCommon.CellToGrid(cells);
        areaData.behaviorAreaType = behaviorAreaType;
        return areaData;
    }

    private void InitPos()
    {
        Vector3 pos = GameCommon.GetMapPos(new Vector2Int(this.pos.x, this.pos.y));
        transform.localPosition = pos;
        oldPos = pos;
    }

    BehaviorAreaType OldbehaviorAreaType;
    // Update is called once per frame
    private void Update()
    {
        if (transform.position != oldPos)
        {
            pos = GameCommon.GetMapCoordinateInt(transform.localPosition);
            InitPos();
        }
        if (OldbehaviorAreaType != behaviorAreaType)
        {
            OldbehaviorAreaType = behaviorAreaType;
            switch (behaviorAreaType)
            {
                case BehaviorAreaType.创建:
                    tilemap.color = Color.white;
                    break;
                case BehaviorAreaType.聚集:
                    tilemap.color = Color.blue;
                    break;
                case BehaviorAreaType.消失:
                    tilemap.color = Color.red;
                    break;
                case BehaviorAreaType.特殊:
                    tilemap.color = new Color(1, 1, 0);
                    break;
            }
        }
    }
}
#endif
