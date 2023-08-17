using OldName;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AnimalInformationAction : MonoBehaviour
{
    public Image animalImage;

    public InputField AnimalNameInputField;

    public Text animalAge, animalStatus;
    public Text setButtonText;
    private Animal animal;
	// Use this for initialization
	void Start () {
		LanguageManage.TextFanyi(setButtonText);
	}

    public void ChangeAnimal(InputField _inputField)
    {
        animal.Name = _inputField.text;
    
    }
    public void InitAnimal(OldName.Animal _animal)
    {
        animal = _animal;
        AnimalNameInputField.text = animal.Name;
        animalAge.text = LanguageManage.SwitchStr(animal.ageStatus.ToString());
        animalStatus.text = LanguageManage.SwitchStr(animal.animalStatus.ToString());
        animalImage.sprite = animal.Obj.GetComponentInChildren<SpriteRenderer>().sprite;
    }

    public void ClickSetAnimal()
    {
        if (animal != null)
        {
            GameComponentData.gameData.pasturePanelAction.animalPanel.SetActive(true);
            GameComponentData.gameData.pasturePanelAction.animalPanel.GetComponent<AnimalSetPanelAction>().InitAnimalSetPanelData(animal);
        }
        
    }
	// Update is called once per frame
	void Update () {
		
	}
}
