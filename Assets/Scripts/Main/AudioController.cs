using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Playables;

public enum AudioClearType
{
    All,
    NoClear
}

public struct AudioPlayData
{
    public AudioClip audioClip;
    public float weight;
    public bool loop;
}

public enum BGSGroup
{
    Default, Map, Lightning, Rain, Wind
}
public enum BGMGroup
{
    Default, Map, Battle, Theme
}
public enum MEGroup
{
    Default,
    Battle
}
public enum SEGroup
{
    Default, UI
}
public delegate void SetAudioAction(AudioClip audioClip);

public class AudioController : Singleton<AudioController>
{
    private bool TryGetClip(Playable playable, out AudioClip clip)
    {
        clip = null;
        if (!playable.IsValid() || !playable.IsPlayableOfType<AudioClipPlayable>())
            return false;

        clip = ((AudioClipPlayable)playable).GetClip();
        return clip != null;
    }

    public override async void Init()
    {
        base.Init();
        audioMixer = await ExtensionsResources.LoadResourceAsync<AudioMixer>("Audio/AudioMixer");
        lerpIEnumeratorDic.Clear();
        float value = PlayerPrefs.GetFloat("MasterVolume", 1);
        SetMasterVolume(value);
        value = PlayerPrefs.GetFloat("BGMVolume", 1);
        SetBGMVolume(value);
        value = PlayerPrefs.GetFloat("SEVolume", 1);
        SetSEVolume(value);
    }

    protected override void Clear()
    {
        base.Clear();
        lerpIEnumeratorDic.Clear();
        StopBgm();
        StopBGS();
        StopME();
        StopSE();
        meMixerDic.Clear();
        bgsMixerDic.Clear();
        bgmMixerDic.Clear();
        seMixerDic.Clear();

        if (bgmGraph.IsValid()) bgmGraph.Destroy();
        if (bgsGraph.IsValid()) bgsGraph.Destroy();
        if (seGraph.IsValid()) seGraph.Destroy();
        if (meGraph.IsValid()) meGraph.Destroy();

        bgmOut = AudioPlayableOutput.Null;
        bgsOut = AudioPlayableOutput.Null;
        meOut = AudioPlayableOutput.Null;
        seOut = AudioPlayableOutput.Null;

        if (!audioMixer.GetFloat("MasterVolume", out var value))
        {
            value = 0;
        }
        value = (value + 40) * 0.025f;
        PlayerPrefs.SetFloat("MasterVolume", value);
        if (!audioMixer.GetFloat("BGMVolume", out value))
        {
            value = 0;
        }
        value = (value + 40) * 0.025f;
        PlayerPrefs.SetFloat("BGMVolume", value);
        if (!audioMixer.GetFloat("SEVolume", out value))
        {
            value = 0;
        }
        value = (value + 40) * 0.025f;
        PlayerPrefs.SetFloat("SEVolume", value);
    }

    [SerializeField]
    AudioSource bgmAudioSource, bgsAudioSource, meAudioSource, seAudioSource;

    public void SetBgmAudioSourceVolume(float value)
    {
        if (bgmAudioSource) bgmAudioSource.volume = value;
    }

    public void SetAudioSource(GameObject audioObj)
    {
        var BGM = audioObj.transform.Find("BGM");
        var BGS = audioObj.transform.Find("BGS");
        var ME = audioObj.transform.Find("ME");
        var SE = audioObj.transform.Find("SE");

        if (BGM)
        {
            bgmAudioSource = BGM.GetComponent<AudioSource>();
            bgmGraph = PlayableGraph.Create("BGM");
            bgmOut = AudioPlayableOutput.Create(bgmGraph, "BGM", bgmAudioSource);
            bgmMixer = AudioMixerPlayable.Create(bgmGraph);
            bgmOut.SetSourcePlayable(bgmMixer);
        }
        if (BGS)
        {
            bgsAudioSource = BGS.GetComponent<AudioSource>();
            bgsGraph = PlayableGraph.Create("BGS");
            bgsOut = AudioPlayableOutput.Create(bgsGraph, "BGS", bgsAudioSource);
            bgsMixer = AudioMixerPlayable.Create(bgsGraph);
            bgsOut.SetSourcePlayable(bgsMixer);
        }

        if (ME)
        {
            meAudioSource = ME.GetComponent<AudioSource>();
            meGraph = PlayableGraph.Create("ME");
            meOut = AudioPlayableOutput.Create(meGraph, "ME", meAudioSource);
            meMixer = AudioMixerPlayable.Create(meGraph);
            meOut.SetSourcePlayable(meMixer);
        }
        if (SE)
        {
            seAudioSource = SE.GetComponent<AudioSource>();
            seGraph = PlayableGraph.Create("SE");
            seOut = AudioPlayableOutput.Create(seGraph, "SE", seAudioSource);
            seMixer = AudioMixerPlayable.Create(seGraph);
            seOut.SetSourcePlayable(seMixer);
        }
    }

    private AudioMixer audioMixer;

    private PlayableGraph bgmGraph, bgsGraph, meGraph, seGraph;
    private AudioPlayableOutput bgmOut, bgsOut, meOut, seOut;
    private AudioMixerPlayable bgmMixer, bgsMixer, meMixer, seMixer;

    private Dictionary<string, AudioMixerPlayable> bgmMixerDic = new Dictionary<string, AudioMixerPlayable>();
    private Dictionary<string, AudioMixerPlayable> bgsMixerDic = new Dictionary<string, AudioMixerPlayable>();
    private Dictionary<string, AudioMixerPlayable> meMixerDic = new Dictionary<string, AudioMixerPlayable>();
    private Dictionary<string, AudioMixerPlayable> seMixerDic = new Dictionary<string, AudioMixerPlayable>();

    private string nowBGM;
    private readonly Dictionary<string, string> nowBGSs = new();
    private Dictionary<string, float> bgsWeights = new Dictionary<string, float>();
    private float bgmWeight;

    public async void PlayAudio(SE se, bool loop = false, string Group = "Default")
    {
        AudioClip audioClip = await GameSourceManager.instance.GetAudioClip(GameCommon.AddString(DataPath.SEPath, se.ToString()));
        PlaySE(audioClip, loop, Group);
    }

    public void PlayAudioME(AudioClip audioClip, bool loop = false, AudioClearType audioClearType = AudioClearType.All, float weight = 1, bool isLerp = false, string Group = "Default")
    {
        PlayME(audioClip, loop, audioClearType, weight, isLerp, Group);
    }

    public void PlayAudioBGS(AudioClip bgs, bool loop = true, AudioClearType audioClearType = AudioClearType.NoClear, float weight = 1,
        bool isLerp = false, string Group = "Default")
    {
        if (nowBGSs.TryGetValue(Group, out var nowBGS))
        {
            nowBGS = "NULL";
        }

        if (bgsWeights.TryGetValue(Group, out var bgsWeight))
        {
            bgsWeight = 0;
        }

        if ((bgs == null && nowBGS != "NULL") || (bgs != null && nowBGM != bgs.name))
        {
            bgsWeight = weight;
            nowBGM = bgs != null ? bgs.name : "NULL";
            bgsWeights[Group] = bgsWeight;
            nowBGSs[Group] = nowBGS;
            PlayBGS(bgs, loop, audioClearType, weight, isLerp, Group);
        }
        else if (bgsWeight != weight)
        {
            if (bgsMixerDic.TryGetValue(Group, out var audioMixerPlayable))
            {
                SetPlayAudioWeight(audioMixerPlayable, bgs, weight);
            }
            bgsWeight = weight;
            bgsWeights[Group] = bgsWeight;
        }
    }

    public void PlayAudioBGM(AudioClip bgm, bool loop = true, AudioClearType audioClearType = AudioClearType.NoClear, float weight = 1, bool isLerp = false, string Group = "Default")
    {
        if ((bgm == null && nowBGM != "NULL") || (bgm != null && nowBGM != bgm.name))
        {
            bgmWeight = weight;
            nowBGM = bgm != null ? bgm.name : "NULL";
            PlayBGM(bgm, loop, audioClearType, weight, isLerp, Group);
        }
        else if (bgmWeight != weight)
        {
            if (bgmMixerDic.TryGetValue(Group, out var audioMixerPlayable))
            {
                SetPlayAudioWeight(audioMixerPlayable, bgm, weight);
            }
            bgmWeight = weight;
        }
    }

    public float3 GetAudioVolume()
    {
        float3 result = new float3(1, 1, 1);
        if (audioMixer.GetFloat("MasterVolume", out var value))
        {
            result.x = (value + 40) * 0.025f;
        }
        if (audioMixer.GetFloat("BGMVolume", out value))
        {
            result.y = (value + 40) * 0.025f;
        }
        if (audioMixer.GetFloat("SEVolume", out value))
        {
            result.z = (value + 40) * 0.025f;
        }
        return result;
    }

    public async void PlayAudio(BGS bgs, bool loop = true, AudioClearType audioClearType = AudioClearType.NoClear, float weight = 1, bool isLerp = false, string Group = "Default")
    {
        if (nowBGSs.TryGetValue(Group, out var nowBGS))
        {
            nowBGS = "NULL";
        }
        if (bgsWeights.TryGetValue(Group, out var bgsWeight))
        {
            bgsWeight = 0;
        }
        if (bgs == BGS.NULL)
        {
            if (nowBGS != "NULL")
            {
                bgsWeight = weight;
                PlayBGS(null, loop, audioClearType, weight, isLerp, Group);
                nowBGS = bgs.ToString();

                bgsWeights[Group] = bgsWeight;
                nowBGSs[Group] = nowBGS;
            }
        }
        else
        {
            string bgsStr = bgs.ToString();
            var strs = bgsStr.Split("_");
            bgsStr = strs[strs.Length - 1];
            if (bgsStr != nowBGS)
            {
                bgsWeight = weight;
                nowBGS = bgsStr;
                var audioClip =
                    await GameSourceManager.instance.GetAudioClip(
                        GameCommon.AddString(DataPath.BGSPath, bgs.ToString()));
                PlayBGS(audioClip, loop, audioClearType, weight, isLerp, Group);
                bgsWeights[Group] = bgsWeight;
                nowBGSs[Group] = nowBGS;
            }
            else if (bgsWeight != weight)
            {
                if (bgsMixerDic.TryGetValue(Group, out var audioMixerPlayable))
                {
                    var audioClip =
                        await GameSourceManager.instance.GetAudioClip(GameCommon.AddString(DataPath.BGSPath,
                            bgs.ToString()));
                    SetPlayAudioWeight(audioMixerPlayable, audioClip, weight);
                }
                bgsWeight = weight;
                bgsWeights[Group] = bgsWeight;
            }
        }
    }

    public async void PlayAudio(List<float3> bgs, AudioClearType audioClearType = AudioClearType.NoClear, bool isLerp = false, string Group = "Default")
    {
        if (nowBGSs.TryGetValue(Group, out var nowBGS))
        {
            nowBGS = "NULL";
        }
        if (bgs.Count > 0)
        {
            var BGS = (BGS)(int)bgs[bgs.Count - 1].x;
            if (BGS == BGS.NULL)
            {
                nowBGS = "NULL";
            }
            else
            {
                string bgsStr = BGS.ToString();
                var strs = bgsStr.Split("_");
                nowBGS = strs[strs.Length - 1];
            }
        }
        else
        {
            nowBGS = "NULL";
        }
        List<AudioPlayData> audioPlayDatas = new List<AudioPlayData>();
        for (int i = 0; i < bgs.Count; i++)
        {
            var b = (BGS)(int)(bgs[i].x);
            AudioClip audioClip = await GameSourceManager.instance.GetAudioClip(GameCommon.AddString(DataPath.BGSPath, b.ToString()));
            AudioPlayData audioPlayData = new AudioPlayData
            {
                audioClip = audioClip,
                weight = bgs[i].y,
                loop = bgs[i].z > 0
            };
            audioPlayDatas.Add(audioPlayData);
        }

        PlayBGS(audioPlayDatas, audioClearType, isLerp, Group);
        nowBGSs[Group] = nowBGS;
        bgsWeights[Group] = 1;
    }

    public void PlaySE(AudioClip audioClip, bool loop = false, string Group = "Default")
    {
        try
        {
            AudioClipPlayable audioClipPlayable = AudioClipPlayable.Create(seGraph, audioClip, loop);
            if (!seMixerDic.TryGetValue(Group, out var audioMixerPlayable))
            {
                audioMixerPlayable = AudioMixerPlayable.Create(seGraph, 1);
                seMixerDic.Add(Group, audioMixerPlayable);
                seMixer.AddInput(audioMixerPlayable, 0, 1);
            }

            var oldPlayable = audioMixerPlayable.GetInput(0);
            if (oldPlayable.IsValid())
            {
                var oldAudioClip = (AudioClipPlayable)oldPlayable;
                if (!oldAudioClip.IsNull()) DestroyPlayable(oldAudioClip, false); // 只销毁，不卸载 SE Clip
            }
          
            audioMixerPlayable.SetInputCount(0);
            audioMixerPlayable.AddInput(audioClipPlayable, 0, 1);
            seOut.SetSourcePlayable(audioClipPlayable);
            seGraph.Play();
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }

    public void StopSE()
    {
        if (seGraph.IsValid()) seGraph.Stop();
    }

    private void SetPlayAudioWeight(AudioMixerPlayable audioMixerPlayable, AudioClip audioClip, float weight)
    {
        for (int i = 0; i < audioMixerPlayable.GetInputCount(); i++)
        {
            var oldPlayable = audioMixerPlayable.GetInput(i);
            if (oldPlayable.IsValid())
            {
                var audioClipPlayable = (AudioClipPlayable)oldPlayable;
                if (audioClipPlayable.GetClip() == audioClip)
                {
                    audioMixerPlayable.SetInputWeight(i, weight);
                    break;
                }
            }
          
        }
    }

    Dictionary<PlayableGraph, IEnumerator> lerpIEnumeratorDic = new Dictionary<PlayableGraph, IEnumerator>();

    private void PlayAudio(PlayableGraph playableGraph, AudioMixerPlayable audioMixer, Dictionary<string, AudioMixerPlayable> childMixers, AudioClip audioClip, bool loop = false,
        AudioClearType audioClearType = AudioClearType.NoClear, float weight = 1, bool isLerp = false, string Group = "Default")
    {
        if (!childMixers.TryGetValue(Group, out var childMixer))
        {
            childMixer = AudioMixerPlayable.Create(playableGraph);
            childMixers.Add(Group, childMixer);
            audioMixer.AddInput(childMixer, 0, 1);
        }

        var count = childMixer.GetInputCount();
        bool haveAudio = false;
        for (int i = count - 1; i >= 0; i--)
        {
            if (TryGetClip(childMixer.GetInput(i), out var clip) && clip == audioClip)
            { 
                childMixer.SetInputWeight(i, weight);
                haveAudio = true;
                break;
            }
        }
        if (haveAudio)
        {
            return;
        }

        if (!isLerp)
        {
            TryEndLerpAudioIEnumerator(playableGraph);
            AudioClipPlayable audioClipPlayable = AudioClipPlayable.Create(playableGraph, audioClip, loop);

            if (count > 0)
            {
                if (audioClearType == AudioClearType.All)
                {
                    var toDestroy = new List<Playable>();
                    for (int i = count - 1; i >= 0; i--)
                    {
                        var oldPlayable = childMixer.GetInput(i);
                        playableGraph.Disconnect(childMixer, i);
                        toDestroy.Add(oldPlayable);
                    }

                    foreach (var p in toDestroy)
                        // 这里是 BGM/BGS 的通用通路，选择“卸载”
                        DestroyPlayable(p, true);

                    if (audioClip != null)
                    {
                        var newPlayable = AudioClipPlayable.Create(playableGraph, audioClip, loop);
                        ReplaceInput(childMixer, 0, playableGraph, newPlayable, weight); // ✅ 用安全方法
                    }
                    else
                    {
                        childMixer.SetInputCount(0);
                    }
                }
                else
                {
                    if (audioClip != null)
                        childMixer.AddInput(audioClipPlayable, 0, weight);
                }
            }
            else
            {
                if (audioClip != null)
                    childMixer.AddInput(audioClipPlayable, 0, weight);
            }
        }
        else
        {
            if (count > 0)
            {
                if (audioClearType == AudioClearType.All)
                {
                    if (lerpIEnumeratorDic.TryGetValue(playableGraph, out var enumerator))
                    {
                        GameObjectCurveController.instance.StopIEnumerator(enumerator);
                        lerpIEnumeratorDic.Remove(playableGraph);
                    }

                    enumerator = LerpAudio(childMixer, audioClip);
                    lerpIEnumeratorDic.Add(playableGraph, enumerator);
                    GameObjectCurveController.instance.StartIEnumerator(enumerator);

                    IEnumerator LerpAudio(AudioMixerPlayable childMixerLocal, AudioClip audioClipLocal)
                    {
                        AudioClipPlayable audioClipPlayable = default(AudioClipPlayable);
                        if (audioClipLocal != null)
                        {
                            audioClipPlayable = AudioClipPlayable.Create(playableGraph, audioClipLocal, loop);
                            childMixerLocal.AddInput(audioClipPlayable, 0);
                        }

                        var cnt = childMixerLocal.GetInputCount();
                        List<float> startWeights = new List<float>();
                        if (cnt > 0)
                        {
                            for (var i = 0; i < cnt; i++)
                            {
                                startWeights.Add(childMixerLocal.GetInputWeight(i));
                            }
                        }

                        float timeValue = 0;
                        while (timeValue < 1)
                        {
                            timeValue += Time.deltaTime;

                            if (!childMixerLocal.IsValid()) yield break;

                            if (cnt > 1)
                            {
                                for (var i = 0; i < cnt - 1; i++)
                                {
                                    childMixerLocal.SetInputWeight(i, startWeights[i] * (1 - timeValue));
                                }
                            }

                            if (audioClipLocal != null)
                                childMixerLocal.SetInputWeight(cnt - 1, timeValue * weight);

                            yield return null;
                        }

                        //var toDestroy = new List<Playable>();
                        // --- 淡入结束后 ---
                        var oldCount = childMixerLocal.GetInputCount();


                        var newIndex = -1;
                        for (var i = 0; i < oldCount; i++)
                        {
                            var p = childMixerLocal.GetInput(i);
                            if (p.Equals(audioClipPlayable))
                            {
                                newIndex = i;
                                break;
                            }
                        }

                        for (var i = oldCount - 1; i >= 0; i--)
                        {
                            var oldPlayable = childMixerLocal.GetInput(i);
                            if (!oldPlayable.IsValid()) continue;

                            // 跳过“新”的 playable，避免误删
                            if (newIndex == i) continue;

                            childMixerLocal.DisconnectInput(i);
                            DestroyPlayable(oldPlayable, true); // 卸载旧 BGM
                        }

                        if (audioClipLocal != null && audioClipPlayable.IsValid())
                        {
                            // ✅ 直接保留新曲子，强制它在 slot 0
                            childMixerLocal.DisconnectInput(newIndex);
                            if (newIndex != 0) playableGraph.Connect(audioClipPlayable, 0, childMixerLocal, 0);
                            childMixerLocal.SetInputWeight(0, weight);
                        }
                        else
                        {
                            childMixerLocal.SetInputCount(0);
                        }
 
                        lerpIEnumeratorDic.Remove(playableGraph);
                        if (audioClipPlayable.IsValid())
                            childMixerLocal.SetInputWeight(childMixerLocal.GetInputCount() - 1, weight);

                    }
                }
                else
                {
                    if (audioClip != null)
                    {
                        var audioClipPlayable = AudioClipPlayable.Create(playableGraph, audioClip, loop);
                        childMixer.AddInput(audioClipPlayable, 0, weight);
                    }
                }
            }
            else if (audioClip != null)
            {
                AudioClipPlayable audioClipPlayable = AudioClipPlayable.Create(playableGraph, audioClip, loop);
                childMixer.AddInput(audioClipPlayable, 0, weight);
            }
        }

        if (!playableGraph.IsPlaying())
        {
            playableGraph.Play();
        }
    }

 

    /// <summary>
    ///     安全销毁 Playable；当且仅当是 AudioClipPlayable 且需要时卸载对应 AudioClip。
    /// </summary>
    private void DestroyPlayable(Playable playable, bool unloadClip)
    {
        if (!playable.IsValid())
            return;

        if (unloadClip && playable.IsPlayableOfType<AudioClipPlayable>())
        {
            var acp = (AudioClipPlayable)playable;
            var oldAudioClip = acp.GetClip();
            if (oldAudioClip != null)
                try
                {
                    Resources.UnloadAsset(oldAudioClip);
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"UnloadAsset failed: {oldAudioClip.name} - {e.Message}");
                }
        }

        playable.Destroy();
    }

    public void SetBGMGroupValue(string group, float value)
    {
        if (bgmMixerDic.TryGetValue(group, out var audioMixerPlayable))
        {
            bgmMixer.SetInputWeight(audioMixerPlayable, value);
        }
    }

    public void SetBGSGroupValue(string group, float value)
    {
        if (bgsMixerDic.TryGetValue(group, out var audioMixerPlayable))
        {
            bgsMixer.SetInputWeight(audioMixerPlayable, value);
        }
    }

    Dictionary<PlayableGraph, IEnumerator> lerpAudioIEnumeratorDic = new Dictionary<PlayableGraph, IEnumerator>();

    private void SetLerpAudioIEnumerator(PlayableGraph playableGraph, IEnumerator enumerator)
    {
        if (lerpAudioIEnumeratorDic.TryGetValue(playableGraph, out var oldEnumerator))
        {
            while (oldEnumerator.MoveNext())
            {
            }
        }

        lerpAudioIEnumeratorDic[playableGraph] = enumerator;
    }
    void TryEndLerpAudioIEnumerator(PlayableGraph playableGraph)
    {
        if (lerpAudioIEnumeratorDic.TryGetValue(playableGraph, out var oldEnumerator))
        {
            while (oldEnumerator.MoveNext()) { }
        }
    }

    private void ReplaceInput(AudioMixerPlayable mixer, int index, PlayableGraph graph, Playable newPlayable,
        float weight, bool unloadOldClip = true)
    {
        if (!mixer.IsValid() || !graph.IsValid() || !newPlayable.IsValid()) return;

        if (index < mixer.GetInputCount())
        {
            var oldPlayable = mixer.GetInput(index);
            if (oldPlayable.IsValid())
            {
                mixer.DisconnectInput(index);
                DestroyPlayable(oldPlayable, unloadOldClip);
            }
        }

        if (index >= mixer.GetInputCount()) mixer.SetInputCount(index + 1);

        graph.Connect(newPlayable, 0, mixer, index);
        mixer.SetInputWeight(index, weight);

        // 保底：确保 Graph 在 Play
        if (!graph.IsPlaying()) graph.Play();
    }

    private void PlayAudio(PlayableGraph playableGraph, AudioMixerPlayable audioMixer, Dictionary<string, AudioMixerPlayable> childMixers, List<AudioPlayData> audioClips,
       AudioClearType audioClearType = AudioClearType.NoClear, bool isLerp = false, string Group = "Default")
    {
        if (!childMixers.TryGetValue(Group, out var childMixer))
        {
            childMixer = AudioMixerPlayable.Create(playableGraph);
            childMixers.Add(Group, childMixer);
            audioMixer.AddInput(childMixer, 0, 1);
        }

        var count = childMixer.GetInputCount();

        for (int i = count - 1; i >= 0; i--)
        {
            var playable = childMixer.GetInput(i);
            if (TryGetClip(playable, out var _audioClip))
            {
                for (var index = audioClips.Count - 1; index >= 0; index--)
                {
                    var clip = audioClips[index].audioClip;

                    if (_audioClip == clip)
                    {
                        childMixer.SetInputWeight(i, audioClips[index].weight);
                        audioClips.RemoveAt(index);
                        break;
                    }

                    if (audioClips.Count == 0) return;
                }
            }  
        }

        if (!isLerp)
        {
            TryEndLerpAudioIEnumerator(playableGraph);
            if (count > 0)
            { 
                if (audioClearType == AudioClearType.All)
                {
                    var toDestroy = new List<Playable>();
                    for (var i = count - 1; i >= 0; i--) // 修复：原来写成 <= 0
                    {
                        var oldPlayable = childMixer.GetInput(i);
                        playableGraph.Disconnect(childMixer, i);
                        toDestroy.Add(oldPlayable);
                    }

                    for (var i = 0; i < toDestroy.Count; i++)
                        DestroyPlayable(toDestroy[i], true); // 多 BGS 情况：清理旧轨并卸载

                    childMixer.SetInputCount(0);
                    if (audioClips.Count > 0)
                    {
                        for (int i = 0; i < audioClips.Count; i++)
                        {
                            AudioClipPlayable audioClipPlayable = AudioClipPlayable.Create(playableGraph, audioClips[i].audioClip, audioClips[i].loop);
                            childMixer.AddInput(audioClipPlayable, 0, audioClips[i].weight);
                        }
                    }
                }
                else
                {
                    if (audioClips.Count > 0)
                    {
                        childMixer.SetInputCount(count + audioClips.Count);
                        for (int i = count - 1; i >= 0; i--)
                        {
                            var playable = childMixer.GetInput(i);
                            float weight = childMixer.GetInputWeight(i);
                            playableGraph.Connect(playable, 0, childMixer, i + audioClips.Count);
                            childMixer.SetInputWeight(i + audioClips.Count, weight);
                        }
                        for (int i = 0; i < audioClips.Count; i++)
                        {
                            AudioClipPlayable audioClipPlayable = AudioClipPlayable.Create(playableGraph, audioClips[i].audioClip, audioClips[i].loop);
                            playableGraph.Connect(audioClipPlayable, 0, childMixer, i);
                            childMixer.SetInputWeight(i, audioClips[i].weight);
                        }
                    }
                }
            }
            else
            {
                if (audioClips.Count > 0)
                {
                    for (int i = 0; i < audioClips.Count; i++)
                    {
                        AudioClipPlayable audioClipPlayable = AudioClipPlayable.Create(playableGraph, audioClips[i].audioClip, audioClips[i].loop);
                        childMixer.AddInput(audioClipPlayable, 0, audioClips[i].weight);
                    }
                }
            }
        }
        else
        {
            if (count > 0)
            {
                if (audioClearType == AudioClearType.All)
                {
                    if (audioClips.Count > 0)
                    {
                        for (int i = 0; i < audioClips.Count; i++)
                        {
                            AudioClipPlayable audioClipPlayable = AudioClipPlayable.Create(playableGraph, audioClips[i].audioClip, audioClips[i].loop);
                            childMixer.AddInput(audioClipPlayable, 0, audioClips[i].weight);
                        }
                    }
                    List<float> startWeights = new List<float>();
                    for (int i = 0; i < count; i++)
                    {
                        startWeights.Add(childMixer.GetInputWeight(i));
                    }
                    var enumerator = LerpAudio(startWeights);
                    GameObjectCurveController.instance.StartIEnumerator(enumerator);
                    SetLerpAudioIEnumerator(playableGraph, enumerator);

                    IEnumerator LerpAudio(List<float> startWeightsLocal)
                    {
                        float timeValue = 0;
                        while (timeValue < 1)
                        {
                            if (!childMixer.IsValid()) yield break;

                            for (var i = 0; i < startWeightsLocal.Count; i++)
                            {
                                childMixer.SetInputWeight(i, startWeightsLocal[i] * (1 - timeValue));
                            }
                            if (audioClips.Count > 0)
                            {
                                for (int i = 0; i < audioClips.Count; i++)
                                {
                                    childMixer.SetInputWeight(count + i, timeValue * audioClips[i].weight);
                                }
                            }

                            timeValue += Time.deltaTime * 0.25f;
                            yield return null;
                        }
                     

                        
                        int mixerCount = childMixer.GetInputCount();
                        if (audioClips.Count > 0)
                        {
                            var toDestroy = new List<Playable>();
                            for (int i = 0; i < audioClips.Count; i++)
                            {
                                AudioClipPlayable audioClipPlayable = AudioClipPlayable.Create(playableGraph, audioClips[i].audioClip, audioClips[i].loop);

                                if (mixerCount > i)
                                {
                                    // ✅ 用安全替换，内部会先 Disconnect 再 Connect，并销毁旧的
                                    ReplaceInput(childMixer, i, playableGraph, audioClipPlayable, audioClips[i].weight);
                                }
                                else
                                {
                                    childMixer.AddInput(audioClipPlayable, 0, audioClips[i].weight);
                                }
                            }

                            for (var i = 0; i < toDestroy.Count; i++) DestroyPlayable(toDestroy[i], true);
                        }
                        lerpAudioIEnumeratorDic.Remove(playableGraph);
                        
                    }
                }
                else
                {
                    if (audioClips.Count > 0)
                    {
                        childMixer.SetInputCount(count + audioClips.Count);

                        for (int i = count - 1; i >= 0; i--)
                        {
                            var playable = childMixer.GetInput(i);
                            var weight0 = childMixer.GetInputWeight(i);
                            playableGraph.Connect(playable, 0, childMixer, i + audioClips.Count);
                            childMixer.SetInputWeight(i + audioClips.Count, weight0);
                        }
                        for (int i = 0; i < audioClips.Count; i++)
                        {
                            AudioClipPlayable audioClipPlayable = AudioClipPlayable.Create(playableGraph, audioClips[i].audioClip, audioClips[i].loop);
                            playableGraph.Connect(audioClipPlayable, 0, childMixer, i);
                            childMixer.SetInputWeight(i, audioClips[i].weight);
                        }
                        var enumerator = LerpAudio();
                        GameObjectCurveController.instance.StartIEnumerator(enumerator);
                        SetLerpAudioIEnumerator(playableGraph, enumerator);
                        IEnumerator LerpAudio()
                        {
                            float timeValue = 0;
                            while (timeValue < 1)
                            {
                                if (!childMixer.IsValid()) yield break;

                                timeValue += Time.deltaTime;
                                for (int i = 0; i < audioClips.Count; i++)
                                {
                                    childMixer.SetInputWeight(i, timeValue * audioClips[i].weight);
                                }

                                yield return null;
                            }
                            
                            
                            lerpAudioIEnumeratorDic.Remove(playableGraph);
                        }
                    }
                }
            }
            else
            {
                if (audioClips.Count > 0)
                {
                    for (int i = 0; i < audioClips.Count; i++)
                    {
                        AudioClipPlayable audioClipPlayable = AudioClipPlayable.Create(playableGraph, audioClips[i].audioClip, audioClips[i].loop);
                        childMixer.AddInput(audioClipPlayable, 0, audioClips[i].weight);
                    }
                    var enumerator = LerpAudio();
                    GameObjectCurveController.instance.StartIEnumerator(enumerator);
                    SetLerpAudioIEnumerator(playableGraph, enumerator);
                    IEnumerator LerpAudio()
                    {
                        float timeValue = 0;
                        while (timeValue < 1)
                        {
                            if (!childMixer.IsValid()) yield break;

                            timeValue += Time.deltaTime;
                            for (int i = 0; i < audioClips.Count; i++)
                            {
                                childMixer.SetInputWeight(i, timeValue * audioClips[i].weight);
                            }

                            yield return null;
                        }
                        lerpAudioIEnumeratorDic.Remove(playableGraph);
                    }
                }
            }
        }

        if (!playableGraph.IsPlaying())
        {
            playableGraph.Play();
        }
    }

    private void PlayME(AudioClip audioClip, bool loop = false, AudioClearType audioClearType = AudioClearType.NoClear, float weight = 1, bool isLerp = false, string Group = "Default")
    {
        if (audioClip == null)
        {
            if (GameDataManager.instance.GlobalData.debug)
                Debug.Log($"PlayMe: null");
        }
        else
        {
            if (GameDataManager.instance.GlobalData.debug)
                Debug.Log($"PlayMe:{audioClip.name}");
        }
        PlayAudio(meGraph, meMixer, meMixerDic, audioClip, loop, audioClearType, weight, isLerp, Group);
    }

    public void StopME()
    {
        if (meGraph.IsValid()) meGraph.Stop();
    }

    public void PlayBGM(AudioClip audioClip, bool loop = true, AudioClearType audioClearType = AudioClearType.NoClear, float weight = 1, bool isLerp = false, string Group = "Default")
    {
        if (audioClip == null)
        {
            if (GameDataManager.instance.GlobalData.debug)
                Debug.Log($"PlayBgm: null");
        }
        else
        {
            if (GameDataManager.instance.GlobalData.debug)
                Debug.Log($"PlayBgm:{audioClip.name}");
        }

        PlayAudio(bgmGraph, bgmMixer, bgmMixerDic, audioClip, loop, audioClearType, weight, isLerp, Group);
    }

    public void StopBgm()
    {
        if (bgmGraph.IsValid()) bgmGraph.Stop();
    }

    private void PlayBGS(AudioClip audioClip, bool loop = true, AudioClearType audioClearType = AudioClearType.NoClear, float weight = 1, bool isLerp = false, string Group = "Default")
    {
        if (audioClip == null)
        {
            if (GameDataManager.instance.GlobalData.debug)
                Debug.Log($"PlayBGS: null");
        }
        else
        {
            // Debug.Log($"PlayBGS:{audioClip.name}");
        }
        PlayAudio(bgsGraph, bgsMixer, bgsMixerDic, audioClip, loop, audioClearType, weight, isLerp, Group);
    }

    private void PlayBGS(List<AudioPlayData> audioClips, AudioClearType audioClearType = AudioClearType.NoClear, bool isLerp = false, string Group = "Default")
    {
        PlayAudio(bgsGraph, bgsMixer, bgsMixerDic, audioClips, audioClearType, isLerp, Group);
    }

    public void StopBGS()
    {
        if (bgsGraph.IsValid()) bgsGraph.Stop();
    }

    public void SetMasterVolume(float volume)    // 控制主音量的函数
    {
        audioMixer.SetFloat("MasterVolume", -40 + 40 * Mathf.Clamp01(volume));
    }

    public void SetBGMVolume(float volume)    // 控制背景音乐音量的函数
    {
        audioMixer.SetFloat("BGMVolume", -40 + 40 * Mathf.Clamp01(volume));
    }

    public void SetSEVolume(float volume)    // 控制音效音量的函数
    {
        audioMixer.SetFloat("SEVolume", -40 + 40 * Mathf.Clamp01(volume));
    }
}
