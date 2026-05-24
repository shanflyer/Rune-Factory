using UnityEngine;
using UnityEngine.Playables;
using System.Collections.Generic;
using UnityEngine.Audio;
using Unity.Entities.UniversalDelegates;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class TeatAudio : MonoBehaviour
{
    PlayableGraph playableGraph;
    AudioMixerPlayable mixerPlayable;
    AudioPlayableOutput AudioPlayableOutput;
    public AudioSource audioSource;
    public List<AudioClip> audioClips=new List<AudioClip>();
    public AudioClip testAudio;
    public List<float> weights= new List<float>();

    public void SetWeights()
    {
        var count = mixerPlayable.GetInputCount();
        for (int i = 0; i < count; i++)
        {
            mixerPlayable.SetInputWeight(i, weights[i]);
        }
    }
    public void TestAudioList()
    {
        if (!playableGraph.IsValid())
        {
            playableGraph = PlayableGraph.Create("TestAudio");
        }
        if (!AudioPlayableOutput.IsOutputValid())
        {
            AudioPlayableOutput = AudioPlayableOutput.Create(playableGraph, "TestOut", audioSource);
        }
        if (!mixerPlayable.IsValid())
        {
            mixerPlayable = AudioMixerPlayable.Create(playableGraph, 0);
        }
        for(int i = audioClips.Count-1; i >=0; i--)
        {
            AudioClipPlayable audioClipPlayable = AudioClipPlayable.Create(playableGraph, audioClips[i], true);
            mixerPlayable.AddInput(audioClipPlayable, 0,1);
        }
        AudioPlayableOutput.SetSourcePlayable(mixerPlayable);
        playableGraph.Play();
    }
    public void TestAudio()
    {
        if (!playableGraph.IsValid())
        {
            playableGraph = PlayableGraph.Create("TestAudio");
        }
       if(!AudioPlayableOutput.IsOutputValid())
        {
            AudioPlayableOutput = AudioPlayableOutput.Create(playableGraph, "TestOut", audioSource);
        }
        if (!mixerPlayable.IsValid())
        {
            mixerPlayable = AudioMixerPlayable.Create(playableGraph, 0);
        }

        AudioClipPlayable audioClipPlayable = AudioClipPlayable.Create(playableGraph, testAudio, true);
        var count = mixerPlayable.GetInputCount();
        Debug.Log($"InputCount:{count}");
        if (count > 0)
        {

        }
        mixerPlayable.AddInput(audioClipPlayable, 0,1);

        //mixerPlayable.AddInput(audioClipPlayable, 0);
       // playableGraph.Connect(audioClipPlayable,0, mixerPlayable,0);
      //  mixerPlayable.SetInputWeight(audioClipPlayable, 1);
        AudioPlayableOutput.SetSourcePlayable(mixerPlayable);
        playableGraph.Play();

    }
    public void DestroyPlayableGraph()
    {
        playableGraph.Destroy();
        mixerPlayable.Destroy();
        AudioPlayableOutput = AudioPlayableOutput.Null;
    }
    public void RemoveClip()
    {
        var count = mixerPlayable.GetInputCount();
        Debug.Log($"InputCount:{count}");

        if (count > 0)
        {
           playableGraph.Disconnect(mixerPlayable, count-1);

            // mixerPlayable.DisconnectInput(0);
            mixerPlayable.SetInputCount(count - 1);
        }

        var root = playableGraph.GetRootPlayableCount();
        Debug.Log($"root:{root}");
        for (int i = 0; i < root; i++)
        {
            var playable = playableGraph.GetRootPlayable(i);
            Debug.Log(playable.GetType());
            try
            {
                var audioPlayable = (AudioClipPlayable)playable;
                if (audioPlayable.IsNull())
                {

                }
                else
                {
                    var audioClip = audioPlayable.GetClip();
                    if (audioClip != null)
                    {
                        Debug.Log($"root:{i}--{audioClip.name}");
                    }
                    else
                    {
                        Debug.Log("clip null");
                    }
                    audioPlayable.Destroy();
                }
            }
            catch
            {

            }
        }

        count = mixerPlayable.GetInputCount();
        Debug.Log($"InputCount:{count}");
    }
    public void OutMixPlayable()
    {
        var count = mixerPlayable.GetInputCount();
        for(int i = 0; i < count; i++)
        {
           var playable=  (AudioClipPlayable) mixerPlayable.GetInput(i);
            if (playable.IsNull())
            {
                Debug.Log($"out {i}:playable null");
                continue;
            }
           var clip = playable.GetClip();
            if (clip == null)
            {
                Debug.Log($"out {i}:null");
            }
            else
            {
                Debug.Log($"out {i}:{clip.name}");
            }

        }

        var root = playableGraph.GetRootPlayableCount();
        Debug.Log($"root:{root}");

        for (int i = 0; i < root; i++)
        {
            var playable = playableGraph.GetRootPlayable(i);
            Debug.Log(playable.GetType());
            try
            {
                var audioPlayable = (AudioClipPlayable)playable;
                if (audioPlayable.IsNull())
                {

                }
                else
                {
                    var audioClip = audioPlayable.GetClip();
                    if (audioClip != null)
                    {
                        Debug.Log($"root:{i}--{audioClip.name}");
                    }
                    else
                    {
                        Debug.Log("clip null");
                    }

                }
            }
            catch
            {

            }
        }
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
#if UNITY_EDITOR
[CustomEditor(typeof(TeatAudio))]
public class TeatAudioEditor : Editor
{
    public TeatAudio teatAudio => target as TeatAudio;
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (GUILayout.Button("test"))
        {
            teatAudio.TestAudio();
        }
        if (GUILayout.Button("remove"))
        {
            teatAudio.RemoveClip();
        }
        if (GUILayout.Button("Out"))
        {
            teatAudio.OutMixPlayable();
        }
        if (GUILayout.Button("TestList"))
        {
            teatAudio.TestAudioList();
        }
        if (GUILayout.Button("destroy"))
        {
            teatAudio.DestroyPlayableGraph();
        }
        if (GUILayout.Button("SetWeights"))
        {
            teatAudio.SetWeights();
        }
    }
}
#endif
