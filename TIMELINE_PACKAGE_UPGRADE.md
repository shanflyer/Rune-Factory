# Timeline Package Upgrade Notes

## Scope

Project package:

`Packages/com.unity.timeline`

Reference package used for comparison:

`D:/Init3.2/Library/PackageCache/com.unity.timeline@6b9e48457ddb`

Current project package version:

`com.unity.timeline@1.8.10`

This package has been modified into a project-specific battle/cutscene Timeline system. It should not be replaced with the official package until the custom behavior and serialized data are migrated out of the package.

## Executive Summary

The official Timeline package was extended in several core runtime and editor areas:

- Conditional track playback by gameplay data such as `AttackType`.
- Runtime animation clip override through `TimelineClip`.
- Multi-target prefab control for battle effects.
- Prefab parent selection for single-target, multi-target, and editor preview workflows.
- Curve-driven prefab offset, scale, and rotation directly from Control clips.
- Custom inspectors for the new Timeline fields.
- Unity 6000.3 editor selection compatibility using `EntityId` through an `ObjectId` wrapper.

The project already depends on these APIs from game code and serialized `.playable` assets. Replacing this package with the official package now will cause compile errors and loss of Timeline battle/cutscene behavior.

## Runtime Feature Changes

### 1. Track Match Data

Added `MatchData` in `Runtime/Control/ControlTrack.cs`:

```csharp
[Serializable]
public struct MatchData
{
    public string key;
    public string value;
}
```

Added serialized match data lists to:

- `AudioTrack.matchDatas`
- `ControlTrack.matchDatas`

Project usage:

- `Assets/Scripts/Film/TimeLineManger.cs`
- Reads `matchDatas`.
- Looks for `key == "AttackType"`.
- Mutes `AudioTrack` or `ControlTrack` when the value does not match the current attack type.

Purpose:

- A single Timeline asset can contain multiple alternative audio/effect tracks.
- Runtime logic selects the correct tracks for the current skill or attack type.

Official Timeline does not have this feature.

### 2. Runtime Animation Clip Override

Modified `Runtime/TimelineClip.cs`.

Added:

```csharp
public AnimationClip assetClip { get; }
public AnimationClip overideClip;
public void ClearOverride();
```

Changed `TimelineClip.animationClip` so `overideClip` wins when set:

```csharp
if (overideClip != null)
    return overideClip;
```

Project usage:

- `Assets/Scripts/Film/TimeLineManger.cs`
- Reads `clip.assetClip`.
- Uses the current character animator override table to find a replacement clip.
- Writes the replacement into `clip.overideClip`.

Purpose:

- The same Timeline can be reused by different characters or monsters.
- Animation clips are selected dynamically from the character's `AnimatorOverrideController`.
- No need to duplicate Timeline assets per character animation set.

This is a core package modification. `TimelineClip` is not a normal user-created `PlayableAsset`, so this cannot be migrated by simply subclassing `TimelineClip`.

### 3. Control Clip Multi-Target Prefab Instantiation

Modified `Runtime/Control/ControlPlayableAsset.cs`.

Added:

```csharp
public List<Transform> targets = new List<Transform>();
public bool muliPlayable = false;
```

Runtime behavior:

- If `muliPlayable == false`, the clip behaves like a normal Control clip and creates one prefab instance.
- If `muliPlayable == true`, the clip creates one prefab instance for every target transform in `targets`.

Project usage:

- `Assets/Scripts/Film/TimeLineManger.cs`
- Before playback, iterates `ControlTrack` clips.
- Casts each clip asset to `ControlPlayableAsset`.
- Injects battle targets through `asset.targets = targets`.

Purpose:

- One Control clip can spawn the same effect on multiple battle targets.
- Group attacks and multi-hit skills can use one Timeline clip instead of duplicated tracks.

Official Timeline only supports one prefab instance per Control clip.

### 4. Prefab Parent Selection

Modified `Runtime/Control/ControlPlayableAsset.cs` and `Runtime/Playables/PrefabControlPlayable.cs`.

Added editor-only parent field:

```csharp
#if UNITY_EDITOR
public ExposedReference<Transform> targetParent;
#endif
```

Parent resolution rules:

- Single instance mode: prefab is parented under `sourceGameObject.transform`, matching the original Control clip behavior.
- Editor preview multi-target mode: if `targetParent` is assigned, all children of `targetParent` are used as parent transforms.
- Runtime multi-target mode: `targets` is used, and each target receives one prefab instance.

Relevant behavior:

```csharp
if (Targets.Count == 0)
{
    Transform parentTransform = sourceObj != null ? sourceObj.transform : null;
    PrefabControlPlayable.Create(graph, prefabGameObject, parentTransform);
}
else
{
    foreach (Transform parentTransform in Targets)
        PrefabControlPlayable.Create(graph, prefabGameObject, parentTransform);
}
```

`PrefabControlPlayable.Initialize()` also resets local transform after instantiation:

```csharp
m_Instance.transform.localPosition = Vector3.zero;
m_Instance.transform.localRotation = Quaternion.identity;
```

Purpose:

- Runtime effects are attached to the correct battle target.
- Editor preview can simulate multi-target effects by assigning a parent object with child placeholders.
- The same Timeline can be authored and previewed without running the full battle system.

### 5. Control Clip Transform Curves

Modified:

- `Runtime/Control/ControlPlayableAsset.cs`
- `Runtime/Playables/PrefabControlPlayable.cs`

Added serialized fields:

```csharp
public bool rot;
public float rotAngle;
public Vector3 initOffset;
public Vector3 scale = Vector3.one;
public AnimationCurve offsetXCurve;
public AnimationCurve offsetYCurve;
public AnimationCurve angleZCurve;
```

`PrefabControlPlayable.ProcessFrame()` evaluates these values per frame:

- Normalizes current playable time by clip duration.
- Evaluates X/Y offset curves.
- Evaluates Z rotation curve.
- Applies fixed rotation angle when `rot` is enabled.
- Applies local position, local scale, and local Euler rotation to the instantiated prefab.

Purpose:

- Effect movement can be authored directly on the Control clip.
- Simple projectile, slash, offset, or rotation behavior does not require a separate animation clip.
- Timeline becomes a stronger battle effect authoring tool.

Official Timeline Control clips do not animate prefab transforms this way.

## Editor Feature Changes

### 1. Control Playable Inspector

Modified `Editor/Playables/ControlPlayableInspector.cs`.

Added UI for:

- `targetParent`
- `targets`
- `scale`
- `muliPlayable`
- `initOffset`
- `offsetXCurve`
- `offsetYCurve`
- `angleZCurve`

Purpose:

- Exposes the new Control clip behavior in the Timeline inspector.
- Allows effect movement and preview parent setup without custom external tools.

### 2. Audio Track Inspector

Modified `Editor/Audio/AudioTrackInspector.cs`.

Added UI for:

- `matchDatas`

Purpose:

- Allows authoring conditional audio tracks directly in Timeline.

### 3. Unity 6000.3 Selection Compatibility

Added:

- `Runtime/ObjectId.cs`
- `Runtime/Extensions/SelectionExtensions.cs`
- `Editor/ObjectIdExtension.cs`

Modified many Timeline editor tree/selection files to use `ObjectId` instead of raw `int instanceID`.

Purpose:

- Unity 6000.3 uses `EntityId` APIs in places where old editor code used instance IDs.
- `ObjectId` wraps either `EntityId` or `int`, depending on Unity version.

This is mostly an editor compatibility patch, not battle gameplay functionality.

## Project Code Dependencies

The project currently directly references package-added APIs.

Important usages:

- `Assets/Scripts/Film/TimeLineManger.cs`

Package APIs used there:

- `TimelineClip.assetClip`
- `TimelineClip.overideClip`
- `AudioTrack.matchDatas`
- `ControlTrack.matchDatas`
- `ControlPlayableAsset.targets`
- `ControlPlayableAsset.sourceGameObject`

If the official package is restored now, this script will not compile.

## Serialized Asset Dependencies

Many `.playable` assets already contain serialized custom fields.

Examples of serialized custom fields:

- `overideClip`
- `matchDatas`
- `targetParent`
- `muliPlayable`
- `rot`
- `rotAngle`
- `initOffset`
- `scale`
- `offsetXCurve`
- `offsetYCurve`
- `angleZCurve`

Important asset area:

`Assets/Resources/Film`

Risk:

- Official Timeline does not know these fields.
- Opening and saving assets after package replacement can drop or ignore this data.
- Battle/cutscene behavior will not match the current project.

## Why This Cannot Be Replaced Like UGUI

UGUI changes could be migrated to project-side subclasses such as `GameButton` and `GameImage`.

Timeline is different:

- `TimelineClip` is a core Timeline data type and is not practically replaceable with a subclass in existing assets.
- `AudioTrack` and `ControlTrack` are official track types already used by serialized Timeline assets.
- `ControlPlayableAsset` is the official Control clip asset already embedded in `.playable` files.
- The project code mutates Timeline package objects directly at runtime.
- Existing `.playable` resources already store the custom fields inside official Timeline object types.

Direct package replacement would cause both compile-time and data/runtime behavior failures.

## Migration Strategy For Future Official Package Upgrade

Do not directly delete the custom package. Migrate in phases.

### Phase 1: Preserve Current Behavior

- Keep the current custom `com.unity.timeline`.
- Add tests or manual checklists for representative battle Timelines.
- Identify all `.playable` assets under `Assets/Resources/Film` that use custom fields.
- Export a report of all custom field usage.

### Phase 2: Move Track Condition Data Out Of Official Types

Options:

- Create custom project-side track types such as `GameAudioTrack` and `GameControlTrack`.
- Or store condition metadata in a separate `ScriptableObject` keyed by Timeline asset and track identity.

Recommended:

- Use custom project-side track types where practical.
- Keep `MatchData` in project code, not in package code.

Migration tool requirements:

- Read existing `AudioTrack.matchDatas` and `ControlTrack.matchDatas`.
- Create equivalent project-side metadata.
- Preserve `AttackType` behavior.

### Phase 3: Replace ControlPlayableAsset Custom Behavior

Create project-side custom playable assets:

- `GameControlPlayableAsset`
- `GamePrefabControlPlayable`

Move these fields into the custom asset:

- `muliPlayable`
- `targets`
- `targetParent`
- `initOffset`
- `scale`
- `offsetXCurve`
- `offsetYCurve`
- `angleZCurve`
- `rot`
- `rotAngle`

Migration tool requirements:

- Find official `ControlPlayableAsset` clips with custom fields.
- Replace them with `GameControlPlayableAsset`.
- Copy prefab reference, source object reference, activation/update settings, and all custom transform/multi-target fields.

### Phase 4: Replace TimelineClip Animation Override

This is the hardest part.

Current behavior modifies `TimelineClip.animationClip` itself through `overideClip`.

Safer future options:

- At runtime, clone the Timeline asset and replace `AnimationPlayableAsset.clip` on the cloned asset.
- Or keep a mapping from original clip to override clip in `TimeLineManger` and build a custom animation playable graph.
- Or replace affected animation tracks with custom playable assets that resolve the correct animation clip at graph build time.

Do not rely on official `TimelineClip` to store override data.

Migration tool requirements:

- Preserve original clip references.
- Preserve runtime override mapping behavior.
- Ensure overrides are cleared after playback or isolated to cloned Timeline instances.

### Phase 5: Editor Compatibility

The `ObjectId` / `EntityId` editor compatibility changes should be reviewed separately.

Options:

- If a newer official Timeline package already supports Unity 6000.3, drop these edits.
- If not, keep equivalent compatibility code in project-side editor utilities only where possible.

## Validation Checklist Before Replacing The Package

Before switching to official Timeline, all items below must pass:

- Project compiles with official `com.unity.timeline`.
- `TimeLineManger` no longer references package-added APIs.
- Existing `.playable` assets no longer rely on custom fields inside official Timeline types.
- Battle Timeline plays correct animation per character/monster.
- Audio tracks still filter by `AttackType`.
- Control tracks still filter by `AttackType`.
- Single-target effects instantiate under the correct parent.
- Multi-target effects instantiate once per target.
- Editor preview with `targetParent` or equivalent still works.
- Effect offset, scale, and rotation curves still evaluate correctly.
- Timeline assets can be opened and saved without losing migration data.

## Current Recommendation

Keep the custom Timeline package for now.

This package is not just patched for convenience. It currently contains core battle/cutscene authoring and playback behavior. A future migration should be treated as a dedicated `GameTimeline` refactor with asset migration tooling, not as a package cleanup task.
