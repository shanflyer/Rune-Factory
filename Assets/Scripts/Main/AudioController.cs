using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
    private const string NullClipKey = "NULL";
    private const float DefaultFadeDuration = 1f;
    private const float WeightEpsilon = 0.001f;
    private const int MaxSeVoicesPerGroup = 8;

    private sealed class AudioLayer
    {
        public readonly string Name;
        public AudioSource Source;
        public PlayableGraph Graph;
        public AudioPlayableOutput Output;
        public AudioMixerPlayable RootMixer;
        public readonly Dictionary<string, AudioMixerPlayable> GroupMixers = new Dictionary<string, AudioMixerPlayable>();
        public readonly Dictionary<string, AudioGroupState> States = new Dictionary<string, AudioGroupState>();

        public AudioLayer(string name)
        {
            Name = name;
        }
    }

    private sealed class AudioGroupState
    {
        public string Signature = NullClipKey;
        public float Weight = 1f;
        public bool Loop;
        public int RequestVersion;
    }

    private sealed class AudioFadeTask
    {
        public AudioLayer Layer;
        public string Group;
        public AudioMixerPlayable Mixer;
        public readonly List<Playable> OldPlayables = new List<Playable>();
        public readonly List<float> OldStartWeights = new List<float>();
        public readonly List<Playable> NewPlayables = new List<Playable>();
        public readonly List<float> NewTargetWeights = new List<float>();
        public float Duration = DefaultFadeDuration;
        public float Elapsed;
        public bool RemoveOldWhenDone = true;
    }

    private sealed class SeVoice
    {
        public string Group;
        public AudioMixerPlayable Mixer;
        public AudioClipPlayable Playable;
        public bool Loop;
    }

    private struct PendingAudioPlayData
    {
        public string Key;
        public float Weight;
        public bool Loop;
        public AudioClip Clip;
    }

    [SerializeField]
    private AudioSource bgmAudioSource, bgsAudioSource, meAudioSource, seAudioSource;

    private AudioMixer audioMixer;
    private readonly AudioLayer bgmLayer = new AudioLayer("BGM");
    private readonly AudioLayer bgsLayer = new AudioLayer("BGS");
    private readonly AudioLayer meLayer = new AudioLayer("ME");
    private readonly AudioLayer seLayer = new AudioLayer("SE");
    private readonly List<AudioFadeTask> fadeTasks = new List<AudioFadeTask>();
    private readonly Dictionary<string, List<SeVoice>> seVoices = new Dictionary<string, List<SeVoice>>();
    private float seCleanTimer;

    public override bool NeedUpdate => true;

    public override async void Init()
    {
        base.Init();
        audioMixer = await ExtensionsResources.LoadResourceAsync<AudioMixer>("Audio/AudioMixer");
        ApplySavedMixerVolume();
    }

    protected override void Clear()
    {
        SaveMixerVolume();
        ClearFadeTasks();
        ClearSeVoices();
        DestroyLayer(bgmLayer);
        DestroyLayer(bgsLayer);
        DestroyLayer(meLayer);
        DestroyLayer(seLayer);
        audioMixer = null;
        base.Clear();
    }

    protected override void Update()
    {
        UpdateFadeTasks(Time.deltaTime);
        UpdateSeVoices(Time.deltaTime);
    }

    public void SetBgmAudioSourceVolume(float value)
    {
        if (bgmAudioSource) bgmAudioSource.volume = value;
    }

    public void SetAudioSource(GameObject audioObj)
    {
        if (audioObj == null) return;

        InitializeLayer(bgmLayer, audioObj.transform.Find("BGM"));
        InitializeLayer(bgsLayer, audioObj.transform.Find("BGS"));
        InitializeLayer(meLayer, audioObj.transform.Find("ME"));
        InitializeLayer(seLayer, audioObj.transform.Find("SE"));

        bgmAudioSource = bgmLayer.Source;
        bgsAudioSource = bgsLayer.Source;
        meAudioSource = meLayer.Source;
        seAudioSource = seLayer.Source;
    }

    public async void PlayAudio(SE se, bool loop = false, string Group = "Default")
    {
        if (se == SE.NULL) return;

        var audioClip = await GameSourceManager.instance.GetAudioClip(GameCommon.AddString(DataPath.SEPath, se.ToString()));
        PlaySE(audioClip, loop, Group);
    }

    public void PlayAudioME(AudioClip audioClip, bool loop = false, AudioClearType audioClearType = AudioClearType.All,
        float weight = 1, bool isLerp = false, string Group = "Default")
    {
        PlayME(audioClip, loop, audioClearType, weight, isLerp, Group);
    }

    public void PlayAudioBGS(AudioClip bgs, bool loop = true, AudioClearType audioClearType = AudioClearType.NoClear,
        float weight = 1, bool isLerp = false, string Group = "Default")
    {
        PlayLayerClip(bgsLayer, bgs, GetClipKey(bgs), loop, audioClearType, weight, isLerp, Group, true);
    }

    public void ClearBGM(AudioClearType audioClearType = AudioClearType.NoClear, string Group = "Default")
    {
        PlayAudioBGM(null, false, audioClearType, 1, false, Group);
    }

    public async void PlayBGM(string clipName, bool loop = true, AudioClearType audioClearType = AudioClearType.NoClear,
        float weight = 1, bool isLerp = false, string Group = "Default")
    {
        if (string.IsNullOrEmpty(clipName) || clipName == NullClipKey)
        {
            PlayLayerClip(bgmLayer, null, NullClipKey, loop, audioClearType, weight, isLerp, Group, true);
            return;
        }

        var version = NextRequestVersion(bgmLayer, Group);
        var audioClip = await GameSourceManager.instance.GetAudioClip(GameCommon.AddString(DataPath.BGMPath, clipName));
        if (!IsRequestCurrent(bgmLayer, Group, version)) return;

        PlayLayerClip(bgmLayer, audioClip, clipName, loop, audioClearType, weight, isLerp, Group, false);
    }

    public async void PlayBGM(BGM bgm, bool loop = true, AudioClearType audioClearType = AudioClearType.NoClear,
        float weight = 1, bool isLerp = false, string Group = "Default")
    {
        if (bgm == BGM.NULL)
        {
            PlayLayerClip(bgmLayer, null, NullClipKey, loop, audioClearType, weight, isLerp, Group, true);
            return;
        }

        var key = bgm.ToString();
        var version = NextRequestVersion(bgmLayer, Group);
        var audioClip = await GameSourceManager.instance.GetAudioClip(GameCommon.AddString(DataPath.BGMPath, key));
        if (!IsRequestCurrent(bgmLayer, Group, version)) return;

        PlayLayerClip(bgmLayer, audioClip, key, loop, audioClearType, weight, isLerp, Group, false);
    }

    public async void PlaySE(string clipName, bool loop = false, string Group = "Default")
    {
        if (string.IsNullOrEmpty(clipName) || clipName == NullClipKey) return;

        var audioClip = await GameSourceManager.instance.GetAudioClip(GameCommon.AddString(DataPath.SEPath, clipName));
        PlaySE(audioClip, loop, Group);
    }

    public void PlayAudioBGM(AudioClip bgm, bool loop = true, AudioClearType audioClearType = AudioClearType.NoClear,
        float weight = 1, bool isLerp = false, string Group = "Default")
    {
        PlayLayerClip(bgmLayer, bgm, GetClipKey(bgm), loop, audioClearType, weight, isLerp, Group, true);
    }

    public float3 GetAudioVolume()
    {
        return new float3(GetMixerVolume("MasterVolume"), GetMixerVolume("BGMVolume"), GetMixerVolume("SEVolume"));
    }

    public async void PlayAudio(BGS bgs, bool loop = true, AudioClearType audioClearType = AudioClearType.NoClear,
        float weight = 1, bool isLerp = false, string Group = "Default")
    {
        if (bgs == BGS.NULL)
        {
            PlayLayerClip(bgsLayer, null, NullClipKey, loop, audioClearType, weight, isLerp, Group, true);
            return;
        }

        var key = bgs.ToString();
        var version = NextRequestVersion(bgsLayer, Group);
        var audioClip = await GameSourceManager.instance.GetAudioClip(GameCommon.AddString(DataPath.BGSPath, key));
        if (!IsRequestCurrent(bgsLayer, Group, version)) return;

        PlayLayerClip(bgsLayer, audioClip, key, loop, audioClearType, weight, isLerp, Group, false);
    }

    public async void PlayAudio(List<float3> bgs, AudioClearType audioClearType = AudioClearType.NoClear,
        bool isLerp = false, string Group = "Default")
    {
        var version = NextRequestVersion(bgsLayer, Group);
        var pendingDatas = BuildPendingBgsDatas(bgs);

        if (pendingDatas.Count == 0)
        {
            if (!IsRequestCurrent(bgsLayer, Group, version)) return;
            PlayLayerClips(bgsLayer, null, NullClipKey, audioClearType, isLerp, Group, false);
            return;
        }

        var loadTasks = new Task<AudioClip>[pendingDatas.Count];
        for (var i = 0; i < pendingDatas.Count; i++)
        {
            loadTasks[i] = GameSourceManager.instance.GetAudioClip(GameCommon.AddString(DataPath.BGSPath, pendingDatas[i].Key));
        }

        var clips = await Task.WhenAll(loadTasks);
        if (!IsRequestCurrent(bgsLayer, Group, version)) return;

        var playDatas = new List<AudioPlayData>(pendingDatas.Count);
        for (var i = 0; i < pendingDatas.Count; i++)
        {
            if (clips[i] == null) continue;

            playDatas.Add(new AudioPlayData
            {
                audioClip = clips[i],
                weight = pendingDatas[i].Weight,
                loop = pendingDatas[i].Loop
            });
        }

        var signature = BuildSignature(pendingDatas);
        PlayLayerClips(bgsLayer, playDatas, signature, audioClearType, isLerp, Group, false);
    }

    public void PlaySE(AudioClip audioClip, bool loop = false, string Group = "Default")
    {
        if (audioClip == null || !EnsureLayerReady(seLayer)) return;

        var groupMixer = GetOrCreateGroupMixer(seLayer, Group);
        var audioClipPlayable = AudioClipPlayable.Create(seLayer.Graph, audioClip, loop);
        groupMixer.AddInput(audioClipPlayable, 0, 1);

        if (!seVoices.TryGetValue(Group, out var voices))
        {
            voices = new List<SeVoice>();
            seVoices.Add(Group, voices);
        }

        voices.Add(new SeVoice
        {
            Group = Group,
            Mixer = groupMixer,
            Playable = audioClipPlayable,
            Loop = loop
        });

        // SE 是短音效，限制同组并发数量，避免脚步声/UI 高频触发时无限堆 playable。
        while (voices.Count > MaxSeVoicesPerGroup)
        {
            DestroySeVoice(voices[0]);
            voices.RemoveAt(0);
        }

        PlayGraph(seLayer);
    }

    public void StopSE()
    {
        ClearSeVoices();
        if (seLayer.Graph.IsValid()) seLayer.Graph.Stop();
    }

    public void SetBGMGroupValue(string group, float value)
    {
        SetRootGroupWeight(bgmLayer, group, value);
    }

    public void SetBGSGroupValue(string group, float value)
    {
        SetRootGroupWeight(bgsLayer, group, value);
    }

    public void StopME()
    {
        if (meLayer.Graph.IsValid()) meLayer.Graph.Stop();
    }

    public void PlayBGM(AudioClip audioClip, bool loop = true, AudioClearType audioClearType = AudioClearType.NoClear,
        float weight = 1, bool isLerp = false, string Group = "Default")
    {
        PlayLayerClip(bgmLayer, audioClip, GetClipKey(audioClip), loop, audioClearType, weight, isLerp, Group, true);
    }

    public void StopBgm()
    {
        if (bgmLayer.Graph.IsValid()) bgmLayer.Graph.Stop();
    }

    public void StopBGS()
    {
        if (bgsLayer.Graph.IsValid()) bgsLayer.Graph.Stop();
    }

    public void SetMasterVolume(float volume)
    {
        SetMixerVolume("MasterVolume", volume);
    }

    public void SetBGMVolume(float volume)
    {
        SetMixerVolume("BGMVolume", volume);
    }

    public void SetSEVolume(float volume)
    {
        SetMixerVolume("SEVolume", volume);
    }

    private void PlayME(AudioClip audioClip, bool loop = false, AudioClearType audioClearType = AudioClearType.NoClear,
        float weight = 1, bool isLerp = false, string Group = "Default")
    {
        PlayLayerClip(meLayer, audioClip, GetClipKey(audioClip), loop, audioClearType, weight, isLerp, Group, true);
    }

    private void PlayBGS(AudioClip audioClip, bool loop = true, AudioClearType audioClearType = AudioClearType.NoClear,
        float weight = 1, bool isLerp = false, string Group = "Default")
    {
        PlayLayerClip(bgsLayer, audioClip, GetClipKey(audioClip), loop, audioClearType, weight, isLerp, Group, true);
    }

    private void PlayBGS(List<AudioPlayData> audioClips, AudioClearType audioClearType = AudioClearType.NoClear,
        bool isLerp = false, string Group = "Default")
    {
        PlayLayerClips(bgsLayer, audioClips, BuildSignature(audioClips), audioClearType, isLerp, Group, true);
    }

    private void PlayLayerClip(AudioLayer layer, AudioClip audioClip, string clipKey, bool loop,
        AudioClearType audioClearType, float weight, bool isLerp, string group, bool reserveRequest)
    {
        if (reserveRequest) NextRequestVersion(layer, group);

        var state = GetState(layer, group);
        var nextSignature = audioClip == null ? NullClipKey : $"{clipKey}:{loop}";
        if (state.Signature == nextSignature && state.Loop == loop)
        {
            if (math.abs(state.Weight - weight) > WeightEpsilon)
            {
                SetClipWeights(layer, group, audioClip, weight);
                state.Weight = weight;
            }

            return;
        }

        if (!EnsureLayerReady(layer))
        {
            state.Signature = nextSignature;
            state.Weight = weight;
            state.Loop = loop;
            return;
        }

        var groupMixer = GetOrCreateGroupMixer(layer, group);
        CancelFade(layer, group);

        if (audioClip == null)
        {
            if (isLerp && groupMixer.GetInputCount() > 0)
                StartFadeReplace(layer, group, groupMixer, null, null, true);
            else
                ClearGroupMixer(groupMixer, false);
        }
        else if (audioClearType == AudioClearType.All)
        {
            var newPlayable = AudioClipPlayable.Create(layer.Graph, audioClip, loop);
            if (isLerp && groupMixer.GetInputCount() > 0)
            {
                StartFadeReplace(layer, group, groupMixer, new List<Playable> { newPlayable }, new List<float> { weight }, true);
            }
            else
            {
                ClearGroupMixer(groupMixer, false);
                groupMixer.AddInput(newPlayable, 0, weight);
            }
        }
        else
        {
            if (!TrySetExistingClipWeight(groupMixer, audioClip, weight))
            {
                var newPlayable = AudioClipPlayable.Create(layer.Graph, audioClip, loop);
                groupMixer.AddInput(newPlayable, 0, isLerp ? 0 : weight);
                if (isLerp)
                    StartFadeIn(layer, group, groupMixer, newPlayable, weight);
            }
        }

        state.Signature = nextSignature;
        state.Weight = weight;
        state.Loop = loop;
        PlayGraph(layer);
    }

    private void PlayLayerClips(AudioLayer layer, List<AudioPlayData> audioClips, string signature,
        AudioClearType audioClearType, bool isLerp, string group, bool reserveRequest)
    {
        if (reserveRequest) NextRequestVersion(layer, group);

        var state = GetState(layer, group);
        if (state.Signature == signature)
        {
            UpdateExistingWeights(layer, group, audioClips);
            return;
        }

        if (!EnsureLayerReady(layer))
        {
            state.Signature = signature;
            state.Weight = 1;
            return;
        }

        var groupMixer = GetOrCreateGroupMixer(layer, group);
        CancelFade(layer, group);

        var newPlayables = new List<Playable>();
        var targetWeights = new List<float>();
        if (audioClips != null)
        {
            for (var i = 0; i < audioClips.Count; i++)
            {
                if (audioClips[i].audioClip == null) continue;

                var playable = AudioClipPlayable.Create(layer.Graph, audioClips[i].audioClip, audioClips[i].loop);
                newPlayables.Add(playable);
                targetWeights.Add(audioClips[i].weight);
            }
        }

        if (audioClearType == AudioClearType.All)
        {
            if (isLerp && groupMixer.GetInputCount() > 0)
                StartFadeReplace(layer, group, groupMixer, newPlayables, targetWeights, true);
            else
                ReplaceGroupMixer(groupMixer, layer.Graph, newPlayables, targetWeights, false);
        }
        else
        {
            for (var i = 0; i < newPlayables.Count; i++)
                groupMixer.AddInput(newPlayables[i], 0, isLerp ? 0 : targetWeights[i]);

            if (isLerp)
                StartFadeIn(layer, group, groupMixer, newPlayables, targetWeights);
        }

        state.Signature = signature;
        state.Weight = 1;
        PlayGraph(layer);
    }

    private void InitializeLayer(AudioLayer layer, Transform root)
    {
        DestroyLayer(layer);
        if (root == null) return;

        layer.Source = root.GetComponent<AudioSource>();
        if (layer.Source == null) return;

        layer.Graph = PlayableGraph.Create(layer.Name);
        layer.Output = AudioPlayableOutput.Create(layer.Graph, layer.Name, layer.Source);
        layer.RootMixer = AudioMixerPlayable.Create(layer.Graph, 0);
        layer.Output.SetSourcePlayable(layer.RootMixer);
    }

    private void DestroyLayer(AudioLayer layer)
    {
        CancelFade(layer);
        layer.GroupMixers.Clear();
        layer.States.Clear();

        if (layer.Graph.IsValid())
            layer.Graph.Destroy();

        layer.Output = AudioPlayableOutput.Null;
        layer.RootMixer = default;
        layer.Source = null;
    }

    private bool EnsureLayerReady(AudioLayer layer)
    {
        return layer.Graph.IsValid() && layer.RootMixer.IsValid();
    }

    private AudioMixerPlayable GetOrCreateGroupMixer(AudioLayer layer, string group)
    {
        group = NormalizeGroup(group);
        if (layer.GroupMixers.TryGetValue(group, out var groupMixer) && groupMixer.IsValid())
            return groupMixer;

        groupMixer = AudioMixerPlayable.Create(layer.Graph, 0);
        layer.GroupMixers[group] = groupMixer;
        layer.RootMixer.AddInput(groupMixer, 0, 1);
        return groupMixer;
    }

    private AudioGroupState GetState(AudioLayer layer, string group)
    {
        group = NormalizeGroup(group);
        if (!layer.States.TryGetValue(group, out var state))
        {
            state = new AudioGroupState();
            layer.States.Add(group, state);
        }

        return state;
    }

    private int NextRequestVersion(AudioLayer layer, string group)
    {
        var state = GetState(layer, group);
        unchecked
        {
            state.RequestVersion++;
        }

        return state.RequestVersion;
    }

    private bool IsRequestCurrent(AudioLayer layer, string group, int version)
    {
        return GetState(layer, group).RequestVersion == version;
    }

    private void PlayGraph(AudioLayer layer)
    {
        if (layer.Graph.IsValid() && !layer.Graph.IsPlaying())
            layer.Graph.Play();
    }

    private void ClearGroupMixer(AudioMixerPlayable mixer, bool unloadClip)
    {
        if (!mixer.IsValid()) return;

        for (var i = mixer.GetInputCount() - 1; i >= 0; i--)
        {
            var playable = mixer.GetInput(i);
            mixer.DisconnectInput(i);
            DestroyPlayable(playable, unloadClip);
        }

        mixer.SetInputCount(0);
    }

    private void ReplaceGroupMixer(AudioMixerPlayable mixer, PlayableGraph graph, List<Playable> playables,
        List<float> weights, bool unloadOldClip)
    {
        ClearGroupMixer(mixer, unloadOldClip);

        if (playables == null) return;

        for (var i = 0; i < playables.Count; i++)
        {
            if (!playables[i].IsValid()) continue;

            var weight = weights != null && i < weights.Count ? weights[i] : 1f;
            mixer.AddInput(playables[i], 0, weight);
        }
    }

    private void StartFadeReplace(AudioLayer layer, string group, AudioMixerPlayable mixer, List<Playable> newPlayables,
        List<float> targetWeights, bool removeOldWhenDone)
    {
        var task = new AudioFadeTask
        {
            Layer = layer,
            Group = NormalizeGroup(group),
            Mixer = mixer,
            RemoveOldWhenDone = removeOldWhenDone
        };

        var oldCount = mixer.GetInputCount();
        for (var i = 0; i < oldCount; i++)
        {
            var playable = mixer.GetInput(i);
            if (!playable.IsValid()) continue;

            task.OldPlayables.Add(playable);
            task.OldStartWeights.Add(mixer.GetInputWeight(i));
        }

        if (newPlayables != null)
        {
            for (var i = 0; i < newPlayables.Count; i++)
            {
                if (!newPlayables[i].IsValid()) continue;

                mixer.AddInput(newPlayables[i], 0, 0);
                task.NewPlayables.Add(newPlayables[i]);
                task.NewTargetWeights.Add(targetWeights != null && i < targetWeights.Count ? targetWeights[i] : 1f);
            }
        }

        if (task.OldPlayables.Count == 0 && task.NewPlayables.Count == 0) return;

        fadeTasks.Add(task);
    }

    private void StartFadeIn(AudioLayer layer, string group, AudioMixerPlayable mixer, Playable playable, float targetWeight)
    {
        StartFadeIn(layer, group, mixer, new List<Playable> { playable }, new List<float> { targetWeight });
    }

    private void StartFadeIn(AudioLayer layer, string group, AudioMixerPlayable mixer, List<Playable> playables,
        List<float> targetWeights)
    {
        var task = new AudioFadeTask
        {
            Layer = layer,
            Group = NormalizeGroup(group),
            Mixer = mixer,
            RemoveOldWhenDone = false
        };

        for (var i = 0; i < playables.Count; i++)
        {
            if (!playables[i].IsValid()) continue;

            task.NewPlayables.Add(playables[i]);
            task.NewTargetWeights.Add(targetWeights != null && i < targetWeights.Count ? targetWeights[i] : 1f);
        }

        if (task.NewPlayables.Count > 0)
            fadeTasks.Add(task);
    }

    private void UpdateFadeTasks(float deltaTime)
    {
        for (var i = fadeTasks.Count - 1; i >= 0; i--)
        {
            var task = fadeTasks[i];
            if (!task.Mixer.IsValid() || !task.Layer.Graph.IsValid())
            {
                fadeTasks.RemoveAt(i);
                continue;
            }

            task.Elapsed += deltaTime;
            var t = task.Duration <= 0 ? 1f : Mathf.Clamp01(task.Elapsed / task.Duration);

            for (var index = 0; index < task.OldPlayables.Count; index++)
            {
                SetPlayableWeight(task.Mixer, task.OldPlayables[index], task.OldStartWeights[index] * (1f - t));
            }

            for (var index = 0; index < task.NewPlayables.Count; index++)
            {
                SetPlayableWeight(task.Mixer, task.NewPlayables[index], task.NewTargetWeights[index] * t);
            }

            if (t < 1f) continue;

            CompleteFadeTask(task);
            fadeTasks.RemoveAt(i);
        }
    }

    private void CompleteFadeTask(AudioFadeTask task)
    {
        if (!task.Mixer.IsValid()) return;

        if (task.RemoveOldWhenDone)
        {
            for (var i = 0; i < task.OldPlayables.Count; i++)
            {
                DisconnectPlayable(task.Mixer, task.OldPlayables[i]);
                DestroyPlayable(task.OldPlayables[i], false);
            }

            CompactMixer(task.Mixer, task.Layer.Graph, task.NewPlayables, task.NewTargetWeights);
        }
        else
        {
            for (var i = 0; i < task.NewPlayables.Count; i++)
                SetPlayableWeight(task.Mixer, task.NewPlayables[i], task.NewTargetWeights[i]);
        }
    }

    private void CompactMixer(AudioMixerPlayable mixer, PlayableGraph graph, List<Playable> playables, List<float> weights)
    {
        if (!mixer.IsValid()) return;

        for (var i = mixer.GetInputCount() - 1; i >= 0; i--)
            mixer.DisconnectInput(i);

        mixer.SetInputCount(0);

        for (var i = 0; i < playables.Count; i++)
        {
            if (!playables[i].IsValid()) continue;

            mixer.AddInput(playables[i], 0, weights[i]);
        }
    }

    private void CancelFade(AudioLayer layer, string group = null)
    {
        for (var i = fadeTasks.Count - 1; i >= 0; i--)
        {
            var task = fadeTasks[i];
            if (task.Layer != layer) continue;
            if (group != null && task.Group != NormalizeGroup(group)) continue;

            fadeTasks.RemoveAt(i);
        }
    }

    private void ClearFadeTasks()
    {
        fadeTasks.Clear();
    }

    private bool TrySetExistingClipWeight(AudioMixerPlayable mixer, AudioClip clip, float weight)
    {
        if (clip == null || !mixer.IsValid()) return false;

        for (var i = 0; i < mixer.GetInputCount(); i++)
        {
            var playable = mixer.GetInput(i);
            if (TryGetClip(playable, out var currentClip) && currentClip == clip)
            {
                mixer.SetInputWeight(i, weight);
                return true;
            }
        }

        return false;
    }

    private void SetClipWeights(AudioLayer layer, string group, AudioClip clip, float weight)
    {
        if (!layer.GroupMixers.TryGetValue(NormalizeGroup(group), out var mixer) || !mixer.IsValid())
            return;

        if (clip == null)
        {
            for (var i = 0; i < mixer.GetInputCount(); i++)
                mixer.SetInputWeight(i, weight);
            return;
        }

        TrySetExistingClipWeight(mixer, clip, weight);
    }

    private void UpdateExistingWeights(AudioLayer layer, string group, List<AudioPlayData> audioClips)
    {
        if (audioClips == null) return;
        if (!layer.GroupMixers.TryGetValue(NormalizeGroup(group), out var mixer) || !mixer.IsValid())
            return;

        for (var i = 0; i < audioClips.Count; i++)
            TrySetExistingClipWeight(mixer, audioClips[i].audioClip, audioClips[i].weight);
    }

    private void SetPlayableWeight(AudioMixerPlayable mixer, Playable playable, float weight)
    {
        if (!mixer.IsValid() || !playable.IsValid()) return;

        for (var i = 0; i < mixer.GetInputCount(); i++)
        {
            if (mixer.GetInput(i).Equals(playable))
            {
                mixer.SetInputWeight(i, weight);
                return;
            }
        }
    }

    private void DisconnectPlayable(AudioMixerPlayable mixer, Playable playable)
    {
        if (!mixer.IsValid() || !playable.IsValid()) return;

        for (var i = mixer.GetInputCount() - 1; i >= 0; i--)
        {
            if (mixer.GetInput(i).Equals(playable))
            {
                mixer.DisconnectInput(i);
                return;
            }
        }
    }

    private bool TryGetClip(Playable playable, out AudioClip clip)
    {
        clip = null;
        if (!playable.IsValid() || !playable.IsPlayableOfType<AudioClipPlayable>())
            return false;

        clip = ((AudioClipPlayable)playable).GetClip();
        return clip != null;
    }

    private void DestroyPlayable(Playable playable, bool unloadClip)
    {
        if (!playable.IsValid()) return;

        // 资源释放交给资源管理器/场景生命周期处理；这里仅销毁 playable，避免切歌时触发 UnloadAsset 抖动。
        playable.Destroy();
    }

    private void SetRootGroupWeight(AudioLayer layer, string group, float value)
    {
        if (!EnsureLayerReady(layer)) return;
        if (!layer.GroupMixers.TryGetValue(NormalizeGroup(group), out var groupMixer) || !groupMixer.IsValid()) return;

        var index = FindInputIndex(layer.RootMixer, groupMixer);
        if (index >= 0)
            layer.RootMixer.SetInputWeight(index, Mathf.Clamp01(value));
    }

    private int FindInputIndex(AudioMixerPlayable mixer, Playable playable)
    {
        if (!mixer.IsValid() || !playable.IsValid()) return -1;

        for (var i = 0; i < mixer.GetInputCount(); i++)
        {
            if (mixer.GetInput(i).Equals(playable))
                return i;
        }

        return -1;
    }

    private void UpdateSeVoices(float deltaTime)
    {
        seCleanTimer += deltaTime;
        if (seCleanTimer < 0.2f) return;

        seCleanTimer = 0;
        foreach (var pair in seVoices)
        {
            var voices = pair.Value;
            for (var i = voices.Count - 1; i >= 0; i--)
            {
                var voice = voices[i];
                if (!voice.Playable.IsValid())
                {
                    voices.RemoveAt(i);
                    continue;
                }

                var clip = voice.Playable.GetClip();
                if (voice.Loop || clip == null) continue;

                if (voice.Playable.GetTime() >= clip.length)
                {
                    DestroySeVoice(voice);
                    voices.RemoveAt(i);
                }
            }
        }
    }

    private void DestroySeVoice(SeVoice voice)
    {
        if (voice == null) return;

        DisconnectPlayable(voice.Mixer, voice.Playable);
        DestroyPlayable(voice.Playable, false);
    }

    private void ClearSeVoices()
    {
        foreach (var pair in seVoices)
        {
            var voices = pair.Value;
            for (var i = 0; i < voices.Count; i++)
                DestroySeVoice(voices[i]);
        }

        seVoices.Clear();
    }

    private List<PendingAudioPlayData> BuildPendingBgsDatas(List<float3> bgs)
    {
        var result = new List<PendingAudioPlayData>();
        if (bgs == null) return result;

        for (var i = 0; i < bgs.Count; i++)
        {
            var bgsType = (BGS)(int)bgs[i].x;
            if (bgsType == BGS.NULL || bgs[i].y <= 0) continue;

            result.Add(new PendingAudioPlayData
            {
                Key = bgsType.ToString(),
                Weight = bgs[i].y,
                Loop = bgs[i].z > 0
            });
        }

        return result;
    }

    private string BuildSignature(List<PendingAudioPlayData> audioClips)
    {
        if (audioClips == null || audioClips.Count == 0) return NullClipKey;

        var signature = string.Empty;
        for (var i = 0; i < audioClips.Count; i++)
        {
            if (i > 0) signature += "|";
            signature += audioClips[i].Key;
            signature += ":";
            signature += audioClips[i].Loop ? "1" : "0";
        }

        return signature;
    }

    private string BuildSignature(List<AudioPlayData> audioClips)
    {
        if (audioClips == null || audioClips.Count == 0) return NullClipKey;

        var signature = string.Empty;
        for (var i = 0; i < audioClips.Count; i++)
        {
            if (audioClips[i].audioClip == null) continue;
            if (signature.Length > 0) signature += "|";
            signature += audioClips[i].audioClip.name;
            signature += ":";
            signature += audioClips[i].loop ? "1" : "0";
        }

        return signature.Length == 0 ? NullClipKey : signature;
    }

    private string GetClipKey(AudioClip clip)
    {
        return clip == null ? NullClipKey : clip.name;
    }

    private string NormalizeGroup(string group)
    {
        return string.IsNullOrEmpty(group) ? "Default" : group;
    }

    private void ApplySavedMixerVolume()
    {
        SetMasterVolume(PlayerPrefs.GetFloat("MasterVolume", 1));
        SetBGMVolume(PlayerPrefs.GetFloat("BGMVolume", 1));
        SetSEVolume(PlayerPrefs.GetFloat("SEVolume", 1));
    }

    private void SaveMixerVolume()
    {
        if (audioMixer == null) return;

        PlayerPrefs.SetFloat("MasterVolume", GetMixerVolume("MasterVolume"));
        PlayerPrefs.SetFloat("BGMVolume", GetMixerVolume("BGMVolume"));
        PlayerPrefs.SetFloat("SEVolume", GetMixerVolume("SEVolume"));
    }

    private float GetMixerVolume(string parameter)
    {
        if (audioMixer == null || !audioMixer.GetFloat(parameter, out var value))
            return PlayerPrefs.GetFloat(parameter, 1);

        return Mathf.Clamp01((value + 40) * 0.025f);
    }

    private void SetMixerVolume(string parameter, float volume)
    {
        var normalized = Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat(parameter, normalized);

        if (audioMixer != null)
            audioMixer.SetFloat(parameter, -40 + 40 * normalized);
    }
}
