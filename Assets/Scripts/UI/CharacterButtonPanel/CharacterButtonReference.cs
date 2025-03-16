using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class CharacterButtonReference : UIObjReference<MyInt>
{
    [SerializeField]
    Button button;
    [SerializeField]
    Image icon;
    [SerializeField]
    Vector2 iconSize = new Vector2(48, 48);
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
            try
            {
                character.characterData.head.SetImageSprite(icon, iconSize, Vector2.zero);
            }
            catch
            {
                Debug.LogError($"errr:{character.name}");
            }
           
        }
        return base.InitData(t, SelectAction, toggleGroup);
    }
}
