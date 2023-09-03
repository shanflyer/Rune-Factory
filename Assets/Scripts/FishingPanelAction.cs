using OldName;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FishingPanelAction : MonoBehaviour
{
    public List<Text> Texts;
    public Animator animator;
    public Slider FishSlider, HookSlider;
    public Sprite fish0, fish1;
    public GameObject functionObj, fishItemObj, nextObj;
    public Image FishImage;
    public Text fishingLevel;

    public Slider FishExpSlider;
    public Image itemIcon;
    public Text ItemNoticeText;
    public Vector2 speedRange;

    private Fish selectFish;

    private Item fishItem;
    // Use this for initialization
    void Start () {
        foreach (var text in Texts)
        {
            LanguageManage.TextFanyi(text);
        }
	}
    public void InitFishData()
    {

        functionObj.SetActive(true);
        fishItemObj.SetActive(false);
        nextObj.SetActive(false);
        
        StartFishing();
    }
    
    public async void OKButtonAction()
    {
        GamePlayer gamePlayer = GameComponentData.gameData.gameManager.gamePlayer;
        if (PackageManager.instance.IsHaveItem(0,1168100))
        {
            if (gamePlayer.property.Power >= 10)
            {
                gamePlayer.property.Power -= 10;
                RemovePackageItem removePackageItem = new RemovePackageItem
                {
                    itemDataId = 1168100,
                    itemCount = 1
                };
                GameActionManager.instance.QueueAction(removePackageItem, true);
                AudioController.instance.StopBgm();
                StopCoroutine("FishMoving");
                animator.SetBool("IsFish", true);
                GameComponentData.gameData.gameManager.UpDataPlayer();
            }
            else
            {
                GameNotificationManager.instance.DisplayTips(LanguageManage.SwitchStr("体力不足"),
                    LanguageManage.SwitchStr("体力不足，无法垂钓，在床上休息可恢复体力！"));
            }
        }
        else if (PackageManager.instance.IsHaveItem(0, 1125100))
        {

            if (gamePlayer.property.Power >= 10)
            {
                gamePlayer.property.Power -= 10;

                RemovePackageItem removePackageItem = new RemovePackageItem
                {
                    itemDataId = 1125100,
                    itemCount = 1
                };
                GameActionManager.instance.QueueAction(removePackageItem, true);
                 
                AudioController.instance.StopBgm();
                StopCoroutine("FishMoving");
                animator.SetBool("IsFish", true);
                GameComponentData.gameData.gameManager.UpDataPlayer();
            }
            else
            {
                GameNotificationManager.instance.DisplayTips(LanguageManage.SwitchStr("体力不足"),
                    LanguageManage.SwitchStr("体力不足，无法垂钓，在床上休息可恢复体力！"));
            }

        }
        else
        {
            GameNotificationManager.instance.DisplayTips(LanguageManage.SwitchStr("鱼饵不足"),
                LanguageManage.SwitchStr("鱼饵不足，无法垂钓，商店街的动物店有鱼饵出售！"));
        }







    }

    public async void GetFish()
    {

        AudioController.instance.PlayAudio(SE.Fish);
        if (Mathf.Abs(FishSlider.value - HookSlider.value) > 0.15f)
        {
            functionObj.SetActive(false);
            nextObj.SetActive(true);

            GameNotificationManager.instance.DisplayTips(LanguageManage.SwitchStr("很遗憾"),
                LanguageManage.SwitchStr("鱼跑了！"));
        }
        else
        {

            var fishes =
                GameComponentData.gameData.fishManager.Fishes.FindAll(
                    f => f.Seasons.Exists(s => s == GameTimeManager.nowGameTime.gameDate.season));
            int maxValue = 0;
            List<int> indexes = new List<int>();
            if (fishes.Count > 0)
            {
                foreach (var fish in fishes)
                {
                    maxValue += fish.randomValue;
                    indexes.Add(maxValue);
                }
            }
            int randowValue = Random.Range(0, maxValue);
            for (int i = 0; i < indexes.Count; i++)
            {
                if (i < indexes.Count - 1)
                {
                    if (indexes[i] <= randowValue && indexes[i + 1] > randowValue)
                    {
                        selectFish = fishes[i];
                        break;
                    }
                }
                else
                {
                    selectFish = fishes[i];
                }
                
            }
            fishItemObj.SetActive(true);
            functionObj.SetActive(false);
            ItemData itemData = await GameDataManager.instance.GetAsyncData<ItemData>(selectFish.item);
            GameComponentData.gameData.charactorTitleAction.AddFishCount(selectFish.item);
            fishItem = ItemManager.instance.CreatItem(itemData, 1);
            itemIcon.sprite = itemData.icon;
            ItemNoticeText.text = LanguageManage.SwitchStr("收获了1条") + selectFish.name;
 
        }
        
    }
    public async void PutInPackage()
    {
        AudioController.instance.PlayAudio(SE.click);
        int x =await PackageManager.instance.SetItemInPackage(fishItem,0);
        if (x > 0)
        {
            Vector2Int coordinate = GameComponentData.gameData.gameManager.playerCharactor.coordinate;
            GameComponentData.gameData.gameManager.GreatGroundItem(fishItem,new Vector2Int(coordinate.x,coordinate.y-1));
            GameNotificationManager.instance.DisplayTips(LanguageManage.SwitchStr("背包已满"),LanguageManage.SwitchStr("一条")
                +selectFish.name+ LanguageManage.SwitchStr("落在地上，落在地上的道具随时会丢失，请及时回收！"));

        }
        fishItemObj.SetActive(false);
        nextObj.SetActive(true);
    }

    public void ContineuButtonAction()
    {
        AudioController.instance.PlayAudio(SE.click);
        functionObj.SetActive(true);
        nextObj.SetActive(false);
        StartFishing();
    }
    public void StartFishing()
    {
        animator.SetBool("IsFish", false);
        AudioController.instance.PlayAudio(BGM.water); 
        StartCoroutine("FishMoving");
    }

    public void EndFishing()
    {
        AudioController.instance.PlayAudio(SE.Return);
        GameComponentData.gameData.passDataManager.PlayerMapBGM();
        gameObject.SetActive(false);
    }
    IEnumerator FishMoving()
    {
        float speed0 = Random.Range(speedRange.x, speedRange.y);
        float speed1 = -Random.Range(speedRange.x, speedRange.y);
        float value0=0, value1=1;
        while (true)
        {
            value0 += speed0;
            value1 += speed1;
            FishSlider.value = value0;
            HookSlider.value = value1;
            if (value0 >= 1)
            {
                AudioController.instance.PlayAudio(SE.water2);
                speed0 = -Random.Range(speedRange.x, speedRange.y);
                value0 = 1;
                FishImage.sprite = fish1;
            }
            if (value0 <= 0)
            {
                AudioController.instance.PlayAudio(SE.water2);
                speed0 = Random.Range(speedRange.x, speedRange.y);
                value0 = 0;
                FishImage.sprite = fish0;
            }
            if (value1 <= 0)
            {
                speed1 = +Random.Range(speedRange.x, speedRange.y);
                value1 = 0;
            }
            if (value1 >= 1)
            {
                speed1 = -Random.Range(speedRange.x, speedRange.y);
                value1 = 1;
            }
            yield return new WaitForSeconds(0.05f);
        }
        
    }
	// Update is called once per frame
	void Update () {
		
	}
}
