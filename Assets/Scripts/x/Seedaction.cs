
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Seedaction : MonoBehaviour
{
    public SpriteRenderer seedRenderer;

    public async void InitSeed()
    {
        Item seed = GameComponentData.gameData.farmAction.farmTool;
        ItemData itemData =
               await GameDataManager.instance.GetAsyncObjectData<ItemData>(seed.dataId);
        seedRenderer.sprite = itemData.iconSprite;
    }
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
