using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopClickEventAction : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}

    public void ClickShop(int shopId)
    {
        GameComponentData.gameData.gameManager.MoveToShopMap(shopId);
    }
    public void ClickMove(int mapID)
    {
        GameComponentData.gameData.gameManager.MoveToMap(mapID);
    }
    // Update is called once per frame
    void Update () {
		
	}
}
