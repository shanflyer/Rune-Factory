using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CostItem : UIObjReference<MyInt3>
{
    [SerializeField]
    Image icon;
    [SerializeField]
    TextMeshProUGUI countText;
    public override void InitChildObjData()
    {
        base.InitChildObjData();
        icon = FindChildGameObject<Image>("Icon");
        countText = FindChildGameObject<TextMeshProUGUI>("count");
    }
    public override async Task InitData(MyInt3 t, SelectAction<MyInt3> SelectAction = null, ToggleGroup toggleGroup = null)
    {
        ItemData itemData=await GameDataManager.instance.GetAsyncData<ItemData>(t.value.x);
        icon.sprite = itemData.icon;
        countText.text = $"{t.value.y}/{t.value.z}";
        countText.color = t.value.y > t.value.z ? Color.red : Color.green;

      await  base.InitData(t, SelectAction, toggleGroup);

    }
}
