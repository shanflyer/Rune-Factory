using System.Collections.Generic;
using UnityEngine;

public class InformationController : Singleton<InformationController>
{
    string[] informations = new string[200];
    Queue<string> informationsQueue = new Queue<string>();
    int nowIndex = 0;
    public override bool NeedUpdate => true;


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
    PromptPanel PromptPanel
    {
        get
        {
            if (_PromptPanel == null)
            {
                AsyncTaskRunner.Run(GetPromptPanelAsync, nameof(GetPromptPanelAsync));
            }
            return _PromptPanel;
        }
    }

    private PromptPanel _PromptPanel;

    private async System.Threading.Tasks.Task GetPromptPanelAsync()
    {
        _PromptPanel = await UIManager.instance.GetGamePanel<PromptPanel>();
    }

    private float showTime;
    private bool updateRunning;

    protected override void Update()
    {
        if (updateRunning)
        {
            return;
        }

        AsyncTaskRunner.Run(UpdateAsync, nameof(InformationController.Update));
    }

    private async System.Threading.Tasks.Task UpdateAsync()
    {
        updateRunning = true;
        try
        {
            base.Update();
            if (showTime <= 0)
            {
                if (informationsQueue.Count > 0)
                {
                    var information = informationsQueue.Dequeue();
                    if (InformationShowPanel == null)
                        InformationShowPanel = await UIManager.instance.GetGamePanel<InformationShowPanel>(true);
                    InformationShowPanel.SetInfo(information);
                    InformationShowPanel.Show();
                    showTime = 2.0f;
                }
                else
                {
                    if (InformationShowPanel != null) InformationShowPanel.Close();
                }
            }
            else
            {
                showTime -= Time.deltaTime;
                if (showTime < 0) showTime = 0;
            }
        }
        finally
        {
            // Update 由每帧触发，异步等待 UI 时必须防止重复进入同一轮刷新。
            updateRunning = false;
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
        }
        if (PromptShow&& PromptPanel!=null)
        {
            PromptPanel.Show();
            PromptPanel.InitData(information);
            GameTimerController.instance.DelayAction((int)(GameCommon.PromptTime*1000), ClosePromptPanel);
        }
    }
    void ClosePromptPanel()
    {
        if (PromptPanel != null)
        {
            PromptPanel.Close();
        }
    }
}
