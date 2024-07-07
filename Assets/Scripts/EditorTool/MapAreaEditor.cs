using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Tilemaps;

[ExecuteAlways]
public class MapAreaEditor : MonoBehaviour
{
    public int2 pos;
    public BehaviorAreaType behaviorAreaType;

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
    public void SetData(string name,BehaviorAreaType behaviorAreaType)
    {
        this.name = name;
        text.name = name;
        this.behaviorAreaType = behaviorAreaType; 
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
    }

    public NpcBehaviorArea GetAreaData()
    {
        tilemap = GetComponentInChildren<Tilemap>();
        text = GetComponentInChildren<TextMeshPro>();

        NpcBehaviorArea npcBehaviorArea = new NpcBehaviorArea
        {
            pos = this.pos,
            Name =int.Parse(text.text),
            behaviorAreaType = behaviorAreaType
        };
        for (int x = tilemap.cellBounds.xMin; x < tilemap.cellBounds.xMax; x++)
        {
            for (int y = tilemap.cellBounds.yMin; y < tilemap.cellBounds.yMax; y++)
            {
                var tile = tilemap.GetTile(new Vector3Int(x, y, 0));
                if (tile != null)
                {
                    npcBehaviorArea.cells.Add(new int2(x, y));
                }
            }
        }
        return npcBehaviorArea;
    }

    private void InitPos()
    {
        Vector3 pos = GameCommon.GetMapPos(new Vector2Int(this.pos.x, this.pos.y));
        transform.localPosition = pos;
        oldPos = pos;
    }

    // Update is called once per frame
    private void Update()
    {
        if (transform.position != oldPos)
        {
            pos = GameCommon.GetMapCoordinateInt(transform.localPosition);
            InitPos();
        }
    }
}