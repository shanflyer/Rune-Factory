using OldName;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum EmployType
{
    佣兵=0,
    NPC=1,
    动物=2
}
public class CharactorShop : MonoBehaviour
{
    public List<Text> Texts;
    public Toggle NpcToggle;
    public Text notice;
    public Text MoneyText;
    public Text totalMoneyText;
    public GameObject employPro;
    public Transform employParent;

    public Button employButton;

    private List<GameObject> emplorObjs;

    private Employer selectedEmployer;

    private GamePlayer gamePlayer;

    private EmployType employType;
    // Use this for initialization
    void Start ()
    {
        gamePlayer = GameComponentData.gameData.gameManager.gamePlayer;
        foreach (var text in Texts)
        {
            LanguageManage.TextFanyi(text);
        }
    }

    public void ClickReturn()
    {
        AudioController.instance.PlayAudio(SE.Return);
        gameObject.SetActive(false);
    }
    public void BuyEmployAction()
    {
        AudioController.instance.PlayAudio(SE.click);
        if (gamePlayer.TeamPlayer0 == null ||gamePlayer.TeamPlayer0.id== 0 || gamePlayer.TeamPlayer1 == null || gamePlayer.TeamPlayer1.id==0)
        {
            if (employType == EmployType.NPC)
            {
                 
            }
            else if (GameComponentData.gameData.gameManager.InitCostData(selectedEmployer.cost,ShopMoneyType.金币,  CostType.购买佣兵))
            {
                MoneyText.text = gamePlayer.money.ToString();
                TeamPlayer teamPlayer = new TeamPlayer(selectedEmployer);
                if (gamePlayer.TeamPlayer0 == null || gamePlayer.TeamPlayer0.name==null)
                {
                    gamePlayer.TeamPlayer0 = teamPlayer;
                }
                else
                {
                    gamePlayer.TeamPlayer1 = teamPlayer;
                }
               // GameComponentData.gameData.intelligencePanelAction.InitIntelligenceData();
                if (GameComponentData.gameData.adventurePanelAction.gameObject.activeSelf)
                {
                    GameComponentData.gameData.adventurePanelAction.InitData();
                }
                SwitchEmployType((int)employType);

                GameNotificationManager.instance.DisplayTips(LanguageManage.SwitchStr("雇佣成功"),LanguageManage.SwitchStr("成功雇佣到队友:")+teamPlayer.name);
            }
            else
            {
                GameNotificationManager.instance.DisplayTips(LanguageManage.SwitchStr("提示"), 
                    LanguageManage.SwitchStr("金币不足，无法雇佣"));
            }
        }
        else
        {
            GameNotificationManager.instance.DisplayTips(LanguageManage.SwitchStr("提示"),
                LanguageManage.SwitchStr("队伍已满，无法雇佣"));
        }
      
    }
    public void Selected(Employer _employer)
    {
        if (gamePlayer == null)
        {
            gamePlayer = GameComponentData.gameData.gameManager.gamePlayer;
        }
        AudioController.instance.PlayAudio(SE.select); 
        selectedEmployer = _employer;
        totalMoneyText.text =LanguageManage.SwitchStr("佣金:")+ _employer.cost.ToString();
        if (_employer.isHired)
        {
            employButton.interactable = false;
        }
        else
        {
            employButton.interactable = true;
        }
        
        employButton.GetComponentInChildren<Text>().text = LanguageManage.SwitchStr("雇佣");
        if (employType == EmployType.NPC)
        { 
        }
        if (employType != EmployType.佣兵)
        {
            if ((gamePlayer.TeamPlayer0 != null&&gamePlayer.TeamPlayer0.id != 0 && gamePlayer.TeamPlayer0.id == selectedEmployer.id)|| 
                (gamePlayer.TeamPlayer1 != null&&gamePlayer.TeamPlayer1.id != 0 && gamePlayer.TeamPlayer1.id == selectedEmployer.id))
            {
                employButton.interactable = false;
                employButton.GetComponentInChildren<Text>().text = LanguageManage.SwitchStr("已雇佣");
            }
        }
    }
    public void SwitchEmployType()
    {
        employButton.interactable = false;
        MoneyText.text = GameComponentData.gameData.gameManager.gamePlayer.money.ToString();
        AudioController.instance.PlayAudio(SE.select);
        if (emplorObjs == null)
        {
            emplorObjs=new List<GameObject>();
        }
        else
        {
            foreach (var emplorObj in emplorObjs)
            {
                Destroy(emplorObj);
            }
            emplorObjs.Clear();
        }
        switch (employType)
        {
            case EmployType.佣兵:
                List<Employer> employers = GameComponentData.gameData.employerManger.Employers;
              // List<BatteleMap> batteleMaps = GameComponentData.gameData.BattleMapAction.BatteleMaps;
                foreach (var employer in employers)
                {
                    /*
                    if (batteleMaps.Exists(b => b.id == employer.openBattleId && b.isOpen))
                    {
                        GameObject emplorobj = Instantiate(employPro);
                        emplorobj.transform.SetParent(employParent,true);
                        emplorobj.GetComponentInChildren<Toggle>().group =
                            employParent.GetComponentInChildren<ToggleGroup>();
                        emplorobj.GetComponent<EmplorPanelAction>().InitEmplorData(employer);
                        emplorobj.transform.localScale=Vector3.one;
                        emplorObjs.Add(emplorobj);
                    }
                    */
                    
                }
                notice.text = LanguageManage.SwitchStr("佣兵等级固定，无法进行成长");
                break;
            case EmployType.动物:
                
                List<Pasture> pastures = GameComponentData.gameData.pastureAction.Pastures;
                List<Animal> animals=new List<Animal>();
                foreach (var pasture in pastures)
                {
                    if (pasture.Animals != null)
                    {
                        animals.AddRange(pasture.Animals);
                    }
                    
                }
                foreach (var animal in animals)
                {
                    
                    Employer employer=new Employer(animal);
                    GameObject emplorobj = Instantiate(employPro);
                    emplorobj.transform.SetParent(employParent, true);
                    emplorobj.GetComponentInChildren<Toggle>().group =
                        employParent.GetComponentInChildren<ToggleGroup>();
                    emplorobj.GetComponent<EmplorPanelAction>().InitEmplorData(employer);
                    emplorObjs.Add(emplorobj);
                    emplorobj.transform.localScale = Vector3.one;
                }

                notice.text = LanguageManage.SwitchStr("动物无法装备武器和防具");
                break;
            case EmployType.NPC:
               

                notice.text = LanguageManage.SwitchStr("NPC的友好度越高佣金越低");
                break;
        }
        if (emplorObjs.Count > 0)
        {
            emplorObjs[0].GetComponentInChildren<Toggle>().isOn = true;
            Selected(emplorObjs[0].GetComponent<EmplorPanelAction>().employer);
        }
        totalMoneyText.text = "";
        
    }
    public void SwitchEmployType(int index)
    {
       
        employButton.interactable = false;
        MoneyText.text = GameComponentData.gameData.gameManager.gamePlayer.money.ToString();
        AudioController.instance.PlayAudio(SE.select);
        employType = (EmployType)index;
        if (emplorObjs == null)
        {
            emplorObjs = new List<GameObject>();
        }
        else
        {
            foreach (var emplorObj in emplorObjs)
            {
                Destroy(emplorObj);
            }
            emplorObjs.Clear();
        }
        switch (employType)
        {
            case EmployType.佣兵:
                List<Employer> employers = GameComponentData.gameData.employerManger.Employers;
                /*List<BatteleMap> batteleMaps = GameComponentData.gameData.BattleMapAction.BatteleMaps;
                foreach (var employer in employers)
                {
                    if (batteleMaps.Exists(b => b.id == employer.openBattleId && b.isOpen))
                    {
                        GameObject emplorobj = Instantiate(employPro);
                        emplorobj.transform.SetParent(employParent, true);
                        emplorobj.transform.localScale = Vector3.one;
                        emplorobj.GetComponentInChildren<Toggle>().group =
                            employParent.GetComponentInChildren<ToggleGroup>();
                        emplorobj.GetComponent<EmplorPanelAction>().InitEmplorData(employer);
                        emplorObjs.Add(emplorobj);
                    }

                }*/
                notice.text = "佣兵等级固定，无法进行成长";
                break;
            case EmployType.动物:

                var emploers =
                    GameComponentData.gameData.employerManger.Employers.FindAll(e => e.employType == EmployType.动物);
                foreach (var employer in emploers)
                {
                    GameObject emplorobj = Instantiate(employPro);
                    emplorobj.transform.SetParent(employParent, true);
                    emplorobj.transform.localScale=Vector3.one;
                    emplorobj.GetComponentInChildren<Toggle>().group =
                        employParent.GetComponentInChildren<ToggleGroup>();
                    emplorobj.GetComponent<EmplorPanelAction>().InitEmplorData(employer);
                    emplorObjs.Add(emplorobj);
                }

                notice.text = LanguageManage.SwitchStr("动物无法装备武器和防具");
                break;
            case EmployType.NPC:
                var emploers1 =
                    GameComponentData.gameData.employerManger.Employers.FindAll(e => e.employType == EmployType.NPC);
                foreach (var employer in emploers1)
                {
                    GameObject emplorobj = Instantiate(employPro);
                    emplorobj.transform.SetParent(employParent, true);
                    emplorobj.transform.localScale = Vector3.one;
                    emplorobj.GetComponentInChildren<Toggle>().group =
                        employParent.GetComponentInChildren<ToggleGroup>();
                    emplorobj.GetComponent<EmplorPanelAction>().InitEmplorData(employer);
                    emplorObjs.Add(emplorobj);
                }

                notice.text = LanguageManage.SwitchStr("NPC的友好度越高佣金越低");
                break;
        }
        if (emplorObjs.Count > 0)
        {
            emplorObjs[0].GetComponentInChildren<Toggle>().isOn = true;
            Selected(emplorObjs[0].GetComponent<EmplorPanelAction>().employer);
        }
        totalMoneyText.text = "";

    }
    // Update is called once per frame
    void Update () {
		
	}
}
