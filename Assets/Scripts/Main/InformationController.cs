using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InformationController : Singleton<InformationController>
{
    string[] informations = new string[200];
    Queue<string> informationsQueue = new Queue<string>();
    int nowIndex = 0;
    public override void Init()
    {
        informations = new string[200];
        informationsQueue.Clear();
        base.Init();
    }
    protected override void Clear()
    {
        informations = new string[200];
        informationsQueue.Clear();
        base.Clear();
    }

    InformationShowPanel InformationShowPanel;
    bool nowShow = false;
    void ShowInformation()
    { 
        if(informationsQueue.Count>0)
        {
            nowShow = true;
            var information = informationsQueue.Dequeue();
            GameTimerController.instance.DelayAction(2000, ShowInformation);

            if (InformationShowPanel == null)
            {
                InformationShowPanel = UIManager.instance.GetGamePanel<InformationShowPanel>();
            }
            InformationShowPanel.SetInfo(information);
        }
        else
        {
            nowShow = false;
        }       
    }
    public void AddInformation(string information,bool Show = true)
    {
        if (nowIndex >= 200)
        {
            nowIndex = 0;
            
        }
        informations[nowIndex]=information;
        nowIndex++;

        if (Show)
        {
            informationsQueue.Enqueue(information);
            if (!nowShow)
            {
                ShowInformation();
            }
        }
        InformationPanel informationPanel = UIManager.instance.GetGamePanel<InformationPanel>();
        if (informationPanel)
        {
            informationPanel.AddInfo(information);
        }
    }

}
