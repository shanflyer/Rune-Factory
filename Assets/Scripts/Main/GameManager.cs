using System.Collections;
using UnityEngine;
using System;

public class GameManager : Singleton<GameManager>
{
    public void ShowTwoSelectAction(string title, string notice, Action yesAction, Action noAction)
    {
        TwoSelectData twoSelectData = new TwoSelectData
        {
            title = title,
            notice = notice,
            yesAction = yesAction,
            noAction = noAction
        };
        UIManager.instance.ShowGamePanel<TwoSelectPanel,TwoSelectData>(twoSelectData);
    }
}