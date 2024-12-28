using System.Threading.Tasks;
using UnityEngine;

public class FilmPanel : GamePanel<IReferenceData>
{
    public override Task InitData(string dataKey)
    {
        HideAllPanel hidePanels = new HideAllPanel
        {
            hide = true, 
        };
        GameActionManager.instance.QueueAction(hidePanels,true);

       // UIManager.instance.CloseGamePanel<PlayerTopPanel>();
       // UIManager.instance.CloseGamePanel<MainPanel>();
       // UIManager.instance.CloseGamePanel<ScreenControllerPanel>();
        return base.InitData(dataKey);
    }
    public override void Close()
    {
        if (!SingletonType.Cleared)
        {
            HideAllPanel hidePanels = new HideAllPanel
            {
                hide = false,
            };
            GameActionManager.instance.QueueAction(hidePanels);
        }
        
        base.Close();
    }
}
