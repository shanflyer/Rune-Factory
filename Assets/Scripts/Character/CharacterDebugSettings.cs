using System;
using System.Collections.Generic;
using UnityEngine;

public enum CharacterDebugEventType
{
    BehaviorChange,
    ScheduleChange,
    ScheduleWarning
}

public readonly struct CharacterDebugEvent
{
    public CharacterDebugEvent(CharacterDebugEventType type, int characterId, string characterName, string message)
    {
        time = DateTime.Now;
        this.type = type;
        this.characterId = characterId;
        this.characterName = characterName;
        this.message = message;
    }

    public readonly DateTime time;
    public readonly CharacterDebugEventType type;
    public readonly int characterId;
    public readonly string characterName;
    public readonly string message;

    public override string ToString()
    {
        return $"{time:HH:mm:ss.fff} {type} characterId={characterId} character={characterName ?? "null"} {message}";
    }
}

public static class CharacterDebugSettings
{
    private const int DefaultEventCapacity = 128;
    private static readonly Queue<CharacterDebugEvent> Events = new Queue<CharacterDebugEvent>(DefaultEventCapacity);

    public static bool EnableBehaviorLogs { get; set; }
    public static bool EnableScheduleLogs { get; set; }
    public static bool EnableEventCapture { get; set; } = true;
    public static int EventCapacity { get; set; } = DefaultEventCapacity;

    public static CharacterDebugEvent[] GetRecentEvents()
    {
        return Events.ToArray();
    }

    public static void ClearEvents()
    {
        Events.Clear();
    }

    public static void RecordEvent(CharacterDebugEventType type, int characterId, string characterName, string message,
        bool writeToConsole)
    {
        var debugEvent = new CharacterDebugEvent(type, characterId, characterName, message);
        if (EnableEventCapture)
        {
            int capacity = Math.Max(1, EventCapacity);
            while (Events.Count >= capacity)
            {
                Events.Dequeue();
            }

            Events.Enqueue(debugEvent);
        }

        if (writeToConsole)
        {
            Debug.Log(debugEvent.ToString());
        }
    }
}
