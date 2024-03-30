using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class AnimalReference : UIObjReference<MyInt>
{
    [SerializeField]
    Toggle toggle;
    [SerializeField]
    Image icon;
    public override void SelectDefault()
    {
        base.SelectDefault();
        toggle.isOn = true;
    }
    public override void ClearSelect()
    {
        base.ClearSelect();
        toggle.SetIsOnWithoutNotify(false);
    }
    public override void SetPanelUISerializeObj()
    {
        base.SetPanelUISerializeObj();
        toggle = GetComponent<Toggle>();
        icon = FindChildGameObject<Image>("Icon");
    }
    private void Awake()
    {
        toggle.onValueChanged.AddListener((bool value) =>
        {
            if (SelectAction != null)
            {
                SelectAction(data);
            }
        });
    }
    public override Task InitData(MyInt t, SelectAction<MyInt> SelectAction = null, ToggleGroup toggleGroup = null)
    {
        toggle.group = toggleGroup;
        Character character = CharacterManager.instance.GetCharacter(data.value);
        icon.sprite = character.characterData.icon;
        icon.rectTransform.sizeDelta= GameCommon.SetImageSize(icon.sprite, new Vector2(48, 48));
        return base.InitData(t, SelectAction, toggleGroup);
    }
}
