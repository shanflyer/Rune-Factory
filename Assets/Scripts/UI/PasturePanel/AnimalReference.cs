using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class AnimalReference : UIObjReference<MyInt>
{
    [SerializeField]
    Toggle toggle;
    [SerializeField]
    Image icon;
    [SerializeField]
    Vector2 iconSize = new Vector2(48, 48);
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
        icon = FindChildGameObject<Image>("NPCImage");
    }
    private void Awake()
    {
        toggle.onValueChanged.AddListener((bool value) =>
        {
            if (SelectAction != null)
            {
                SelectAction(data,index);
            }
        });
    }
    public override async Task InitData(MyInt t, SelectAction<MyInt> SelectAction = null, ToggleGroup toggleGroup = null)
    {
        await base.InitData(t, SelectAction, toggleGroup);
        toggle.group = toggleGroup;
        Character character = CharacterManager.instance.GetCharacter(data.value);
        character.characterData.head.SetImageSprite(icon, iconSize,Vector2.zero);
    }
}
