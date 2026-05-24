using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Playables;

public class LoadingPanel : GamePanel<IReferenceData>
{
    [SerializeField]
    PlayableDirector playableDirector;
    [SerializeField]
    GameObject boy, girl;
    public override void InitReferenceData(IReferenceData v)
    {
        boy = FindChildGameObject("主角男").gameObject;
        girl= FindChildGameObject("主角女").gameObject;
        playableDirector = FindChildGameObject<PlayableDirector>("Load");
        base.InitReferenceData(v);
    }
    public override void Close()
    {
        playableDirector.gameObject.SetActive(false);
        base.Close();
    }
    public override Task InitData(string dataKey)
    {
        playableDirector.gameObject.SetActive(true);
        playableDirector.Play();
        boy.SetActive(GameDataSaveManager.instance.UserGameSaveData.playerData.gender == Gender.male);
        girl.SetActive(GameDataSaveManager.instance.UserGameSaveData.playerData.gender == Gender.female);
        return base.InitData(dataKey);
    }
}
