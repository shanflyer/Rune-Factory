using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public static class TMPTextLocalization
{
    public static Func<string, string> SwitchString;
    public static Func<float> NowLineSpacing;
    public static Func<float> NowCharacterSpacing;
    public static Func<bool> IsRTL;

    static readonly ConditionalWeakTable<TMP_Text, TextState> TextStates = new ConditionalWeakTable<TMP_Text, TextState>();

    public static void FixedSwitchString(this TMP_Text text)
    {
        if (text == null)
            return;

        var state = TextStates.GetOrCreateValue(text);
        if (string.IsNullOrEmpty(state.OriginalText))
            state.OriginalText = text.text;

        SetNativeText(text, SwitchString != null ? SwitchString(state.OriginalText) : state.OriginalText, state);
    }

    public static void InitTextState(TMP_Text text)
    {
        if (text == null)
            return;

        TextStates.GetOrCreateValue(text).CaptureSpacing(text);
    }

    public static void SetOriginalText(TMP_Text text, string value)
    {
        if (text == null)
            return;

        var state = TextStates.GetOrCreateValue(text);
        state.OriginalText = value;
        state.CaptureSpacing(text);
    }

    public static void SetSWText(this TMP_Text text, string source, string args = null)
    {
        if (text == null)
            return;

        string value = SwitchString != null ? SwitchString(source) : source;
        if (args != null)
        {
            string switchedArgs = SwitchString != null ? SwitchString(args) : args;
            value = string.Format(value, switchedArgs);
        }

        SetOriginalAndText(text, source, value);
    }

    public static string SetSWText(this TMP_Text text, object source, params object[] args)
    {
        if (text == null)
            return string.Empty;

        string sourceText = source?.ToString() ?? string.Empty;
        string value = SwitchString != null ? SwitchString(sourceText) : sourceText;
        if (args != null)
        {
            var switchedArgs = new object[args.Length];
            for (int i = 0; i < args.Length; i++)
            {
                string argText = args[i]?.ToString() ?? string.Empty;
                switchedArgs[i] = SwitchString != null ? SwitchString(argText) : argText;
            }

            value = string.Format(value, switchedArgs);
        }

        SetOriginalAndText(text, sourceText, value);
        return text.text;
    }

    public static void SetADDText(this TMP_Text text, object source, params object[] args)
    {
        if (text == null)
            return;

        string sourceText = source?.ToString() ?? string.Empty;
        var builder = new StringBuilder(SwitchString != null ? SwitchString(sourceText) : sourceText);
        if (args != null)
        {
            for (int i = 0; i < args.Length; i++)
            {
                string argText = args[i]?.ToString() ?? string.Empty;
                builder.Append(SwitchString != null ? SwitchString(argText) : argText);
            }
        }

        SetOriginalAndText(text, sourceText, builder.ToString());
    }

    static void SetOriginalAndText(TMP_Text text, string original, string value)
    {
        var state = TextStates.GetOrCreateValue(text);
        state.OriginalText = original;
        SetNativeText(text, value, state);
    }

    static void SetNativeText(TMP_Text text, string value, TextState state)
    {
        if (!state.Initialized)
            state.CaptureSpacing(text);

        if (NowLineSpacing != null)
            text.lineSpacing = state.OriginalLineSpacing + NowLineSpacing();
        if (NowCharacterSpacing != null)
            text.characterSpacing = state.OriginalCharacterSpacing + NowCharacterSpacing();
        if (IsRTL != null)
            text.isRightToLeftText = IsRTL();

        text.text = value;
    }

    sealed class TextState
    {
        public bool Initialized;
        public string OriginalText;
        public float OriginalLineSpacing;
        public float OriginalCharacterSpacing;

        public void CaptureSpacing(TMP_Text text)
        {
            if (Initialized || text == null)
                return;

            Initialized = true;
            OriginalLineSpacing = text.lineSpacing;
            OriginalCharacterSpacing = text.characterSpacing;
        }
    }
}

public static class SelectableGuideRegistry
{
    public static Action<string> SetStringAction;
    public static Action<int, Selectable> SetIntAction;
    public static Action<int, Selectable> RemoveIntAction;

    public static void InitListSelectable(this Selectable selectable, int index)
    {
        if (selectable is IGuideSelectable guideSelectable)
            guideSelectable.InitListSelectable(index);
    }

    public static void SetHideSelected(this Selectable selectable, bool value)
    {
        if (selectable is IGuideSelectable guideSelectable)
            guideSelectable.HideSelected = value;
    }

    public static void InvokeClick(this Selectable selectable)
    {
        if (selectable == null)
            return;

        var eventData = new PointerEventData(EventSystem.current)
        {
            button = PointerEventData.InputButton.Left
        };
        ExecuteEvents.Execute<IPointerClickHandler>(selectable.gameObject, eventData, ExecuteEvents.pointerClickHandler);
    }
}

public interface IGuideSelectable
{
    bool HideSelected { get; set; }
    void InitListSelectable(int index);
}

public static class UGUISelectableUtility
{
    public static void Register(Selectable selectable, int setUid, ref int guid)
    {
        if (selectable == null || setUid == 0)
            return;

        if (guid == 0)
            guid = setUid;

        if (guid > 0)
            SelectableGuideRegistry.SetIntAction?.Invoke(guid, selectable);
    }

    public static void Unregister(Selectable selectable, int guid)
    {
        if (selectable == null || guid <= 0)
            return;

        SelectableGuideRegistry.RemoveIntAction?.Invoke(guid, selectable);
    }

    public static void InitListSelectable(Selectable selectable, int setUid, ref int guid, int index)
    {
        if (selectable == null || setUid == 0)
            return;

        guid = setUid + index;
        Register(selectable, setUid, ref guid);
    }

    public static void PlayTagAudio(Component component)
    {
        if (component == null || component.CompareTag("Untagged"))
            return;

        SelectableGuideRegistry.SetStringAction?.Invoke(component.tag);
    }
}

public static class GameImageExtensions
{
    public static void SetGradientColorTR(this Graphic graphic, Color color)
    {
        if (graphic is GameImage image)
            image.colorTR = color;
    }
}

public static class EventSystemCompatibility
{
    static readonly MethodInfo EventSystemUpdateMethod = typeof(EventSystem).GetMethod("Update", BindingFlags.Instance | BindingFlags.NonPublic);

    public static void FixUpdate(this EventSystem eventSystem)
    {
        if (eventSystem == null)
            return;

        if (EventSystemUpdateMethod != null)
            EventSystemUpdateMethod.Invoke(eventSystem, null);
        else
            eventSystem.UpdateModules();
    }
}
