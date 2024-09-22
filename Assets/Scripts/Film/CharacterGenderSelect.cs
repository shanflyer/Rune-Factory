using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterGenderSelect : MonoBehaviour
{
    [SerializeField]
    GameObject maleObj,femaleObj;
    private void OnEnable()
    {
        var gender= CharacterManager.instance.player.characterData.gender;
        if (gender == Gender.female)
        {
            maleObj.SetActive(false);
            femaleObj.SetActive(true);
        }
        else
        {
            maleObj.SetActive(true);
            femaleObj.SetActive(false);
        }
    }
}
