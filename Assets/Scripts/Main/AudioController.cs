using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Playables;
using System.Collections.Generic;
using Unity.Entities.UniversalDelegates;

public enum AudioClearType
{
    All,Oldest,NoClear
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
    private BGM nowBGM;
    private BGS nowBGS;
    public async void PlayAudio(SE se, bool loop = false)
    {
        AudioClip audioClip = await GameSourceManager.instance.GetAudioClip(GameCommon.AddString(DataPath.SEPath, se.ToString()));
        PlaySE(audioClip, loop);
    }

    public async void PlayAudio(ME me, bool loop = false)
    {
        
        AudioClip audioClip = await GameSourceManager.instance.GetAudioClip(GameCommon.AddString(DataPath.MEPath, me.ToString()));
        PlayME(audioClip, loop);
    }
     
    public async void PlayAudio(BGM bgm, bool loop = true)
    {
        if (nowBGM != bgm)
        {
            nowBGM = bgm;
            AudioClip audioClip = await
            GameSourceManager.instance.GetAudioClip(GameCommon.AddString(DataPath.BGMPath, bgm.ToString()));
            PlayBGM(audioClip, loop);
        } 
    }

    public async void PlayAudio(string bgm, bool loop = true)
    {
        AudioClip audioClip = await
            GameSourceManager.instance.GetAudioClip(GameCommon.AddString(DataPath.BGMPath, bgm));
        PlayBGM(audioClip, loop);
    }
     
    public async void PlayAudio(BGS bgs, bool loop = true)
    {
        if (bgs != nowBGS)
        {
            nowBGS = bgs;
            AudioClip audioClip = await
           GameSourceManager.instance.GetAudioClip(GameCommon.AddString(DataPath.BGSPath, bgs.ToString()));
            PlayBGS(audioClip, loop);
        } 
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
    void PlayAudio(PlayableGraph playableGraph,AudioMixerPlayable audioMixer,AudioClip audioClip, bool loop = false, AudioClearType audioClearType = AudioClearType.NoClear, float weight = 1, bool isLerp = false)
    {
        var count = audioMixer.GetInputCount();
        if (!isLerp)
        {
            AudioClipPlayable audioClipPlayable = AudioClipPlayable.Create(playableGraph, audioClip, loop);
            if (count > 0)
            {
                if (audioClearType == AudioClearType.All)
                {
                    for (int i = count - 1; i <= 0; i--)
                    {
                        playableGraph.Disconnect(audioMixer, i);
                    }
                    audioMixer.SetInputCount(1);
                    playableGraph.Connect(audioClipPlayable, 0, audioMixer, 0);
                    audioMixer.SetInputWeight(0, weight);
                }
                else if (audioClearType == AudioClearType.Oldest)
                {
                    playableGraph.Disconnect(audioMixer, 0);
                    if (count > 1)
                    {
                        for (int i = 1; i < count; i++)
                        {
                            var playable = audioMixer.GetInput(i);
                            float oldWeight = audioMixer.GetInputWeight(i);
                            playableGraph.Disconnect(audioMixer, i);
                            playableGraph.Connect(playable, 0, audioMixer, i - 1);
                            audioMixer.SetInputWeight(i - 1, oldWeight);
                        }
                    }
                    playableGraph.Connect(audioClipPlayable, 0, audioMixer, count - 1);
                    audioMixer.SetInputWeight(count - 1, weight);
                }
                else
                {
                    audioMixer.AddInput(audioClipPlayable, 0, weight);
                }
            }
            else
            {
                audioMixer.AddInput(audioClipPlayable, 0, weight);
            } ;
        }
        else
        {
            AudioClipPlayable audioClipPlayable = AudioClipPlayable.Create(playableGraph, audioClip, loop);
            if (count > 0)
            {
                audioMixer.AddInput(audioClipPlayable, 0, 0);
                if (audioClearType == AudioClearType.All)
                {
                    List<float> startWeights = new List<float>();
                    for (int i = 0; i < count; i++)
                    {
                        startWeights.Add(audioMixer.GetInputWeight(i));
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
                                audioMixer.SetInputWeight(i, startWeights[i] * (1 - timeValue));
                            }
                            audioMixer.SetInputWeight(count, timeValue * weight);

                            yield return 0;
                        }
                        for (int i = 0; i < count; i++)
                        {
                            playableGraph.Disconnect(audioMixer, i);
                        }
                        audioMixer.SetInputCount(1);
                        audioMixer.SetInputWeight(0, weight);
                    }

                }
                else if (audioClearType == AudioClearType.Oldest)
                {
                    float oldWeight = audioMixer.GetInputWeight(0);
                    List<float> oldWeights = new List<float>();
                    for (int i = 1; i < count; i++)
                    {
                        oldWeights.Add(audioMixer.GetInputWeight(i));
                    }
                    GameObjectCurveController.instance.StartIEnumerator(LerpAudio());
                    IEnumerator LerpAudio()
                    {
                        float timeValue = 0;
                        while (timeValue < 1)
                        {
                            timeValue += Time.deltaTime * 0.5f;
                            audioMixer.SetInputWeight(0, oldWeight * (1 - timeValue));
                            audioMixer.SetInputWeight(1, timeValue * weight);
                            yield return 0;
                        }
                        playableGraph.Disconnect(audioMixer, 0);
                        audioMixer.SetInputCount(count);
                        for (int i = 0; i < oldWeights.Count; i++)
                        {
                            audioMixer.SetInputWeight(i, oldWeights[i]);
                        }
                        audioMixer.SetInputWeight(count - 1, weight);
                    }
                }
                else
                {
                    audioMixer.AddInput(audioClipPlayable, 0, weight);
                }

            }
            else
            {
                audioMixer.AddInput(audioClipPlayable, 0, weight);
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


    private void PlayME(AudioClip audioClip, bool loop = false, AudioClearType audioClearType=AudioClearType.NoClear,float weight=1, bool isLerp=false)
    {
        PlayAudio(meGraph, meMixer, audioClip, loop, audioClearType, weight, isLerp);
    }

    public void StopME()
    {  
        meGraph.Stop();
    }
     
    public void PlayBGM(AudioClip audioClip, bool loop = true, AudioClearType audioClearType = AudioClearType.NoClear, float weight = 1, bool isLerp = false)
    { 
        PlayAudio(bgmGraph, bgmMixer, audioClip, loop, audioClearType, weight, isLerp); 
    }

    public void StopBgm()
    {
        bgmGraph.Stop();
    } 
    private void PlayBGS(AudioClip audioClip, bool loop = true, AudioClearType audioClearType = AudioClearType.NoClear, float weight = 1, bool isLerp = false)
    {
        PlayAudio(bgsGraph, bgsMixer, audioClip, loop, audioClearType, weight, isLerp);
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