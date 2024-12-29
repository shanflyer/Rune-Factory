using System.Threading.Tasks;
using UnityEngine;

public class FilmPanel : GamePanel<IReferenceData>
{
    public override Task InitData(string dataKey)
    { 
        return base.InitData(dataKey);
    }
    public override void Close()
    { 
        base.Close();
    }
}
