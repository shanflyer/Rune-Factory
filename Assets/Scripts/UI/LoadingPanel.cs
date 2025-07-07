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
        boy = FindChildGameObject("Ö÷½ÇÄÐ").gameObject;
        girl= FindChildGameObject("Ö÷½ÇÅ®").gameObject;
        playableDirector = FindChildGameObject<PlayableDirector>("Load");
        base.InitReferenceData(v);
    }
    public override void Close()
    {
        playableDirector.gameObject.SetActive(false);
        base.Close();
    }
    public override void InitData(string dataKey)
    {
        playableDirector.gameObject.SetActive(true);
        playableDirector.Play();
        boy.SetActive(GameDataSaveManager.instance.UserGameSaveData.playerData.gender == Gender.male);
        girl.SetActive(GameDataSaveManager.instance.UserGameSaveData.playerData.gender == Gender.female);
        base.InitData(dataKey);
    }
}
