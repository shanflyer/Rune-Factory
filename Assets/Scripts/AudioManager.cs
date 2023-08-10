using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum PlayType
{
    ONCE=1,
    CYCLE=2,
}
public static class AudioManager
{

    public static List<AudioClip> BGMClips,BGSClips;
    public static List<AudioClip> SEClips,MEClips;
    public static List<AudioSource> AudioSources;
    public static void PlayClick() { PlaySE(PlayType.ONCE, "Click"); }
    public static void PlayBook() { PlaySE(PlayType.ONCE, "Book"); }
    public static void PlayClickResult() { PlaySE(PlayType.ONCE, "Click2"); }
    public static void PlaySelect() { PlaySE(PlayType.ONCE, "Select"); }
    public static void PlayReturn() { PlaySE(PlayType.ONCE, "Return"); }
    public static void PlayCharactorSE(GameObject obj,PlayType playType,string name)
    {
        AudioClip audioClip = SEClips.Find(a => a.name == name);
        AudioSource audioSource = obj.GetComponent<AudioSource>();
        audioSource.clip = audioClip;
        if (playType == PlayType.CYCLE)
        {
            audioSource.loop = true;
        }
        else
        {
            audioSource.loop = false;
        }
        audioSource.Play();
    }
    public static void InitAudioList()
    {
        BGMClips = Resources.LoadAll<AudioClip>("Audio/BGM/").ToList();
        BGSClips = Resources.LoadAll<AudioClip>("Audio/BGS/").ToList();
        SEClips = Resources.LoadAll<AudioClip>("Audio/SE/").ToList();
        MEClips = Resources.LoadAll<AudioClip>("Audio/ME/").ToList();
        AudioSources =new List<AudioSource>();
        AudioSources = GameObject.Find("Audio").GetComponentsInChildren<AudioSource>().ToList();
    }
    public static void StopAllAudio()
    {
        foreach (var audioSource in AudioSources)
        {
            audioSource.Stop();
        }
    }
    public static void PlayAllAudio()
    {
        foreach (var audioSource in AudioSources)
        {
            audioSource.Play();
        }
    }
    public static void PauseAllAudio()
    {
        foreach (var audioSource in AudioSources)
        {
            audioSource.Pause();
        }
    }

    public static void PlayBGM(PlayType playType)
    {
        AudioSource audioSource = AudioSources.Find(a => a.gameObject.name == "BGM");
        if (playType== PlayType.CYCLE)
        {
            audioSource.loop = true;
        }
        else
        {
            audioSource.loop = false;
        }
        audioSource.Play();
    }
    public static void PlayBGM(PlayType playType, string name)
    {
        AudioClip audioClip = BGMClips.Find(a=>a.name==name);
        AudioSource audioSource = AudioSources.Find(a => a.gameObject.name == "BGM");
        audioSource.clip = audioClip;
        if (playType == PlayType.CYCLE)
        {
            audioSource.loop = true;
        }
        else
        {
            audioSource.loop = false;
        }
        audioSource.Play();
    }
    public static void StopBGM()
    {
        AudioSource audioSource = AudioSources.Find(a => a.gameObject.name == "BGM");
        audioSource.Stop();
    }
    public static void PauseBGM()
    {
        AudioSource audioSource = AudioSources.Find(a => a.gameObject.name == "BGM");
        audioSource.Pause();
    }

    public static void PlayBGS(PlayType playType)
    {
        AudioSource audioSource = AudioSources.Find(a => a.gameObject.name == "BGS");
        if (playType == PlayType.CYCLE)
        {
            audioSource.loop = true;
        }
        else
        {
            audioSource.loop = false;
        }
        audioSource.Play();
    }
    public static void PlayBGS(PlayType playType, string name)
    {
        AudioClip audioClip = BGSClips.Find(a => a.name == name);
        AudioSource audioSource = AudioSources.Find(a => a.gameObject.name == "BGS");
        audioSource.clip = audioClip;
        if (playType == PlayType.CYCLE)
        {
            audioSource.loop = true;
        }
        else
        {
            audioSource.loop = false;
        }
        audioSource.Play();
    }
    public static void StopBGS()
    {
        AudioSource audioSource = AudioSources.Find(a => a.gameObject.name == "BGS");
        audioSource.Stop();
    }
    public static void PauseBGS()
    {
        AudioSource audioSource = AudioSources.Find(a => a.gameObject.name == "BGS");
        audioSource.Pause();
    }

    public static void PlayME(PlayType playType)
    {
        AudioSource audioSource = AudioSources.Find(a => a.gameObject.name == "ME");
        if (playType == PlayType.CYCLE)
        {
            audioSource.loop = true;
        }
        else
        {
            audioSource.loop = false;
        }
        audioSource.Play();
    }
    public static void PlayME(PlayType playType, string name)
    {
        AudioClip audioClip = MEClips.Find(a => a.name == name);
        AudioSource audioSource = AudioSources.Find(a => a.gameObject.name == "ME");
        audioSource.clip = audioClip;
        if (playType == PlayType.CYCLE)
        {
            audioSource.loop = true;
        }
        else
        {
            audioSource.loop = false;
        }
        audioSource.Play();
    }
    public static void StopME()
    {
        AudioSource audioSource = AudioSources.Find(a => a.gameObject.name == "ME");
        audioSource.Stop();
    }
    public static void PauseME()
    {
        AudioSource audioSource = AudioSources.Find(a => a.gameObject.name == "ME");
        audioSource.Pause();
    }

    public static void PlaySE(PlayType playType)
    {
        AudioSource audioSource = AudioSources.Find(a => a.gameObject.name == "SE");
        if (playType == PlayType.CYCLE)
        {
            audioSource.loop = true;
        }
        else
        {
            audioSource.loop = false;
        }
        audioSource.Play();
    }
    public static void PlaySE(PlayType playType,string name)
    {
        AudioClip audioClip = SEClips.Find(a => a.name == name);
        AudioSource audioSource = AudioSources.Find(a => a.gameObject.name == "SE");
        audioSource.clip = audioClip;
        if (playType == PlayType.CYCLE)
        {
            audioSource.loop = true;
        }
        else
        {
            audioSource.loop = false;
        }
        audioSource.Play();
    }
    public static void StopSE()
    {
        AudioSource audioSource = AudioSources.Find(a => a.gameObject.name == "SE");
        audioSource.Stop();
    }
    public static void PauseSE()
    {
        AudioSource audioSource = AudioSources.Find(a => a.gameObject.name == "SE");
        audioSource.Pause();
    }

   
   
   
}
