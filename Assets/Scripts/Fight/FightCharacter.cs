using System.Collections;
using UnityEngine;
using UnityEngine.Playables;

interface FightCharacter
{

}
public struct FightPlayer
{
    public RuntimeObj playerObj;
    public PlayableDirector playableDirector;
    public Animator animator;
}
public class FightCharacterManager:Singleton<FightCharacterManager>  
{
    protected override void Clear()
    {
        base.Clear();
    }
    public override void Init()
    {
        base.Init();
    }
}
 