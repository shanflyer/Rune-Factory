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

    PromptPanel _PromptPanel;
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
                 UIManager.instance.GetGamePanel<InformationShowPanel>(SetPanel: InformationShowPanel =>
                {
                    this.InformationShowPanel = InformationShowPanel;
                    InformationShowPanel.SetInfo(information);
                    InformationShowPanel.Show();
                });
            }
            
        }
        else
        {
            nowShow = false;
            if(InformationShowPanel != null)
            {
                InformationShowPanel.Close();
            }
        }       
    }
    public void AddInformation(string information,bool Show = true,bool PromptShow=false)
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
        if (PromptShow&& _PromptPanel!=null)
        {
            _PromptPanel.Show();
            _PromptPanel.InitData(information);
            GameTimerController.instance.DelayAction((int)(GameCommon.PromptTime*1000), ClosePromptPanel);
        }
        else
        {
            UIManager.instance.GetGamePanel<PromptPanel>(SetPanel:panel=>{
                _PromptPanel = panel;
                _PromptPanel.Show();
                _PromptPanel.InitData(information);
                GameTimerController.instance.DelayAction((int)(GameCommon.PromptTime * 1000), ClosePromptPanel);
            });
        }
    }
    void ClosePromptPanel()
    {
        if (_PromptPanel != null)
        {
            _PromptPanel.Close();
        }
    }
}
