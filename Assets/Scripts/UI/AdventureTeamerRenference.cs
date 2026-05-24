using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AdventureTeamerRenference : UIObjReference<FighterResult>
{
    [SerializeField]
    private Image Icon;

    [SerializeField]
    private Transform LevelUp;

    [SerializeField]
    private Transform SkillUp;

    [SerializeField]
    private TextMeshProUGUI Name;

    [SerializeField]
    private TextMeshProUGUI Level;

    public override void SetPanelUISerializeObj()
    {
        Icon = FindChildGameObject<Image>("NPCImage");
        LevelUp = FindChildGameObject("LevelUp");
        SkillUp = FindChildGameObject("SkillUp");
        Name = FindChildGameObject<TextMeshProUGUI>("NPCName");
        Level = FindChildGameObject<TextMeshProUGUI>("NPCLevel");

        base.SetPanelUISerializeObj();
    }

    static Vector2 iconSize = new Vector2(48, 48);
    public override async Task InitData(FighterResult t, SelectAction<FighterResult> SelectAction = null, ToggleGroup toggleGroup = null)
    {
       await base.InitData(t, SelectAction, toggleGroup);

        this.LevelUp.localScale = data.levelUp ? Vector3.one : Vector3.zero;
        this.SkillUp.localScale = data.skillUp ? Vector3.one : Vector3.zero;
        data.Character.characterData.head.SetImageSprite(Icon, iconSize);
        //Icon.sprite = data.Character.characterData.icon.sprite;
        if (data.Character != CharacterManager.instance.controllerCharacter)
        {
            Name.SetSWText(data.Character.name);
        }
        else
        {
            Name.text=(data.Character.name);
        }

        Level.text = $"Lv.{data.Character.Level}";
    }
}
