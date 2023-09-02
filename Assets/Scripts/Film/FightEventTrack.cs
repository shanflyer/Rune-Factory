using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[TrackClipType(typeof(FightEventAsset))]
public class FightEventTrack : TrackAsset
{
    public int skill;
    [SerializeField]
    private SkillEstimateData skillEstimateData;
    public override void GatherProperties(PlayableDirector director, IPropertyCollector driver)
    {
        base.GatherProperties(director, driver);
    }
    protected override Playable CreatePlayable(PlayableGraph graph, GameObject gameObject, TimelineClip clip)
    {
        return base.CreatePlayable(graph, gameObject, clip);
    }

    public void SetSkillEstimateData(SkillEstimateData skillEstimateData)
    { 
        this.skillEstimateData=skillEstimateData;
        foreach (var clip in GetClips())
        {
            var fightEventAsset = clip.asset as FightEventAsset;

            if (fightEventAsset == null)
                continue;

            fightEventAsset.SetSkillEstimateData(skillEstimateData);

        }
    }
}