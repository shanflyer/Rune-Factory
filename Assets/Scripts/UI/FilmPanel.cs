using System.Threading.Tasks;
using UnityEngine;

public class FilmPanel : GamePanel<IReferenceData>
{
    public override Task InitData(string dataKey)
    {
        UIManager.instance.CloseGamePanel<PlayerTopPanel>();
        UIManager.instance.CloseGamePanel<MainPanel>();
        return base.InitData(dataKey);
    }
    public override void Close()
    {
        UIManager.instance.ShowGamePanel<PlayerTopPanel>();
        UIManager.instance.ShowGamePanel<MainPanel>();
        base.Close();
    }
}
