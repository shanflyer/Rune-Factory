using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

public class EmoteManager : Singleton<EmoteManager>
{
    private Animator emoteAnimator;
    private static string emoteObjPath = "Prefabs/Other/emote";

    public override async void Init()
    {
        base.Init();
        var _emotePrefab = await GameSourceManager.instance.GetPrefab(emoteObjPath);
        emoteAnimator = _emotePrefab.GetComponent<Animator>();
        GameActionManager.instance.AddListener<ShowEmote>(ShowEmote);
    }

    public RuntimeObj GetEmote(int id, Transform parent)
    {
        RuntimeObj runtimeObj = GameRuntimeObjManager.instance.CreatRuntimeObj<Animator>(RuntimeObjType.EMOTE.ToString(),
            "emote", emoteAnimator, id, parent);
        return runtimeObj;
    }

    private async void ShowEmote(ShowEmote showEmote)
    {
        RuntimeObj runtimeObj = default(RuntimeObj);
        switch (showEmote.entityType)
        {
            case EntityType.角色:
                if (CharacterManager.instance.GetRuntimeCharacterObj(showEmote.id, out var characterRuntimeObj))
                {
                    runtimeObj = GetEmote(showEmote.emoteId, characterRuntimeObj.model);
                }

                break;

            default:
                if (WorldMapObjManager.instance.GetRuntimeMapItemObj(showEmote.id, out var itemRuntimeObj))
                {
                    runtimeObj = GetEmote(showEmote.emoteId, itemRuntimeObj.transform);
                }
                break;
        }
        if (runtimeObj.obj == null)
        {
            return;
        }
        var emoteData = await GameDataManager.instance.GetAsyncData<EmoteData>(showEmote.emoteId);
        Animator animator = runtimeObj.obj as Animator;
        animator.transform.localPosition = new Vector3(emoteData.X * 0.01f, emoteData.Y * 0.01f, -emoteData.Y * 0.01f);
        var playableGraph = PlayableGraph.Create();
        var playableOutput = AnimationPlayableOutput.Create(playableGraph, "emote", animator);
        var clipPlayable = AnimationClipPlayable.Create(playableGraph, emoteData.animationClip);
        playableOutput.SetSourcePlayable(clipPlayable);
        playableGraph.Play();

        GameTimerController.instance.DeleyActionMain(showEmote.showTime, () =>
        {
            playableGraph.Stop();
            GameRuntimeObjManager.instance.RecycleRuntimeObj(runtimeObj);
        });
    }
}