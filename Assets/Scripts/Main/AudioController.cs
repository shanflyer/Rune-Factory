using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Playables;
using System.Collections.Generic;
using Unity.Mathematics;

public enum AudioClearType
{
    All,Oldest,NoClear
}
public struct AudioPlayData
{
    public AudioClip audioClip;
    public float weight;
    public bool loop;
}
public class AudioController : Singleton<AudioController>
{
    public override async void Init()
    {
        base.Init();
        audioMixer = await ExtensionsResources.LoadResourceAsync<AudioMixer>("AudioMixer");
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
        bgsOut= AudioPlayableOutput.Null;
        meOut= AudioPlayableOutput.Null;
        seOut = AudioPlayableOutput.Null;
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
            meMixer=AudioMixerPlayable.Create(meGraph);
            meOut.SetSourcePlayable(meMixer);
        }
        if (SE)
        {
            var seAudioSource = SE.GetComponent<AudioSource>();
            seGraph = PlayableGraph.Create("SE");
            seOut = AudioPlayableOutput.Create(seGraph, "SE", seAudioSource);
        }
    }

    private AudioMixer audioMixer;

    private PlayableGraph bgmGraph, bgsGraph, meGraph, seGraph;
    private AudioPlayableOutput bgmOut, bgsOut, meOut, seOut;
    private AudioMixerPlayable bgmMixer, bgsMixer, meMixer;

    private Dictionary<string, AudioMixerPlayable> bgmMixerDic = new Dictionary<string, AudioMixerPlayable>();
    private Dictionary<string, AudioMixerPlayable> bgsMixerDic = new Dictionary<string, AudioMixerPlayable>();
    private Dictionary<string, AudioMixerPlayable> meMixerDic = new Dictionary<string, AudioMixerPlayable>();

    private BGM nowBGM;
    private BGS nowBGS;
    public async void PlayAudio(SE se, bool loop = false)
    {
        AudioClip audioClip = await GameSourceManager.instance.GetAudioClip(GameCommon.AddString(DataPath.SEPath, se.ToString()));
        PlaySE(audioClip, loop);
    }

    public async void PlayAudio(ME me, bool loop = false, AudioClearType audioClearType = AudioClearType.NoClear, float weight = 1, bool isLerp = false, string tag = "Default")
    {
        
        AudioClip audioClip = await GameSourceManager.instance.GetAudioClip(GameCommon.AddString(DataPath.MEPath, me.ToString()));
        PlayME(audioClip, loop,audioClearType,weight,isLerp,tag);
    }
     
    public async void PlayAudio(BGM bgm, bool loop = true, AudioClearType audioClearType = AudioClearType.NoClear, float weight = 1, bool isLerp = false, string tag = "Default")
    {
        if (nowBGM != bgm)
        {
            nowBGM = bgm;
            AudioClip audioClip = await
            GameSourceManager.instance.GetAudioClip(GameCommon.AddString(DataPath.BGMPath, bgm.ToString()));
            PlayBGM(audioClip, loop,audioClearType,weight,isLerp,tag);
        } 
    } 
    public async void PlayAudio(BGS bgs, bool loop = true, AudioClearType audioClearType = AudioClearType.NoClear, float weight = 1, bool isLerp = false, string tag = "Default")
    {
        if (bgs != nowBGS)
        {
            nowBGS = bgs;
            AudioClip audioClip = await
            GameSourceManager.instance.GetAudioClip(GameCommon.AddString(DataPath.BGSPath, bgs.ToString()));
            PlayBGS(audioClip, loop, audioClearType,weight,isLerp,tag);
        } 
    }
    public async void PlayAudio(List<float3> bgs, AudioClearType audioClearType = AudioClearType.NoClear, bool isLerp = false, string tag = "Default")
    {
        if (bgs.Count > 0)
        {
            nowBGS = (BGS)(int)(bgs[bgs.Count - 1].x);
        }
        else
        {
            nowBGS = BGS.NUll;
        }
        List<AudioPlayData> audioPlayDatas = new List<AudioPlayData>();
        for(int i = 0; i < bgs.Count; i++)
        {
            var b= (BGS)(int)(bgs[i].x);
            AudioClip audioClip = await  GameSourceManager.instance.GetAudioClip(GameCommon.AddString(DataPath.BGSPath, b.ToString()));
            AudioPlayData audioPlayData = new AudioPlayData
            {
                audioClip = audioClip,
                weight = bgs[i].y,
                loop = bgs[i].z > 0
            };
            audioPlayDatas.Add(audioPlayData);
        }

       
        PlayBGS(audioPlayDatas,audioClearType, isLerp, tag);
    }
    private void PlaySE(AudioClip audioClip, bool loop = false)
    {
        try
        { 
            AudioClipPlayable audioClipPlayable = AudioClipPlayable.Create(seGraph, audioClip, loop); 
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
    void PlayAudio(PlayableGraph playableGraph,AudioMixerPlayable audioMixer,Dictionary<string,AudioMixerPlayable> childMixers,AudioClip audioClip, bool loop = false,
        AudioClearType audioClearType = AudioClearType.NoClear, float weight = 1, bool isLerp = false,string tag="Default")
    {
        if(!childMixers.TryGetValue(tag,out var childMixer))
        {
            childMixer = AudioMixerPlayable.Create(playableGraph);
            childMixers.Add(tag, childMixer);
            audioMixer.AddInput(childMixer, 0, 1);
        }

        var count = childMixer.GetInputCount();
        if (!isLerp)
        {
            AudioClipPlayable audioClipPlayable =  AudioClipPlayable.Create(playableGraph, audioClip, loop);
            if (count > 0)
            {
                if (audioClearType == AudioClearType.All)
                {
                    for (int i = count - 1; i <= 0; i--)
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
                        childMixer.SetInputCount(count-1);
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
                       
                        if (audioClip != null)
                        {
                            childMixer.SetInputCount(1);
                            childMixer.ConnectInput(0,audioClipPlayable,0);
                            childMixer.SetInputWeight(0, weight);
                        }
                        else
                        {
                            childMixer.SetInputCount(0);
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
                            childMixer.SetInputCount(count-1);
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
    void PlayAudio(PlayableGraph playableGraph, AudioMixerPlayable audioMixer, Dictionary<string, AudioMixerPlayable> childMixers, List<AudioPlayData> audioClips,
       AudioClearType audioClearType = AudioClearType.NoClear, bool isLerp = false, string tag = "Default")
    {
        if (!childMixers.TryGetValue(tag, out var childMixer))
        {
            childMixer = AudioMixerPlayable.Create(playableGraph);
            childMixers.Add(tag, childMixer);
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
                        for(int i = 0; i < count - audioClips.Count; i++)
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
                        for (int i = count-1; i >=0; i--)
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
                    for(int i = 0; i < audioClips.Count; i++)
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
                                    childMixer.SetInputWeight(count+i, timeValue * audioClips[i].weight);
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

                            for(int i = 0; i < audioClips.Count; i++)
                            {
                                if (i < count)
                                {
                                    childMixer.SetInputWeight(0, oldWeights[i] * (1 - timeValue));
                                }
                                childMixer.SetInputWeight(i+count, timeValue * audioClips[i].weight);
                            } 
                            yield return 0;
                        }
                        if (count > audioClips.Count)
                        {
                            for (int i = count - audioClips.Count; i>=0; i--)
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
                        for(int i = count-1; i >= 0; i--)
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

    private void PlayME(AudioClip audioClip, bool loop = false, AudioClearType audioClearType=AudioClearType.NoClear,float weight=1, bool isLerp=false, string tag = "Default")
    {
        PlayAudio(meGraph, meMixer,meMixerDic, audioClip, loop, audioClearType, weight, isLerp,tag);
    }
   

    public void StopME()
    {  
        meGraph.Stop();
    }
     
    public void PlayBGM(AudioClip audioClip, bool loop = true, AudioClearType audioClearType = AudioClearType.NoClear, float weight = 1, bool isLerp = false,string tag="Default")
    {
        PlayAudio(bgmGraph, bgmMixer,bgmMixerDic, audioClip, loop, audioClearType, weight, isLerp,tag); 
    }

    public void StopBgm()
    {
        bgmGraph.Stop();
    }
    private void PlayBGS(AudioClip audioClip, bool loop = true, AudioClearType audioClearType = AudioClearType.NoClear, float weight = 1, bool isLerp = false, string tag = "Default")
    {
        PlayAudio(bgsGraph, bgsMixer, bgsMixerDic, audioClip, loop, audioClearType, weight, isLerp, tag);
    }
    private void PlayBGS(List<AudioPlayData> audioClips,AudioClearType audioClearType = AudioClearType.NoClear, bool isLerp = false, string tag = "Default")
    {
        PlayAudio(bgsGraph, bgsMixer,bgsMixerDic, audioClips, audioClearType, isLerp, tag);
    }

    public void StopBGS()
    {
        bgsGraph.Stop();
    }

    public void SetMasterVolume(float volume)    // 控制主音量的函数
    {
        audioMixer.SetFloat("MasterVolume", -20 + 40 * volume);
    }

    public void SetBGMVolume(float volume)    // 控制背景音乐音量的函数
    {
        audioMixer.SetFloat("BGMVolume", -20 + 40 * volume);
    }

    public void SetSEVolume(float volume)    // 控制音效音量的函数
    {
        audioMixer.SetFloat("SEVolume", -20 + 40 * volume);
    }
}