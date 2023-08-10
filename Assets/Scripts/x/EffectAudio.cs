using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectAudio: MonoBehaviour
{
    public AudioClip effectAudioClip;
	// Use this for initialization
	void Start () {
		
	}

    public void PlayAudio()
    {
        GetComponent<AudioSource>().Stop();;
        GetComponent<AudioSource>().clip = effectAudioClip;
        GetComponent<AudioSource>().Play();
    }
	// Update is called once per frame
	void Update () {
		
	}
}
