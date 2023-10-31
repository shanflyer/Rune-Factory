using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public struct GameSaveData:IReferenceData
{
    public string saveTime;
    public PlayerSaveData playerSaveData;
    public DateData dateData;

    public int money0;
    public bool IsNull()
    {
        return string.IsNullOrEmpty(saveTime);
    }
}

public struct PlayerSaveData
{
    public string name;
    public Gender gender;
    public int level; 
}
public struct DateData
{
    public Season season;
    public int date;
    public int year;
}