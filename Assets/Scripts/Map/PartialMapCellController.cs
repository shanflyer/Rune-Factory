using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;

public partial class MapCellController
{
    private readonly Dictionary<int, MapCharacterGrid> MapCharacterGrids = new();

    public HashSet<int2> GetMapCharacterCells(int mapInstance)
    {
        if (tempMaps.TryGetValue(mapInstance, out var trueMap)) mapInstance = trueMap;
        var result = new HashSet<int2>();
        if (MapCharacterGrids.TryGetValue(mapInstance, out var mapCharacterGrid))
            for (var i = 0; i < mapCharacterGrid.characters.Length; i++)
                result.Add(mapCharacterGrid.characters[i].xy);

        return result;
    }

    public void SetCharacterCoordinate(int3 oldCoordinate, int3 newCoordinate, int characterId, bool isTemp)
    {
        if (tempMaps.TryGetValue(oldCoordinate.z, out var trueMap)) oldCoordinate.z = trueMap;
        if (tempMaps.TryGetValue(newCoordinate.z, out trueMap)) newCoordinate.z = trueMap;
        if (newCoordinate.z != oldCoordinate.z)
            if (MapCharacterGrids.TryGetValue(oldCoordinate.z, out var oldMapCharacterGrid))
                oldMapCharacterGrid.RemoveCharacter(characterId, isTemp);

        if (!MapCharacterGrids.TryGetValue(newCoordinate.z, out var mapCharacterGrid))
        {
            mapCharacterGrid = new MapCharacterGrid
            {
                mapInstance = newCoordinate.z
            };
            MapCharacterGrids.Add(newCoordinate.z, mapCharacterGrid);
        }

        mapCharacterGrid.ChangeCharacter(characterId, newCoordinate.xy, isTemp);
    }

    public void RemoveCharacterCoordinate(int3 coordinate, int characterId, bool isTemp)
    {
        if (tempMaps.TryGetValue(coordinate.z, out var trueMap)) coordinate.z = trueMap;
        if (MapCharacterGrids.TryGetValue(coordinate.z, out var mapCharacterGrid))
            mapCharacterGrid.RemoveCharacter(characterId, isTemp);
    }

    public HashSet<int> GetCharacters(int3 coordinate, int range = 8)
    {
        if (tempMaps.TryGetValue(coordinate.z, out var trueMap)) coordinate.z = trueMap;
        if (MapCharacterGrids.TryGetValue(coordinate.z, out var mapCharacterGrid))
        {
            var result = new HashSet<int>();
            for (var index = 0; index < mapCharacterGrid.characters.Length; index++)
            {
                var target = mapCharacterGrid.characters[index];
                var minX = target.x - range;
                var minY = target.y - range;
                var maxX = target.x + range;
                var maxY = target.y + range;
                if (minX <= coordinate.x && minY <= coordinate.y && maxX > coordinate.x && maxY > coordinate.y)
                    result.Add(target.z);
            }

            return result;
        }

        return new HashSet<int>();
    }

    public int GetClickCharacter(int3 coordinate, int range = 5)
    {
        if (tempMaps.TryGetValue(coordinate.z, out var trueMap)) coordinate.z = trueMap;
        if (MapCharacterGrids.TryGetValue(coordinate.z, out var mapCharacterGrid))
            for (var index = 0; index < mapCharacterGrid.characters.Length; index++)
            {
                var target = mapCharacterGrid.characters[index];
                var minX = target.x - range;
                var minY = target.y - range;
                var maxX = target.x + range;
                var maxY = target.y + range;
                if (minX <= coordinate.x && minY <= coordinate.y && maxX > coordinate.x && maxY > coordinate.y)
                    return target.z;
            }

        return -1;
    }

    private void RemoveCellCharacter(RemoveCellCharacter removeCellCharacter)
    {
        if (removeCellCharacter.cell.Equals(int3.zero))
        {
            var character = CharacterManager.instance.GetCharacter(removeCellCharacter.characterId);
            RemoveCharacterCoordinate(character.ObjCoordinate, removeCellCharacter.characterId,
                removeCellCharacter.isTemp);
        }
        else
        {
            RemoveCharacterCoordinate(removeCellCharacter.cell, removeCellCharacter.characterId,
                removeCellCharacter.isTemp);
        }
    }
}

public class MapCharacterGrid
{
    public int mapInstance;
    public NativeList<int3> characters;
    public Dictionary<int, int> characterIndexs;

    public void RemoveCharacter(int characterInstance, bool isTemp)
    {
        if (isTemp)
        {
        }
        else
        {
            if (characterIndexs.TryGetValue(characterInstance, out var index))
            {
                if (index != characters.Length - 1)
                {
                    var lastCharacter = characters[characters.Length - 1];
                    characters[index] = lastCharacter;
                    characterIndexs[lastCharacter.z] = index;
                }

                characterIndexs.Remove(characterInstance);
                characters.RemoveAt(characters.Length - 1);
            }
        }
    }

    public void ChangeCharacter(int characterInstance, int2 coordinate, bool isTemp)
    {
        if (isTemp)
        {
        }
        else
        {
            if (!characterIndexs.TryGetValue(characterInstance, out var index))
            {
                characterIndexs.Add(characterInstance, characters.Length);
                characters.Add(new int3(coordinate.xy, characterInstance));
            }
            else
            {
                characters[index] = new int3(coordinate.xy, characterInstance);
            }
        }
    }

    public MapCharacterGrid()
    {
        characters = new NativeList<int3>(16, Allocator.Persistent);
        characterIndexs = new Dictionary<int, int>();
    }

    public void Dispose()
    {
        characters.Dispose();
    }
}