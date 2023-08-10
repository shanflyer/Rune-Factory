using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioController : Singleton<AudioController> 
{
    public override async void Init()
    {
        base.Init();
        audioMixer =await ExtensionsResources.LoadResourceAsync<AudioMixer>("AudioMixer");
    }
    AudioMixer audioMixer;
	 
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
    
	// Update is called once per frame
	void Update () {
		
	}
}
