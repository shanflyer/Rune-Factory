using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectAction : MonoBehaviour {
    public void EffectEnd()
    {
        GameComponentData.gameData.BattleMapAction.AfterEffectAction();
        Destroy(gameObject);
    }

    public void RebronEffectEnd()
    {
        GameComponentData.gameData.BattleMapAction.AfterBronEffect();
        Destroy(gameObject);
    }
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
