
using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName ="Data/tag对应音效")]
public class TagAudioDataList : ScriptableObject
{
    public List<TagAudioData> tagAudioDatas; 
}
[Serializable]
public struct TagAudioData
{
    public string tag;
    public AudioClip audioClip;
}