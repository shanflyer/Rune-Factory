using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/SpecialMapLink")]
public class SpecialMapLink : ScriptableObject, IGameData
{
    public int specialId;
    public SpecialMap map0;
    public List<SpecialMap> map1;
    public List<int> maps;

    public bool IsMatchTarget(int mapId)
    {
        if (maps != null && maps.Count > 0) return maps.Contains(mapId);
        return false;
    }

    public bool IsMatchTargetMap(int mapId, int2 coordinate, out SpecialMap map)
    {
        map = default;
        if (map1 != null && map1.Count > 0)
        {
            for (var i = 0; i < map1.Count; i++)
            {
                var specialMap = map1[i];
                if (specialMap.specialMap == mapId && specialMap.IsInArea(coordinate))
                {
                    map = specialMap;
                    return true;
                }
            }

            return false;
        }

        return true;
    }

    public string GetKey()
    {
        return name;
    }

    public void SetReferenceData()
    {
    }
}

[Serializable]
public struct SpecialMap
{
    public int specialMap;
    public List<int> specialAreas;

    public int2 center
    {
        get
        {
            if (specialAreas != null && specialAreas.Count > 0)
            {
                var gridCount = specialAreas.Count / 4;
                var _minX = int.MaxValue;
                var _maxX = int.MinValue;
                var _maxY = int.MinValue;
                var _minY = int.MaxValue;
                for (var i = 0; i < gridCount; i++)
                {
                    var minX = specialAreas[i * 4];
                    var minY = specialAreas[i * 4 + 1];

                    var maxX = specialAreas[i * 4 + 2];
                    var maxY = specialAreas[i * 4 + 3];
                    if (minX < _minX) _minX = minX;

                    if (maxX > _maxX) _maxX = maxX;

                    if (minY < _minY) _minY = minY;

                    if (maxY > _maxY) _maxY = maxY;
                }

                var min = new int2(_minX, _minY);
                var max = new int2(_maxX, _maxY);
                return min + (max - min) / 2;
            }

            return new int2(int.MinValue, int.MinValue);
        }
    }


    public bool IsInArea(int2 coordinate)
    {
        if (specialAreas != null && specialAreas.Count > 0)
        {
            var gridCount = specialAreas.Count / 4;

            for (var i = 0; i < gridCount; i++)
            {
                var minX = specialAreas[i * 4];
                var minY = specialAreas[i * 4 + 1];

                var maxX = specialAreas[i * 4 + 2];
                var maxY = specialAreas[i * 4 + 3];
                if (coordinate.x >= minX && coordinate.x <= maxX && coordinate.y >= minY && coordinate.y <= maxY)
                    return true;
            }

            return false;
        }

        return true;
    }
}