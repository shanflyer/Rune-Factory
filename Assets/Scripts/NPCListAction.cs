using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NPCListAction : MonoBehaviour
{
    public Text YouhaoduText;
    public Text NameText;
    public Text friendlyLevel;
    public Slider firedlySlider;

    private NPCX npcx;
	// Use this for initialization
	void Start ()
	{
	    YouhaoduText.text = LanguageManage.SwitchStr(YouhaoduText.text);
	}

    public void InitData(NPCX _npcx)
    {
        npcx = _npcx;
        NameText.text =npcx.npcData.name;
        friendlyLevel.text = npcx.npcData.friendlyLevel.ToString();
        int fullExp = GameComponentData.gameData.NpcManager.zeroFriendUpExp + npcx.npcData.friendlyLevel *
                      GameComponentData.gameData.NpcManager.AddUpExp;
        firedlySlider.value = npcx.npcData.frienflyExp / (float) fullExp;

    }

    public void Clicked(Toggle toggle)
    {
        if (toggle)
        {
            GameComponentData.gameData.NpcListDataPanelAction.ClickedNpc(npcx);
        }
    }
	// Update is called once per frame
	void Update () {
		
	}
}
