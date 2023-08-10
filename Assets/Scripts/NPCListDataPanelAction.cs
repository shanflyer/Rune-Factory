using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NPCListDataPanelAction : MonoBehaviour
{
    public List<Text> Texts;
    public GameObject npcListpro;
    public Transform npcListparent;
    public Image NpcImage;
    public Text NpcNameText, NpcLevelText;
    public Text HpText, AtText, DfText;
    public Text NoticeText;
    public Text StatusText, FriendlyValueText;
    public Text FrientLevelText;

    public Text AttribuText;
    public Button finderButton;

    private NPCX selectNPC;

    private List<GameObject> npcObjects;
	// Use this for initialization
	void Start () {
	    foreach (var text in Texts)
	    {
	        LanguageManage.TextFanyi(text);
	    }
	}

    public void ClickVisitButton()
    {
        GameComponentData.gameData.NpcManager.VisitNPC(selectNPC);
    }
    public void InitNpcListData()
    {
        if (npcObjects == null)
        {
            npcObjects=new List<GameObject>();
        }
        else
        {
            foreach (var npcObject in npcObjects)
            {
                Destroy(npcObject);
            }
            npcObjects.Clear();
        }
        finderButton.gameObject.SetActive(false);
        List<NPCX> npcs = GameComponentData.gameData.NpcManager.Npcxs;
        foreach (var npc in npcs)
        {
            GameObject npcListObj = Instantiate(npcListpro);
            npcListObj.GetComponent<NPCListAction>().InitData(npc);
            npcListObj.transform.SetParent(npcListparent,false);
            npcListObj.GetComponentInChildren<Toggle>().group = npcListparent.GetComponent<ToggleGroup>();
            npcObjects.Add(npcListObj);
        }
        npcObjects[0].GetComponentInChildren<Toggle>().isOn = true;
    }

    public void ClickedNpc(NPCX npcx)
    {
        selectNPC = npcx;
        NpcImage.sprite = GameComponent.charactorIcon.Find(c => c.name == npcx.npcData.ImageName);
        NpcLevelText.text = "Lv." + selectNPC.level;
        NpcNameText.text = selectNPC.Name;
        AtText.text = selectNPC.property.AT.ToString();
        DfText.text = selectNPC.property.DF.ToString();
        HpText.text = selectNPC.property.MaxHP.ToString();
        AttribuText.text = LanguageManage.SwitchStr("属性:") + LanguageManage.SwitchStr(selectNPC.npcData.attributeType.ToString());
        NoticeText.text = selectNPC.npcData.NoticeText;
        StatusText.text = LanguageManage.SwitchStr(selectNPC.npcData.npcStatus.ToString());
        FriendlyValueText.text = selectNPC.npcData.friendlyLevel.ToString();
        finderButton.gameObject.SetActive(true);
        if (selectNPC.npcData.friendlyLevel >= 10)
        {
            FrientLevelText.text = LanguageManage.SwitchStr("亲密");
        }
        else if (selectNPC.npcData.friendlyLevel >= 7)
        {
            FrientLevelText.text = LanguageManage.SwitchStr("友爱");
        }
        else if (selectNPC.npcData.friendlyLevel >= 4)
        {
            FrientLevelText.text = LanguageManage.SwitchStr("要好");
        }
        else if (selectNPC.npcData.friendlyLevel >= 2)
        {
            FrientLevelText.text = LanguageManage.SwitchStr("熟悉");
        }
        else
        {
            FrientLevelText.text = LanguageManage.SwitchStr("陌生");
        }

    }
	// Update is called once per frame
	void Update () {
		
	}
}
