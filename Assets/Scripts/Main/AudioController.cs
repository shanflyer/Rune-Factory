using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Playables;

public enum AudioClearType
{
    All, Oldest, NoClear
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
    Default, Battle, 
}
public enum SEGroup
{
    Default, UI
}
public delegate void SetAudioAction(AudioClip audioClip);

public class AudioController : Singleton<AudioController>
{
    public override async void Init()
    {
        base.Init();
        audioMixer = await ExtensionsResources.LoadResourceAsync<AudioMixer>("Audio/AudioMixer");

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
        StopBgm();
        StopBGS();
        StopME();
        StopSE();
        meMixerDic.Clear();
        bgsMixerDic.Clear();
        bgmMixerDic.Clear();
        bgmGraph.Destroy();
        bgsGraph.Destroy();
        seGraph.Destroy();
        meGraph.Destroy();

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

    public void SetAudioSource(GameObject audioObj)
    {
        var BGM = audioObj.transform.Find("BGM");
        var BGS = audioObj.transform.Find("BGS");
        var ME = audioObj.transform.Find("ME");
        var SE = audioObj.transform.Find("SE");

        if (BGM)
        {
            var bgmAudioSource = BGM.GetComponent<AudioSource>();
            bgmGraph = PlayableGraph.Create("BGM");
            bgmOut = AudioPlayableOutput.Create(bgmGraph, "BGM", bgmAudioSource);
            bgmMixer = AudioMixerPlayable.Create(bgmGraph);
            bgmOut.SetSourcePlayable(bgmMixer);
        }
        if (BGS)
        {
            var bgsAudioSource = BGS.GetComponent<AudioSource>();
            bgsGraph = PlayableGraph.Create("BGS");
            bgsOut = AudioPlayableOutput.Create(bgsGraph, "BGS", bgsAudioSource);
            bgsMixer = AudioMixerPlayable.Create(bgsGraph);
            bgsOut.SetSourcePlayable(bgsMixer);
        }

        if (ME)
        {
            var meAudioSource = ME.GetComponent<AudioSource>();
            meGraph = PlayableGraph.Create("ME");
            meOut = AudioPlayableOutput.Create(meGraph, "ME", meAudioSource);
            meMixer = AudioMixerPlayable.Create(meGraph);
            meOut.SetSourcePlayable(meMixer);
        }
        if (SE)
        {
            var seAudioSource = SE.GetComponent<AudioSource>();
            seGraph = PlayableGraph.Create("SE");
            seOut = AudioPlayableOutput.Create(seGraph, "SE", seAudioSource);
            seMixer = AudioMixerPlayable.Create(seGraph);
            seOut.SetSourcePlayable(seMixer);
        }
    }

    private AudioMixer audioMixer;

    private PlayableGraph bgmGraph, bgsGraph, meGraph, seGraph;
    private AudioPlayableOutput bgmOut, bgsOut, meOut, seOut;
    private AudioMixerPlayable bgmMixer, bgsMixer, meMixer,seMixer;

    private Dictionary<string, AudioMixerPlayable> bgmMixerDic = new Dictionary<string, AudioMixerPlayable>();
    private Dictionary<string, AudioMixerPlayable> bgsMixerDic = new Dictionary<string, AudioMixerPlayable>();
    private Dictionary<string, AudioMixerPlayable> meMixerDic = new Dictionary<string, AudioMixerPlayable>();
    private Dictionary<string, AudioMixerPlayable> seMixerDic = new Dictionary<string, AudioMixerPlayable>();

    private string nowBGM;
    private Dictionary<string, string> nowBGSs=new Dictionary<string, string>();
    private Dictionary<string, float> bgsWeights = new Dictionary<string, float>();
    private float bgmWeight;
  
    public async void PlayAudio(SE se, bool loop = false,string Group="Default")
    {
        AudioClip audioClip = await GameSourceManager.instance.GetAudioClip(GameCommon.AddString(DataPath.SEPath, se.ToString()));
        PlaySE(audioClip, loop, Group);
    }

    public async void PlayAudio(string se, bool loop = false,string Group="Default")
    {
        if (string.IsNullOrEmpty(se))
        {
            return;
        }
        AudioClip audioClip = await GameSourceManager.instance.GetAudioClip(GameCommon.AddString(DataPath.SEPath, se));
        PlaySE(audioClip, loop, Group);
    }

    public async void PlayAudio(ME me, bool loop = false, AudioClearType audioClearType = AudioClearType.NoClear, float weight = 1, bool isLerp = false, string Group = "Default")
    {
        AudioClip audioClip = await GameSourceManager.instance.GetAudioClip(GameCommon.AddString(DataPath.MEPath, me.ToString()));
        PlayME(audioClip, loop, audioClearType, weight, isLerp, Group);
    }
    public void PlayAudioME(AudioClip audioClip, bool loop = false, AudioClearType audioClearType = AudioClearType.All, float weight = 1, bool isLerp = false, string Group = "Default")
    { 
        PlayME(audioClip, loop, audioClearType, weight, isLerp, Group);
    }
    public async void PlayAudio(BGM bgm, bool loop = true, AudioClearType audioClearType = AudioClearType.NoClear, float weight = 1, bool isLerp = false, string Group = "Default")
    {
        if (bgm == BGM.NULL)
        {
            if (nowBGM != "NULL")
            {
                bgmWeight = weight;
                PlayBGM(null, loop, audioClearType, weight, isLerp, Group);
                nowBGM = bgm.ToString();
            }
        }
        else
        {
            string bgmStr = bgm.ToString();
            var strs = bgmStr.Split("_");
            bgmStr = strs[strs.Length - 1];
            if (bgmStr != nowBGM)
            {
                bgmWeight = weight;
                nowBGM = bgmStr;
                AudioClip audioClip = await
                   GameSourceManager.instance.GetAudioClip(GameCommon.AddString(DataPath.BGMPath, bgm.ToString()));
                PlayBGM(audioClip, loop, audioClearType, weight, isLerp, Group);
            }
            else if (bgmWeight != weight)
            {
                if (bgmMixerDic.TryGetValue(Group, out var audioMixerPlayable))
                {
                    AudioClip audioClip = await
                       GameSourceManager.instance.GetAudioClip(GameCommon.AddString(DataPath.BGMPath, bgm.ToString()));
                    SetPlayAudioWeight(audioMixerPlayable, audioClip, weight);
                }
                bgmWeight = weight;
            }
        }
    }
    public void PlayAudioBGS(AudioClip bgs, bool loop = true, AudioClearType audioClearType = AudioClearType.NoClear, float weight = 1,
        bool isLerp = false, string Group = "Default")
    {
        if (nowBGSs.TryGetValue(Group, out var nowBGS))
        {
            nowBGS = "NULL";
        }
        if(bgsWeights.TryGetValue(Group,out var bgsWeight))
        {
            bgsWeight = 0;
        }

        if ((bgs == null && nowBGS != "NULL") || (bgs != null && nowBGM != bgs.name))
        {
            bgsWeight = weight;
            nowBGM = bgs.name;
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
            nowBGM = bgm.name;
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
                AudioClip audioClip = await
                GameSourceManager.instance.GetAudioClip(GameCommon.AddString(DataPath.BGSPath, bgs.ToString()));
                PlayBGS(audioClip, loop, audioClearType, weight, isLerp, Group);
                bgsWeights[Group] = bgsWeight;
                nowBGSs[Group] = nowBGS;
            }
            else if (bgsWeight != weight)
            {
                if (bgsMixerDic.TryGetValue(Group, out var audioMixerPlayable))
                {
                    AudioClip audioClip = await
                               GameSourceManager.instance.GetAudioClip(GameCommon.AddString(DataPath.BGSPath, bgs.ToString()));
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
            BGS BGS  = (BGS)(int)(bgs[bgs.Count - 1].x);
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

    public void PlaySE(AudioClip audioClip, bool loop = false,string Group="Default")
    {
        try
        {
            AudioClipPlayable audioClipPlayable = AudioClipPlayable.Create(seGraph, audioClip, loop);
            if(!seMixerDic.TryGetValue(Group,out var audioMixerPlayable))
            {
                audioMixerPlayable = AudioMixerPlayable.Create(seGraph,1);
                seMixerDic.Add(Group, audioMixerPlayable) ;
                seMixer.AddInput(audioMixerPlayable, 0, 1);
            }
            AudioClipPlayable oldAudioClip = (AudioClipPlayable)PlayableExtensions.GetInput(audioMixerPlayable, 0);
            if (!oldAudioClip.IsNull())
            {
                oldAudioClip.Destroy();
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
        seGraph.Stop();
    }

    private void SetPlayAudioWeight(AudioMixerPlayable audioMixerPlayable, AudioClip audioClip, float weight)
    {
        for (int i = 0; i < audioMixerPlayable.GetInputCount(); i++)
        {
            AudioClipPlayable audioClipPlayable = (AudioClipPlayable)audioMixerPlayable.GetInput(i);
            if (audioClipPlayable.GetClip() == audioClip)
            {
                audioMixerPlayable.SetInputWeight(i, weight);
                break;
            }
        }
    }

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
        if (!isLerp)
        {
            AudioClipPlayable audioClipPlayable = AudioClipPlayable.Create(playableGraph, audioClip, loop);

            if (count > 0)
            {
                if (audioClearType == AudioClearType.All)
                {
                    for (int i = count - 1; i >= 0; i--)
                    {
                        playableGraph.Disconnect(childMixer, i);
                    }
                    if (audioClip != null)
                    {
                        childMixer.SetInputCount(1);
                        playableGraph.Connect(audioClipPlayable, 0, childMixer, 0);
                        childMixer.SetInputWeight(0, weight);
                    }
                    else
                    {
                        childMixer.SetInputCount(0);
                    }
                }
                else if (audioClearType == AudioClearType.Oldest)
                {
                    if (audioClip != null)
                    {
                        if (count > 1)
                        {
                            for (int i = count - 1; i >= 0; i--)
                            {
                                var playable = childMixer.GetInput(i);
                                float oldWeight = childMixer.GetInputWeight(i);
                                playableGraph.Disconnect(childMixer, i);
                                playableGraph.Connect(playable, 0, childMixer, i + 1);
                                childMixer.SetInputWeight(i + 1, oldWeight);
                            }
                        }
                        playableGraph.Connect(audioClipPlayable, 0, childMixer, 0);
                        childMixer.SetInputWeight(0, weight);
                    }
                    else
                    {
                        playableGraph.Disconnect(childMixer, count - 1);
                        childMixer.SetInputCount(count - 1);
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
            AudioClipPlayable audioClipPlayable = AudioClipPlayable.Create(playableGraph, audioClip, loop);
            if (count > 0)
            {
                if (audioClip != null)
                    childMixer.AddInput(audioClipPlayable, 0, 0);
                if (audioClearType == AudioClearType.All)
                {
                    List<float> startWeights = new List<float>();
                    for (int i = 0; i < count; i++)
                    {
                        startWeights.Add(childMixer.GetInputWeight(i));
                    }
                    GameObjectCurveController.instance.StartIEnumerator(LerpAudio());
                    IEnumerator LerpAudio()
                    {
                        float timeValue = 0;
                        while (timeValue < 1)
                        {
                            timeValue += Time.deltaTime * 0.5f;
                            for (int i = 0; i < count; i++)
                            {
                                childMixer.SetInputWeight(i, startWeights[i] * (1 - timeValue));
                            }
                            if (audioClip != null)
                                childMixer.SetInputWeight(count, timeValue * weight);

                            yield return 0;
                        }
                        for (int i = 0; i < childMixer.GetInputCount(); i++)
                        {
                            childMixer.DisconnectInput(i);
                        }
                        childMixer.SetInputCount(0);
                        if (audioClip != null)
                        {
                            childMixer.AddInput(audioClipPlayable, 0, weight);
                        }
                    }
                }
                else if (audioClearType == AudioClearType.Oldest)
                {
                    float oldWeight = childMixer.GetInputWeight(0);
                    List<float> oldWeights = new List<float>();
                    for (int i = 1; i < count; i++)
                    {
                        oldWeights.Add(childMixer.GetInputWeight(i));
                    }
                    GameObjectCurveController.instance.StartIEnumerator(LerpAudio());
                    IEnumerator LerpAudio()
                    {
                        float timeValue = 0;
                        while (timeValue < 1)
                        {
                            timeValue += Time.deltaTime * 0.5f;
                            childMixer.SetInputWeight(0, oldWeight * (1 - timeValue));
                            childMixer.SetInputWeight(1, timeValue * weight);
                            yield return 0;
                        }

                        if (audioClip != null)
                        {
                            for (int i = count - 1; i >= 0; i--)
                            {
                                var playable = childMixer.GetInput(i);
                                float oldWeight = childMixer.GetInputWeight(i);
                                playableGraph.Disconnect(childMixer, i);
                                playableGraph.Connect(playable, 0, childMixer, i + 1);
                                childMixer.SetInputWeight(i + 1, oldWeight);
                            }
                            playableGraph.Connect(audioClipPlayable, 0, childMixer, 0);
                            childMixer.SetInputWeight(0, weight);
                        }
                        else
                        {
                            playableGraph.Disconnect(childMixer, count - 1);
                            childMixer.SetInputCount(count - 1);
                        }
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

        if (!playableGraph.IsPlaying())
        {
            playableGraph.Play();
        }
        int rootCount = playableGraph.GetRootPlayableCount();
        for (int i = 0; i < rootCount; i++)
        {
            var playable = playableGraph.GetRootPlayable(i);
            try
            {
                var audioPlayable = (AudioClipPlayable)playable;
                if (audioPlayable.IsNull())
                {
                }
                else
                {
                    audioPlayable.Destroy();
                }
            }
            catch
            {
            }
        }
    }

    public void SetBGMGroupValue(string group,float value)
    {
        if(bgmMixerDic.TryGetValue(group,out var audioMixerPlayable))
        {
            bgmMixer.SetInputWeight(audioMixerPlayable, value);
        }
    }
    public void SetBGSGroupValue(string group,float value)
    {
        if(bgsMixerDic.TryGetValue(group,out var audioMixerPlayable))
        {
            bgsMixer.SetInputWeight(audioMixerPlayable, value);
        }
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
        if (!isLerp)
        {
            if (count > 0)
            {
                if (audioClearType == AudioClearType.All)
                {
                    for (int i = count - 1; i <= 0; i--)
                    {
                        playableGraph.Disconnect(childMixer, i);
                    }
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
                else if (audioClearType == AudioClearType.Oldest)
                {
                    if (count > audioClips.Count)
                    {
                        for (int i = 0; i < count - audioClips.Count; i++)
                        {
                            var playable = childMixer.GetInput(i);
                            float weight = childMixer.GetInputWeight(i);
                            playableGraph.Connect(playable, 0, childMixer, i + audioClips.Count);
                            childMixer.SetInputWeight(i + audioClips.Count, weight);
                        }
                        if (audioClips.Count > 0)
                        {
                            for (int i = 0; i < audioClips.Count; i++)
                            {
                                AudioClipPlayable audioClipPlayable = AudioClipPlayable.Create(playableGraph, audioClips[i].audioClip, audioClips[i].loop);
                                playableGraph.Connect(audioClipPlayable, 0, childMixer, i);
                                childMixer.SetInputWeight(i, audioClips[i].weight);
                            }
                        }
                    }
                    else
                    {
                        childMixer.SetInputCount(audioClips.Count);
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
                    GameObjectCurveController.instance.StartIEnumerator(LerpAudio());
                    IEnumerator LerpAudio()
                    {
                        float timeValue = 0;
                        while (timeValue < 1)
                        {
                            timeValue += Time.deltaTime * 0.5f;
                            for (int i = 0; i < count; i++)
                            {
                                childMixer.SetInputWeight(i, startWeights[i] * (1 - timeValue));
                            }
                            if (audioClips.Count > 0)
                            {
                                for (int i = 0; i < audioClips.Count; i++)
                                {
                                    childMixer.SetInputWeight(count + i, timeValue * audioClips[i].weight);
                                }
                            }
                            yield return 0;
                        }

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
                }
                else if (audioClearType == AudioClearType.Oldest)
                {
                    if (audioClips.Count > 0)
                    {
                        for (int i = 0; i < audioClips.Count; i++)
                        {
                            AudioClipPlayable audioClipPlayable = AudioClipPlayable.Create(playableGraph, audioClips[i].audioClip, audioClips[i].loop);
                            childMixer.AddInput(audioClipPlayable, 0, audioClips[i].weight);
                        }
                    }
                    List<float> oldWeights = new List<float>();
                    for (int i = 1; i < count; i++)
                    {
                        oldWeights.Add(childMixer.GetInputWeight(i));
                    }
                    List<AudioClipPlayable> audioClipPlayables = new List<AudioClipPlayable>();
                    if (audioClips.Count > 0)
                    {
                        for (int i = 0; i < audioClips.Count; i++)
                        {
                            AudioClipPlayable audioClipPlayable = AudioClipPlayable.Create(playableGraph, audioClips[i].audioClip, audioClips[i].loop);
                            childMixer.AddInput(audioClipPlayable, 0, audioClips[i].weight);
                            audioClipPlayables.Add(audioClipPlayable);
                        }
                    }
                    GameObjectCurveController.instance.StartIEnumerator(LerpAudio());
                    IEnumerator LerpAudio()
                    {
                        float timeValue = 0;
                        while (timeValue < 1)
                        {
                            timeValue += Time.deltaTime * 0.5f;

                            for (int i = 0; i < audioClips.Count; i++)
                            {
                                if (i < count)
                                {
                                    childMixer.SetInputWeight(0, oldWeights[i] * (1 - timeValue));
                                }
                                childMixer.SetInputWeight(i + count, timeValue * audioClips[i].weight);
                            }
                            yield return 0;
                        }
                        if (count > audioClips.Count)
                        {
                            for (int i = count - audioClips.Count; i >= 0; i--)
                            {
                                var playable = childMixer.GetInput(i);
                                float weight = childMixer.GetInputWeight(i);
                                playableGraph.Connect(playable, 0, childMixer, i + audioClips.Count);
                                childMixer.SetInputWeight(i + audioClips.Count, weight);
                            }
                        }
                        if (audioClips.Count > 0)
                        {
                            for (int i = 0; i < audioClips.Count; i++)
                            {
                                AudioClipPlayable audioClipPlayable = audioClipPlayables[i];
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
                        GameObjectCurveController.instance.StartIEnumerator(LerpAudio());
                        IEnumerator LerpAudio()
                        {
                            float timeValue = 0;
                            while (timeValue < 1)
                            {
                                timeValue += Time.deltaTime * 0.5f;
                                for (int i = 0; i < audioClips.Count; i++)
                                {
                                    childMixer.SetInputWeight(i, timeValue * audioClips[i].weight);
                                }

                                yield return 0;
                            }
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
                    GameObjectCurveController.instance.StartIEnumerator(LerpAudio());
                    IEnumerator LerpAudio()
                    {
                        float timeValue = 0;
                        while (timeValue < 1)
                        {
                            timeValue += Time.deltaTime * 0.5f;
                            for (int i = 0; i < audioClips.Count; i++)
                            {
                                childMixer.SetInputWeight(i, timeValue * audioClips[i].weight);
                            }

                            yield return 0;
                        }
                    }
                }
            }
        }

        if (!playableGraph.IsPlaying())
        {
            playableGraph.Play();
        }
        int rootCount = playableGraph.GetRootPlayableCount();
        for (int i = 0; i < rootCount; i++)
        {
            var playable = playableGraph.GetRootPlayable(i);
            try
            {
                var audioPlayable = (AudioClipPlayable)playable;
                if (audioPlayable.IsNull())
                {
                }
                else
                {
                    audioPlayable.Destroy();
                }
            }
            catch
            {
            }
        }
    }

    private void PlayME(AudioClip audioClip, bool loop = false, AudioClearType audioClearType = AudioClearType.NoClear, float weight = 1, bool isLerp = false, string Group = "Default")
    {
        PlayAudio(meGraph, meMixer, meMixerDic, audioClip, loop, audioClearType, weight, isLerp, Group);
    }

    public void StopME()
    {
        meGraph.Stop();
    }

    public void PlayBGM(AudioClip audioClip, bool loop = true, AudioClearType audioClearType = AudioClearType.NoClear, float weight = 1, bool isLerp = false, string Group = "Default")
    {
        PlayAudio(bgmGraph, bgmMixer, bgmMixerDic, audioClip, loop, audioClearType, weight, isLerp, Group);
    }

    public void StopBgm()
    {
        bgmGraph.Stop();
    }

    private void PlayBGS(AudioClip audioClip, bool loop = true, AudioClearType audioClearType = AudioClearType.NoClear, float weight = 1, bool isLerp = false, string Group = "Default")
    {
        PlayAudio(bgsGraph, bgsMixer, bgsMixerDic, audioClip, loop, audioClearType, weight, isLerp, Group);
    }

    private void PlayBGS(List<AudioPlayData> audioClips, AudioClearType audioClearType = AudioClearType.NoClear, bool isLerp = false, string Group = "Default")
    {
        PlayAudio(bgsGraph, bgsMixer, bgsMixerDic, audioClips, audioClearType, isLerp, Group);
    }

    public void StopBGS()
    {
        bgsGraph.Stop();
    }

    public void SetMasterVolume(float volume)    // 控制主音量的函数
    {
        audioMixer.SetFloat("MasterVolume", -40 + 40 * volume);
    }

    public void SetBGMVolume(float volume)    // 控制背景音乐音量的函数
    {
        audioMixer.SetFloat("BGMVolume", -40 + 40 * volume);
    }

    public void SetSEVolume(float volume)    // 控制音效音量的函数
    {
        audioMixer.SetFloat("SEVolume", -40 + 40 * volume);
    }
}