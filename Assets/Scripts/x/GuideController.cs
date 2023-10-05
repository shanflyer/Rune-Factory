using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using LitJson;
using System.IO;

[System.Serializable]
public enum GuideTriggerType
{
    游戏开始=0,
    睡眠结束=1,
    进入地图=2,
    开始营业=3,
    tips完成=4,
    电影结束=5,
    出现金币=6,
    制作饮食完成=7,
    游戏开始分辨率 = 8,
    对话完成=9
}
[System.Serializable]
public enum StepType
{
    箭头指引=1,
    对话=2
}
[System.Serializable]
public class GuideMask
{
    public int posX,posY;
    public int scaleValueX, scaleValueY;
    public GuideMask() { }

    public GuideMask(GameObject maskObj)
    {
        posX = (int) (maskObj.transform.position.x * 1000);
        posY = (int) (maskObj.transform.position.y * 1000);
        scaleValueX = (int) (maskObj.transform.GetChild(1).localScale.x * 1000);
        scaleValueY = (int) (maskObj.transform.GetChild(1).localScale.y * 1000);
        
    }
}
[System.Serializable]
public class GuideStep
{
    public bool stopTime;
    public StepType stepType;
    public string stepValue;
    public string stepValue1;

    public void SetStepValueForGuideMask(GuideMask guideMask)
    {
        stepValue = guideMask.posX + "," + guideMask.posY + ";" + guideMask.scaleValueX + "," + guideMask.scaleValueY;
    }
    public void SetStepValueForGuideMask2(GuideMask guideMask)
    {
        stepValue1 = guideMask.posX + "," + guideMask.posY + ";" + guideMask.scaleValueX + "," + guideMask.scaleValueY;
    }
}
[System.Serializable]
public class Guide
{
    public string name;
    public int id;
    public int frontId;
    public GuideTriggerType guideTriggerType;
    public string guideTriggerValue;
    public List<GuideStep> guideSteps;
    public bool end;
    public bool CheckTriggers()
    {
        if (!end)
        {
            if (frontId != 0)
            {
                Guide guide = GameComponentData.gameData.guideController.guides.Find(g => g.id == frontId);
                if (!guide.end)
                {
                    return false;
                }
            }
            switch (guideTriggerType)
            {
                case GuideTriggerType.游戏开始分辨率:
                    return GameComponentData.gameData.guideController.startGame0;
                case GuideTriggerType.游戏开始:
                    return GameComponentData.gameData.guideController.startGame;
                    
                case GuideTriggerType.进入地图:
                   
                    return false;
               case GuideTriggerType.电影结束:
                   return GameComponentData.gameData.guideController.filmId == int.Parse(guideTriggerValue) ;
              //  case GuideTriggerType.对话完成:
               //     return GameComponentData.gameData.talkTextsManager.nowTalk.talkId== int.Parse(guideTriggerValue);

            }
            
            
        }
        
        return false;
    }
}


public class GuideController : MonoBehaviour
{
    public bool guideSet,set2,set1;
    public bool isGuide;
    public InputField inputField;
    public GameObject rightFunction;
    public GameObject helpObj;
    public GameObject maskObj;
    public GameObject guidSetPanel;
    public List<Guide> guides;
    [HideInInspector] public bool sleepEnd, startBusiness, needFood, getMoney, tipsOver, startGame,startGame0;
    [HideInInspector] public int cookedFood;
    [HideInInspector] public int filmId;
    public bool isMaskMove;
    [HideInInspector]
    public Guide nowGuide;

    private GuideStep guideStep;
    private int displayIndex;
	// Use this for initialization
	void Start ()
	{
	    //displayIndex = 0;
	    if (guideSet)
	    {
	        isMaskMove = false;
	        helpObj.SetActive(false);
            guidSetPanel.SetActive(true);
	    }
	    
    }

    public void CheckGuide()
    {
        if (!DataSaveAndLoadTest.isJsonData&&isGuide)
        {
            foreach (var guide in guides)
            {
                if (guide.CheckTriggers())
                {
                    nowGuide = guide;
                    break;
                }
            }
            if (nowGuide != null&& nowGuide.id!=0)
            {
                displayIndex = 0;
                DisplayGuide();
            }
        }
        
    }

    public void MoveArraw()
    {
        
    }
    public void StepActionEnd()
    {
        if (displayIndex >= nowGuide.guideSteps.Count)
        {
            Time.timeScale = 1;
            nowGuide.end = true;
            GameComponentData.gameData.eventManager.CheckEvents();
            nowGuide = null;
            guideStep = null;
            maskObj.SetActive(false);
            displayIndex = 0;
            filmId = 0;

        }
        else
        {
            DisplayGuide();
        }
    }
    void DisplayGuide()
    {
        
        guideStep = nowGuide.guideSteps[displayIndex];
        if (guideStep.stopTime)
        {
            Time.timeScale = 0;
        }
        displayIndex++;
        switch (guideStep.stepType)
        {
                case StepType.对话:
                maskObj.SetActive(false);
                //GameComponentData.gameData.talkTextsManager.TalkAction(guideStep.stepValue,TalkActionType.指引);
                break;
                case StepType.箭头指引:
                var x = guideStep.stepValue.Split(';');
                    if (guideStep.stepValue1 != "")
                    {
                        
                    
                        float nowX = Screen.height / (float)Screen.width;
                        if (nowX >  1.8f)
                        {
                        x = guideStep.stepValue1.Split(';');
                    }
                        
                }
                var x1 = x[0].Split(',');
                    var x2 = x[1].Split(',');
                Vector2 pos=new Vector2(int.Parse(x1[0])/1000.0f, int.Parse(x1[1]) / 1000.0f);
                Vector2 scale= new Vector2(int.Parse(x2[0]) / 1000.0f, int.Parse(x2[1]) / 1000.0f);
                DisplayArrawAction(pos,scale);
                break;
        }

       
    }

    public void ClickArrawMask()
    {
        StepActionEnd();
    }
    public void DisplayArrawAction(Vector2 pos,Vector2 scale)
    {
        maskObj.SetActive(true);
        maskObj.transform.position = pos;
        maskObj.transform.GetChild(1).localScale=new Vector3(scale.x,scale.y,1);
    }
    public void LoadGuidData()
    {
        string path = "Datas/GuideDatas";
        TextAsset textAsset = Resources.Load<TextAsset>(path);
        if (textAsset != null)
        {
            string jsonStr = textAsset.text;
            guides = JsonMapper.ToObject<List<Guide>>(jsonStr);
        }
    }
    public void SaveGuidData()
    {
        if (set2 || guideSet)
        {
            foreach (var guide in guides)
            {
                guide.end = false;
            }
        }
        string path = Application.dataPath + "/Resources/Datas/GuideDatas.json";
        if (File.Exists(path))
        {
            File.Delete(path);
        }
        string jsonStr = JsonMapper.ToJson(guides);
        FileStream fileStream = new FileStream(path, FileMode.OpenOrCreate);
        StreamWriter stream = new StreamWriter(fileStream);
        stream.Write(jsonStr);
        stream.Close();
    }
    public void AddGuide()
    {
        if (guides == null) { guides=new List<Guide>();}
        Guide guide = new Guide()
            { guideTriggerType = GuideTriggerType.游戏开始,guideSteps =new List<GuideStep>(),guideTriggerValue = ""};
        guides.Add(guide);
        inputField.text = "";
    }
    public void AddGuideStep()
    {
        if (guides.Count > 0)
        {
            GuideStep guideStep0 = new GuideStep()
            {
                stepType = StepType.对话,
                stepValue = inputField.text
            };

            guides[guides.Count - 1].guideSteps.Add(guideStep0);
        }
        inputField.text = "";
    }

    public void StartMaskMove()
    {
        isMaskMove
            = true;
        helpObj.SetActive(true);
    }
    public void AddGuideMaskStep()
    {
        if (guides.Count > 0)
        {
            GuideStep guideStep=new GuideStep()
            {
                stepType = StepType.箭头指引,
                stepValue = ""
            };
            guideStep.SetStepValueForGuideMask(new GuideMask(maskObj));
            guides[guides.Count-1].guideSteps.Add(guideStep);
        }
        inputField.text = "";
        helpObj.SetActive(false);
        rightFunction.SetActive(false);
    }

    public void ClickCancleButton()
    {
        rightFunction.SetActive(false);
        isMaskMove = true;
    }

    public void ClickMoveSaveButton()
    {
        helpObj.SetActive(false);
        rightFunction.SetActive(false);
        if (set2 && guideStep != null)
        {
            guideStep.SetStepValueForGuideMask2(new GuideMask(maskObj));
        }
        
    }
	// Update is called once per frame
	void Update () {
	    if (guideSet&&isMaskMove)
	    {
	        if (Input.GetMouseButton(0))
	        {
	            Vector3 mousePos = Input.mousePosition;
	            Vector3 maskScreenPos = Camera.main.WorldToScreenPoint(Vector3.zero);
	            Vector3 pos = Camera.main.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, maskScreenPos.z));
	            maskObj.transform.position = pos;
	        }
	        if (Input.GetAxis("Mouse ScrollWheel") != 0)
	        {
	            if (Input.GetKey(KeyCode.LeftControl))
	            {
	                float scaleValue = maskObj.transform.GetChild(1).localScale.x;
	                scaleValue += Input.GetAxis("Mouse ScrollWheel");
	                maskObj.transform.GetChild(1).localScale = new Vector3(scaleValue, maskObj.transform.GetChild(1).localScale.y, scaleValue);
                }
	            else
	            {
	                float scaleValue = maskObj.transform.GetChild(1).localScale.x;
	                scaleValue += Input.GetAxis("Mouse ScrollWheel");
	                maskObj.transform.GetChild(1).localScale = new Vector3(scaleValue, scaleValue, scaleValue);
	                
                }
	            

	        }
	        if (Input.GetMouseButton(1))
	        {
	            isMaskMove = false;
                rightFunction.SetActive(true);
                rightFunction.transform.position= maskObj.transform.position;
	            
            }
	    }
	    if (set1 && guideStep != null)
	    {
	        if (Input.GetMouseButton(1))
	        {
	            Vector3 mousePos = Input.mousePosition;
	            Vector3 maskScreenPos = Camera.main.WorldToScreenPoint(Vector3.zero);
	            Vector3 pos = Camera.main.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, maskScreenPos.z));
	            maskObj.transform.position = pos;
	        }

	        if (Input.GetMouseButtonUp(1))
	        {
	            guideStep.SetStepValueForGuideMask(new GuideMask(maskObj));
	        }
	    }
        if (set2&&guideStep!=null)
	    {
	        if (Input.GetMouseButton(1))
	        {
	            Vector3 mousePos = Input.mousePosition;
	            Vector3 maskScreenPos = Camera.main.WorldToScreenPoint(Vector3.zero);
	            Vector3 pos = Camera.main.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, maskScreenPos.z));
	            maskObj.transform.position = pos;
	        }

	        if (Input.GetMouseButtonUp(1))
            {
	            guideStep.SetStepValueForGuideMask2(new GuideMask(maskObj));
	        }
        }
    }
}
