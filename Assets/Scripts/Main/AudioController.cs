using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Playables;

public class AudioController : Singleton<AudioController> 
{
    public override async void Init()
    {
        base.Init();
        audioMixer =await ExtensionsResources.LoadResourceAsync<AudioMixer>("AudioMixer");
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
    }
    public void SetAudioSource(GameObject audioObj)
    {
        var BGM = audioObj.transform.Find("BGM");
        var BGS= audioObj.transform.Find("BGS");
        var ME = audioObj.transform.Find("ME");
        var SE = audioObj.transform.Find("SE");

        if (BGM)
        {
            var bgmAudioSource = BGM.GetComponent<AudioSource>();
            bgmGraph = PlayableGraph.Create("BGM");
            bgmOut = AudioPlayableOutput.Create(bgmGraph, "BGM", bgmAudioSource); 
        }
        if (BGS)
        {
            var bgsAudioSource = BGS.GetComponent<AudioSource>();
            bgsGraph = PlayableGraph.Create("BGS");
            bgsOut = AudioPlayableOutput.Create(bgsGraph, "BGS", bgsAudioSource);
        }

        if (ME)
        {
            var meAudioSource = ME.GetComponent<AudioSource>();
            meGraph = PlayableGraph.Create("ME");
            meOut = AudioPlayableOutput.Create(meGraph, "ME", meAudioSource);
        }
        if (SE)
        {
            var seAudioSource = SE.GetComponent<AudioSource>();
            seGraph = PlayableGraph.Create("SE");
            seOut = AudioPlayableOutput.Create(seGraph, "SE", seAudioSource);
        }

            
    }
    AudioMixer audioMixer;

    PlayableGraph bgmGraph, bgsGraph, meGraph, seGraph;
    AudioPlayableOutput bgmOut, bgsOut, meOut, seOut;

    

    public async void PlayAudio(SE se, bool loop = false)
    {
        AudioClip audioClip =await GameSourceManager.instance.GetAudioClip(GameCommon.AddString(DataPath.SEPath, se.ToString()));
        PlaySE(audioClip, loop);
    }
    public async void PlayAudio(ME me, bool loop = false)
    {
        AudioClip audioClip = await GameSourceManager.instance.GetAudioClip(GameCommon.AddString(DataPath.MEPath, me.ToString()));
        PlayME(audioClip, loop);
    }
    public async void PlayAudio(BGM bgm, bool loop = true)
    {
        AudioClip audioClip = await 
            GameSourceManager.instance.GetAudioClip(GameCommon.AddString(DataPath.BGMPath, bgm.ToString()));
        PlayBGM(audioClip, loop);
    }
    public async void PlayAudio(string bgm, bool loop = true)
    {
        AudioClip audioClip = await
            GameSourceManager.instance.GetAudioClip(GameCommon.AddString(DataPath.BGMPath, bgm));
        PlayBGM(audioClip, loop);
    }
    public async void PlayAudio(BGS bgs, bool loop = true)
    {
        AudioClip audioClip = await
            GameSourceManager.instance.GetAudioClip(GameCommon.AddString(DataPath.BGSPath, bgs.ToString()));
        PlayBGS(audioClip, loop);
    }

    void PlaySE(AudioClip audioClip,bool loop=false)
    {
        AudioClipPlayable audioClipPlayable=AudioClipPlayable.Create(seGraph, audioClip, loop);
        seOut.SetSourcePlayable(audioClipPlayable);
        seGraph.Play();
    }
    public void StopSE()
    {
        seGraph.Stop();
    }

   void PlayME(AudioClip audioClip, bool loop = false)
    {
        AudioClipPlayable audioClipPlayable = AudioClipPlayable.Create(meGraph, audioClip, loop);
        meOut.SetSourcePlayable(audioClipPlayable);
        meGraph.Play();
    }
    public void StopME()
    {
        meGraph.Stop();
    }

    public void PlayBGM(AudioClip audioClip, bool loop = true)
    {
        AudioClipPlayable audioClipPlayable = AudioClipPlayable.Create(bgmGraph, audioClip, loop);
        bgmOut.SetSourcePlayable(audioClipPlayable);
        bgmGraph.Play();
    }
    public void StopBgm()
    {
        bgmGraph.Stop();
    }

    void PlayBGS(AudioClip audioClip, bool loop = true)
    {
        AudioClipPlayable audioClipPlayable = AudioClipPlayable.Create(bgsGraph, audioClip, loop);
        bgsOut.SetSourcePlayable(audioClipPlayable);
        bgsGraph.Play();
    }
    public void StopBGS()
    {
        bgsGraph.Stop();
    }


    public void SetMasterVolume(float volume)    // 控制主音量的函数
    {
        
        audioMixer.SetFloat("MasterVolume",-20+40*volume);
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
