
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Seedaction : MonoBehaviour
{
    public SpriteRenderer seedRenderer;

    public void InitSeed()
    {
        Item seed = GameComponentData.gameData.farmAction.farmTool;
        seedRenderer.sprite = GameComponentData.gameData.itemsManager.GetItemIcon(seed);
    }
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
