using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; 

public class FightCharacterReference : UIObjReference<FightChapter>
{
    [SerializeField]
    Text ATText, DFText, NameText;
    [SerializeField]
    Slider ExpSlider, HPSlider;
    [SerializeField]
    Text LevelText;
    [SerializeField]
    Text TypeText;
    [SerializeField]
    Image Attritube;

    [SerializeField]
    Toggle PlayerSelect;

    private void Awake()
    {
        
    }
    public override void OnEnable()
    {
        base.OnEnable();
        GameActionManager.instance.AddListener<RefreshCharacter>(RefreshCharacter);
    }
    public override void OnDisable()
    {
        base.OnDisable();
        GameActionManager.instance.RemoveListener<RefreshCharacter>(RefreshCharacter);
    }

    public override void SetPanelUISerializeObj()
    {
        base.SetPanelUISerializeObj();
        PlayerSelect = GetComponent<Toggle>();
        PlayerSelect.onValueChanged.AddListener(SelectPlayer);

        ATText = FindChildGameObject<Text>("AT");
        DFText = FindChildGameObject<Text>("DF");
        NameText = FindChildGameObject<Text>("Name");

        ExpSlider = FindChildGameObject<Slider>("ExpSlider");
        HPSlider = FindChildGameObject<Slider>("HPSlider");

        LevelText = FindChildGameObject<Text>("Level");
        TypeText = FindChildGameObject<Text>("Type");
         
        Attritube = FindChildGameObject<Image>("Attritube");
    }

    int characterId;
    public int CharacterId => characterId;

    void SelectPlayer(bool value)
    {
        if (value)
        {

        }
    }

    void RefreshCharacter(RefreshCharacter refreshCharacter)
    {
        if (this.characterId == refreshCharacter.id)
        {
            InitData(characterId);             
        }
    }

    public void InitData(int characterId, Action action = null)
    {
        Attritube.enabled = false;
        TypeText.enabled = false;

        Character character = CharacterManager.instance.GetCharacter(characterId);
        if (character != null)
        {
            this.characterId = character.instanceId;
            NameText.text = character.name;
            LevelText.text = $"Lv.{character.Level}";
            CharacterProperty characterProperty = character.CharacterProperty;
            HPSlider.value = (float)characterProperty.HP / characterProperty.MaxHP;
            ExpSlider.value = (float)character.exp.nowExp / character.exp.nowLevelExp;
            ATText.text = $"AT {characterProperty.AT}";
            DFText.text = $"DF {characterProperty.DF}";
        }

    }
   
}
