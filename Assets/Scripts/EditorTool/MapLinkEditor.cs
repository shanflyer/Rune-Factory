#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
[ExecuteAlways]
public class MapLinkEditor : MonoBehaviour
{
    private Transform startPoint, endPoint;
    private Vector3 startPos, endPos;
    private LineRenderer lineRenderer;

    private Vector3 mapPos0, mapPos1;


    private void OnEnable()
    {
        startPoint = transform.GetChild(0);
        endPoint = transform.GetChild(1);
        startPos = startPoint.position;
        endPos = endPoint.position;
        lineRenderer = GetComponent<LineRenderer>();
        SetLinePoint();
    }
    [HideInInspector]
    public MapLine mapLine;
    private MapInstanceEditor mapInstance0, mapInstance1;

    public bool CheckLink(int mapId)
    {
        return mapLine.map0 == mapId || mapLine.map1 == mapId;
    }
    public void InitLinkData(MapLine mapLine)
    {
        this.mapLine = mapLine;
        mapInstance0 = WorldInstanceEditor.Instance.mapInstanceEditors[mapLine.map0];
        mapInstance1 = WorldInstanceEditor.Instance.mapInstanceEditors[mapLine.map1];

        mapPos0 = mapInstance0.transform.position;
        mapPos1 = mapInstance1.transform.position;

        SetLinePointPos();
    }
    void SetLinePoint()
    {
        Vector3[] positions = new Vector3[2]
        {
            startPos,endPos
        };
        lineRenderer.SetPositions(positions);
    }

    void SetLinePointPos()
    {
        Vector2 _startPos = mapInstance0.GetMapPos(mapLine.cell0);
        startPoint.position = new Vector3(_startPos.x, _startPos.y, startPoint.position.z);

        Vector2 _endPos = mapInstance1.GetMapPos(mapLine.cell1);
        endPoint.position = new Vector3(_endPos.x, _endPos.y, endPoint.position.z);

        mapPos0 = mapInstance0.transform.position;
        mapPos1 = mapInstance1.transform.position;

        startPos = startPoint.position;
        endPos = endPoint.position;

        SetLinePoint();
    }

    // Update is called once per frame
    void Update()
    {
        if (mapPos0 != mapInstance0.transform.position || mapPos1 != mapInstance1.transform.position)
        {
            SetLinePointPos();
        }
        else if (startPos != startPoint.position || endPos != endPoint.position)
        {
            int2 startCoordinate = GameCommon.GetMapCoordinateInt(startPoint.position);
            Vector2 _startPos = GameCommon.GetMapPos(startCoordinate);
            startPoint.position = new Vector3(_startPos.x, _startPos.y, startPoint.position.z);

            int2 endCoordinate = GameCommon.GetMapCoordinateInt(endPoint.position);
            Vector2 _endPos = GameCommon.GetMapPos(endCoordinate);
            endPoint.position = new Vector3(_endPos.x, _endPos.y, endPoint.position.z);

            startPos = startPoint.position;
            endPos = endPoint.position;
            SetLinePoint();

            if (!WorldInstanceEditor.Instance.InitLinkMap(startCoordinate, endCoordinate, ref mapLine))
            {
                DestroyImmediate(gameObject);
            }
            mapInstance0 = WorldInstanceEditor.Instance.mapInstanceEditors[mapLine.map0];
            mapInstance1 = WorldInstanceEditor.Instance.mapInstanceEditors[mapLine.map1];
        }
    }
}
#endif

