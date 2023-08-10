using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FightAnimationAction : MonoBehaviour {

    public void GoldHurt()
    {
        GameComponentData.gameData.BattleMapAction.Hurt(gameObject);
        GetComponent<Animator>().SetBool("IsFight",false);
    }

    public void PlayerDeadEnd()
    {
        GameComponentData.gameData.BattleMapAction.PlayerDead();
    }
    public void DeadEnd()
    {
        Destroy(gameObject);
        GameComponentData.gameData.BattleMapAction.MonsterDeadAction();
        
    }
    public void FightEnd()
    {
        GameComponentData.gameData.BattleMapAction.FightEnd(gameObject);
    }
    public void HurtEnd()
    {
        GameComponentData.gameData.BattleMapAction.HurtEnd(gameObject);
        GetComponent<Animator>().SetBool("IsHurt", false);
    }
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
