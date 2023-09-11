public class WorldDataObj : CommonObj
{
    public WorldMapData data;
    public WorldDataObj() { }
    public WorldDataObj(WorldMapData data)
    {
        this.data = data;
    }

    public void ChecckSearch(string key, ObjSearchType objSearchType)
    {  
    }

    public bool CheckHide()
    {
        return false;
    }

    public CommonObj CreatNew(int count)
    {
      return  WorldMapEditor.Instance.CreatWorld();
    }

    public void Delete()
    {
        WorldMapEditor.Instance.DeleteWorld(this);
    }

    public void DrawTextureWithTexCoords(float posX, float posY, int texSize)
    {
    }

    public string GetId()
    {
        return data.GetKey();
    }

    public string GetName()
    {
        return data.name;
    }

    public bool GetSearch()
    {
        return true;
    }

    public void InitSearch()
    { 
    }

    public void NoSelectAction()
    { 
    }

    public void Save()
    {
    }

    public void SelectAction()
    {
        WorldMapEditor.Instance.SelectWorld(this);
    }

    public void SetHide()
    { 
    }

    public void SetNoHide()
    { 
    }
}
public class MapRoomDataObj : CommonObj
{
    public MapRoomData mapRoomData;

    public MapRoomDataObj()
    { }

    public MapRoomDataObj(MapRoomData mapRoomData)
    {
        this.mapRoomData = mapRoomData;
    }

    public void ChecckSearch(string key, ObjSearchType objSearchType)
    {
        //throw new System.NotImplementedException();
    }

    public bool CheckHide()
    {
        return false;
    }

    public CommonObj CreatNew(int count)
    {
        return MapEditor.Instance.CreatNewRoom();
    }

    public void Delete()
    {
        MapEditor.Instance.DeleteSelectRoom();
    }

    public void DrawTextureWithTexCoords(float posX, float posY, int texSize)
    {
        // throw new System.NotImplementedException();
    }

    public string GetId()
    {
        return mapRoomData.roomName;
    }

    public string GetName()
    {
        return mapRoomData.roomName;
    }

    public bool GetSearch()
    {
        return true;
    }

    public void InitSearch()
    {
        //throw new System.NotImplementedException();
    }

    public void NoSelectAction()
    {
    }

    public void Save()
    {
        MapEditor.Instance.SaveSelectMap();
    }

    public void SelectAction()
    {
        if (MapEditor.Instance != null)
        {
            MapEditor.Instance.SelectMapRoom(this);
        }
        if (WorldMapEditor.Instance != null)
        {
            WorldMapEditor.Instance.SelectMapRoom(this);
        }
    }

    public void SetHide()
    {
        // throw new System.NotImplementedException();
    }

    public void SetNoHide()
    {
        //throw new System.NotImplementedException();
    }
}

public class MapItemDataObj : CommonObj
{
    public MapItemData itemData;

    public MapItemDataObj()
    { }

    public MapItemDataObj(MapItemData mapItemData)
    {
        this.itemData = mapItemData;
    }

    public void ChecckSearch(string key, ObjSearchType objSearchType)
    {
        //throw new System.NotImplementedException();
    }

    public bool CheckHide()
    {
        return false;
    }

    public CommonObj CreatNew(int count)
    {
        throw new System.NotImplementedException();
    }

    public void Delete()
    {
        throw new System.NotImplementedException();
    }

    public void DrawTextureWithTexCoords(float posX, float posY, int texSize)
    {
    }

    public string GetId()
    {
        return itemData.id.ToString();
    }

    public string GetName()
    {
        return itemData.itemName;
    }

    public bool GetSearch()
    {
        return true;
    }

    public void InitSearch()
    {
    }

    public void NoSelectAction()
    {
    }

    public void Save()
    {
        throw new System.NotImplementedException();
    }

    public void SelectAction()
    {
        MapItemEditor.Instance.SelectMapItem(this);
    }

    public void SetHide()
    {
    }

    public void SetNoHide()
    {
    }
}

public class ExcelDataObj : CommonObj
{
    private string dataName;
    public string path;
    public ExcelDataObj() { }
    public ExcelDataObj(string dataName, string path)
    {
        this.dataName = dataName;
        this.path = path;
    }
    public void ChecckSearch(string key, ObjSearchType objSearchType)
    { 
    }

    public bool CheckHide()
    {
        return false;
    }

    public CommonObj CreatNew(int count)
    {
        throw new System.NotImplementedException();
    }

    public void Delete()
    { 
    }

    public void DrawTextureWithTexCoords(float posX, float posY, int texSize)
    { 
    }

    public string GetId()
    {
        return dataName;
    }

    public string GetName()
    {
        return dataName;
    }

    public bool GetSearch()
    {
        return true;
    }

    public void InitSearch()
    { 
    }

    public void NoSelectAction()
    {
        ExcelDataEditor.Instance.RemoveExcelDataObj(this);
    }

    public void Save()
    { 
    }

    public void SelectAction()
    {
        ExcelDataEditor.Instance.AddExcelDataObj(this);
    }

    public void SetHide()
    { 
    }

    public void SetNoHide()
    { 
    }
}