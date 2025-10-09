using System;
using System.Collections.Generic;
using System.IO;
using OfficeOpenXml;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

public partial class CommonToolEditor
{
    public class MapRoomPointEditor
    {
        public int mapId;
        public List<link> neighbours;
    }

    [Serializable]
    public struct RoomLinks
    {
        public int startMap, endMap;
        public List<link> links;
    }

    [Serializable]
    public struct link
    {
        public int2 start;
        public int2 end;
        public int map;
    }

    private int2 GetGridsCenter(List<int> girds)
    {
        var minX = int.MaxValue;
        var minY = int.MaxValue;
        var maxX = int.MinValue;
        var maxY = int.MinValue;

        var gridCount = girds.Count / 4;
        for (var j = 0; j < gridCount; j++)
        {
            var x = girds[j * 4];
            var y = girds[j * 4 + 1];
            var x1 = girds[j * 4 + 2];
            var y1 = girds[j * 4 + 3];

            minX = minX < x ? minX : x;
            minY = minY < y ? minY : y;
            maxX = maxX > x ? maxX : x;
            maxY = maxY > y ? maxY : y;
        }

        return new int2(minX, minY) + new int2(maxX - minX, maxY - minY) / 2;
    }

    public void CheckWorldMapLink()
    {
        var mapPath = "Assets/Resources/Data/MapRoomData";
        var directoryInfo = new DirectoryInfo(mapPath);
        var files = directoryInfo.GetFiles("*.asset");
        var mapRangeDic = new Dictionary<string, int4>();
        foreach (var file in files)
        {
            var filePath = $"{mapPath}/{file.Name}";
            var mapData = AssetDatabase.LoadAssetAtPath<MapRoomData>(filePath);
            mapRangeDic[mapData.roomName] = new int4(mapData.startCoordinate.xy, mapData.endCoordinate.xy);
        }

        var worldDataPath = "Assets/Resources/Data/WorldMapData/测试.asset";
        var worldMapData = AssetDatabase.LoadAssetAtPath<WorldMapData>(worldDataPath);
        var lines = worldMapData.mapLines;
        var mapNameDic = new Dictionary<int, string>();
        foreach (var worldMap in worldMapData.worldMapDic)
            mapNameDic[worldMap.Key] = worldMap.Value.mapRoomData.roomName;


        for (var i = 0; i < lines.Count; i++)
        {
            var line = lines[i];
            if (mapNameDic.TryGetValue(line.Map0, out var mapName))
                if (mapRangeDic.TryGetValue(mapName, out var mapRange))
                {
                    var range = GameCommon.GridRange(line.cells0.girds);
                    if (range.x < mapRange.x || range.y < mapRange.y || range.z > mapRange.z || range.w > mapRange.w)
                        Debug.Log($"line:{line.instanceId}--超出范围！");
                }
        }
    }
    public void OutWorldMapLink()
    {
        var worldDataPath = "Assets/Resources/Data/WorldMapData/测试.asset";
        var worldMapData = AssetDatabase.LoadAssetAtPath<WorldMapData>(worldDataPath);
        var lines = worldMapData.mapLines;

        var mapRoomPointDic = new MyDic<int, MapRoomPointEditor>();

        var mapDic = new MyDic<int2, int2>();
        for (var i = 0; i < lines.Count; i++)
        {
            var line = lines[i];
            var key = new int2(line.Map0, line.Map1);
            if (mapDic.ContainsKey(key)) continue;
            mapDic.Add(key, new int2(line.center1));

            if (!mapRoomPointDic.TryGetValue(line.Map0, out var mapRoomPoint))
            {
                mapRoomPoint = new MapRoomPointEditor
                {
                    mapId = line.Map0,
                    neighbours = new List<link>()
                };
                mapRoomPointDic.Add(line.Map0, mapRoomPoint);
            }

            mapRoomPoint.neighbours.Add(new link
            {
                start = GetGridsCenter(line.cells0.girds),
                end = line.center1,
                map = line.Map1
            });

            var key1 = new int2(line.Map1, line.Map0);
            mapDic.Add(key1, new int2(line.center0));


            if (!mapRoomPointDic.TryGetValue(line.Map1, out var mapRoomPoint1))
            {
                mapRoomPoint1 = new MapRoomPointEditor
                {
                    mapId = line.Map1,

                    neighbours = new List<link>()
                };
                mapRoomPointDic.Add(line.Map1, mapRoomPoint1);
            }

            mapRoomPoint1.neighbours.Add(new link
            {
                start = GetGridsCenter(line.cells1.girds),
                end = line.center0,
                map = line.Map0
            });
        }

        var roomLinks = new List<RoomLinks>();
        for (var i = 0; i < mapRoomPointDic.length - 1; i++)
        {
            var startMap = mapRoomPointDic[i].mapId;
            for (var j = i + 1; j < mapRoomPointDic.length; j++)
            {
                var endMap = mapRoomPointDic[j].mapId;
                var mapLinks = CreateMapLinks(startMap, endMap);

                var roomLink = new RoomLinks
                {
                    startMap = startMap,
                    endMap = endMap,
                    links = new List<link>()
                };
                var keyX = startMap;
                for (var k = 0; k < mapLinks.Count; k++) roomLink.links.Add(mapLinks[k]);

                roomLinks.Add(roomLink);
            }
        }

        SaveExcel(roomLinks);
        /*
        var str = JsonConvert.SerializeObject(roomLinks);
        File.WriteAllText("mapLink.json", str);*/

        List<link> CreateMapLinks(int startMap, int endMap)
        {
            var checkMap = startMap;
            var links = new Stack<link>();
            var getLink = false;
            var linkMaps = new Dictionary<int, int>();

            var neighbours = new Queue<link>();
            var parentDic = new Dictionary<link, link>();
            var checkMapSet = new HashSet<int>();
            GetLink(checkMap);

            bool GetLink(int checkMap)
            {
                checkMapSet.Add(checkMap);
                if (mapRoomPointDic.TryGetValue(checkMap, out var mapRoomPoint))
                {
                    for (var i = 0; i < mapRoomPoint.neighbours.Count; i++)
                        neighbours.Enqueue(mapRoomPoint.neighbours[i]);

                    while (neighbours.Count > 0)
                    {
                        var neighbour = neighbours.Dequeue();
                        if (linkMaps.ContainsKey(neighbour.map)) continue;
                        var link = neighbour;
                        linkMaps.Add(neighbour.map, checkMap);

                        if (neighbour.map == endMap)
                        {
                            getLink = true;
                            links.Push(neighbour);
                            while (parentDic.TryGetValue(neighbour, out var parent))
                            {
                                links.Push(parent);
                                neighbour = parent;
                            }

                            return true;
                        }


                        if (mapRoomPointDic.TryGetValue(neighbour.map, out mapRoomPoint))
                            for (var i = 0; i < mapRoomPoint.neighbours.Count; i++)
                                if (!checkMapSet.Contains(mapRoomPoint.neighbours[i].map))
                                {
                                    parentDic.Add(mapRoomPoint.neighbours[i], neighbour);
                                    neighbours.Enqueue(mapRoomPoint.neighbours[i]);
                                }

                        checkMapSet.Add(neighbour.map);
                    }


                    if (!getLink)
                    {
                    }
                }


                return false;
            }


            var maps = new List<link>();
            while (links.Count > 0) maps.Add(links.Pop());

            return maps;
        }


        void SaveExcel(List<RoomLinks> data)
        {
            var file = new FileInfo(sourcePath);
            /*if (!File.Exists(excelPath))
            {
            }
            else
            {
            }*/

            using (var package = new ExcelPackage(file))
            {
                ExcelWorksheet worksheet;
                try
                {
                    worksheet = package.Workbook.Worksheets["Sheet1"];
                    var cells = worksheet.Cells;
                    cells.Clear();
                }
                catch
                {
                    worksheet = package.Workbook.Worksheets.Add("Sheet1");
                }

                worksheet.Cells[1, 2].Value = "startMap";
                worksheet.Cells[1, 3].Value = "EndMap";
                for (var i = 0; i < data.Count; i++)
                {
                    var linkData = data[i];
                    worksheet.Cells[i + 2, 2].Value = linkData.startMap;
                    worksheet.Cells[i + 2, 3].Value = linkData.endMap;
                    for (var j = 0; j < linkData.links.Count; j++)
                    {
                        var link = linkData.links[j];
                        worksheet.Cells[i + 2, 4 + j].Value =
                            $"{link.start.x},{link.start.y}|{link.end.x},{link.end.y}|{link.map}";
                    }
                }

                package.Save();
            }

            /*
            FileInfo file = new FileInfo(excelPath);
            using (ExcelPackage package = new ExcelPackage(file))
            {
                ExcelWorksheet excelWorksheet = package.Workbook.Worksheets["PVP"];

                package.Save();
            }*/
        }
    }
}