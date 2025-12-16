using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

[ExecuteAlways]
public class MapItemInstanceEditor : MonoBehaviour
{
    public MapItem mapItem;
    private MapItemData itemData;

    private Vector3 oldPos;

    public void InitData(MapItemData mapItemData, int blindHomeEquipment, int intanceId, int2 coordinate, List<MapItemEventReferenceData> mapItemEventReferenceDatas)
    {
        this.itemData = mapItemData;
        mapItem.coordinate = coordinate;
        mapItem.id = itemData.id;
        mapItem.instanceId = intanceId;
        mapItem.eventReferenceDatas = mapItemEventReferenceDatas;
        mapItem.blindHomeEquipment = blindHomeEquipment;
        InitPos();
    }

    public void InitData(MapItemData mapItemData, int intanceId, Vector2 pos)
    {
        this.itemData = mapItemData;
        int2 coordinate = GameCommon.GetMapCoordinateInt(pos);
        mapItem.coordinate = coordinate;
        mapItem.id = itemData.id;
        mapItem.instanceId = intanceId;
        InitPos();
    }

    private void InitPos()
    {
        Vector3 pos = GameCommon.GetMapPos(mapItem.coordinate);
        transform.localPosition = pos;
        oldPos = pos;
    }

    // Start is called beforee the first frame update
    private void Start()
    {
    }

    // Update is called once per frame
    private void Update()
    {
        if (transform.localPosition != oldPos)
        {
            int2 coordinate = GameCommon.GetMapCoordinateInt(transform.localPosition);
            mapItem.coordinate = coordinate;
            InitPos();
        }
    }
}