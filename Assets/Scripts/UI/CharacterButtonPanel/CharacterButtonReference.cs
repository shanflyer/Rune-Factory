using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class CharacterButtonReference : UIObjReference<MyInt>
{
    [SerializeField]
    Button button;
    [SerializeField]
    Image icon;

    public override void SetPanelUISerializeObj()
    {
        base.SetPanelUISerializeObj();
        button = GetComponent<Button>();
        icon = FindChildGameObject<Image>("Head");
    }
    private void Awake()
    {
        button.onClick.AddListener(() =>
        {
            if (SelectAction != null)
            {
                SelectAction(data, true);
            }

        } );
        
    }
    public override Task InitData(MyInt t, SelectAction<MyInt> SelectAction = null, ToggleGroup toggleGroup = null)
    {
        
        Character character = CharacterManager.instance.GetCharacter(t.value);
        if(character != null)
        {
            character.characterData.head.SetImageSprite(icon);
        }
        return base.InitData(t, SelectAction, toggleGroup);
    }
}
