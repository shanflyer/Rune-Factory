using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadingAction : MonoBehaviour
{
    public Sprite s1, s2;
    public SpriteRenderer sr1, sr2;   
	

    public void InitData(bool isZero)
    {
        
        if (DataSaveAndLoadTest.gameSaveData.playerSaveData.gender == Gender.female)
        {
            sr1.sprite = s2;
        }
        else
        {
            sr1.sprite = s1;
        }
        if (!isZero)
        {
            sr1.enabled = true;
            sr2.enabled = false;
        }
        else
        {
            sr1.enabled = true;
            sr2.enabled = true;
        }
    }
}
