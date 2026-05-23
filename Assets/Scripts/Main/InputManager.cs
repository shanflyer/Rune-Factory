using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using static UnityEngine.InputSystem.InputAction;

public delegate void InputActionDelegate(object value);
public class InputManager :Singleton<InputManager>
{
    private const string UIActionMap = "UI";
    private const string PlayerActionMap = "Player";
     
    private PlayerInput playerInput;
    public InputActionMap playerAction;
    public InputActionMap uiAction;
    public InputActionAsset inputActions;

    private Dictionary<string, InputActionDelegate> performDelegates = new Dictionary<string, InputActionDelegate>();
    private Dictionary<string, InputActionDelegate> cancelDelegates = new Dictionary<string, InputActionDelegate>();
    private Task initializationTask = Task.CompletedTask;
    public override Task InitializationTask => initializationTask;

    public InputManager()
    {
       
        /*
       playerInput = UnityEngine.Object.FindObjectOfType<PlayerInput>();

       clickAction=playerInput.actions.FindAction("Click");
        clickAction.performed += MapClickAction;

        InputAction move = playerInput.actions.FindAction("Move");
        move.performed += MoveAction;

        move.canceled += EndMoveAction;

        InputAction test = playerInput.actions.FindAction("Test");
        test.performed += TestAction;*/
    }

   
    protected override void Clear()
    {
        initializationTask = Task.CompletedTask;
        base.Clear();
        using(var e = performDelegates.GetEnumerator())
        {
            while (e.MoveNext())
            {
                var inputActionDelegate = e.Current.Value;
                inputActionDelegate = null;

            }
        }
        using (var e = cancelDelegates.GetEnumerator())
        {
            while (e.MoveNext())
            {
                var inputActionDelegate = e.Current.Value;
                inputActionDelegate = null;

            }
        }
        performDelegates.Clear();
        cancelDelegates.Clear();
    }

    Dictionary<string, InputAction> InputActions = new Dictionary<string, InputAction>();
    ParticleSystem particleSystem;

    void ShowPointerEffect(object obj)
    {
        var mouseScreenPos = (Vector2)obj;
        Vector2 mousePos = CameraManager.instance.mainCamera.ScreenToWorldPoint(mouseScreenPos);
        ParticleSystem.EmitParams ep = new ParticleSystem.EmitParams();
        ep.position = mousePos;
        particleSystem.Emit(ep, 1);
    }

    public void SetControllerValue()
    {
       
    }

    EventSystem eventSystem;
    public override void Init()
    {
        base.Init();
        initializationTask = InitAsync();
    }

    private async Task InitAsync()
    {
        GameActionManager.instance.AddListener<SwitchInputMap>(SwitchInputMap);
        GameActionManager.instance.AddListener<OpenOrCloseInputMap>(OpenOrCloseInputMap);

        var eventSystems =GameObject.FindObjectsByType<EventSystem>(FindObjectsSortMode.None);
        if (eventSystems.Length > 1)
        {
            GameObject.Destroy(eventSystems[1].gameObject);
        }
        if (eventSystems.Length > 0)
            eventSystem = eventSystems[0];

        playerInput = UnityEngine.Object.FindFirstObjectByType<PlayerInput>();

        if (playerInput == null)
        {
            GameObject inputController = new GameObject("InputController");
            playerInput = inputController.AddComponent<PlayerInput>();
            playerInput.actions = await GameSourceManager.instance.GetScriptableObject<InputActionAsset>(DataPath.InputDataPath);
            if (playerInput.actions == null)
            {
                Debug.LogError($"InputManager init failed: missing input action asset at {DataPath.InputDataPath}");
                return;
            }

            var pointerPre = await GameSourceManager.instance.GetPrefab(DataPath.pointerEffectPath);
            if (pointerPre == null)
            {
                Debug.LogError($"InputManager init failed: missing pointer prefab at {DataPath.pointerEffectPath}");
                return;
            }

            var asyncInstantiateOperation = GameObject.InstantiateAsync(pointerPre, inputController.transform);
            await asyncInstantiateOperation;
            var pointerObj = asyncInstantiateOperation.Result[0];
            particleSystem=pointerObj.GetComponent<ParticleSystem>();
        }
        if (playerInput == null || playerInput.actions == null)
        {
            Debug.LogError("InputManager init failed: PlayerInput or InputActionAsset is null.");
            return;
        }

        var actionMaps= playerInput.actions.actionMaps;
        foreach(var actionMap in actionMaps)
        {
            if(actionMap.name== PlayerActionMap)
            {
                playerAction = actionMap;
            }
            if (actionMap.name == UIActionMap)
            {
                uiAction = actionMap;
            }
            var actions = actionMap.actions;

            for (int i = 0; i < actions.Count; i++)
            {
                var action = actions[i];
               // Debug.Log($"actionName:{action.name}");
                void PerformedDelegate(CallbackContext callbackContext)
                {
                    if (!action.enabled)
                    {
                        return;
                    }
                    if (performDelegates.TryGetValue(action.name, out var del))
                    {
                        del.Invoke(callbackContext.ReadValueAsObject());
                    }
                };
                action.performed += PerformedDelegate;
                

                void CanceledDelegate(CallbackContext callbackContext)
                {
                    if (!action.enabled)
                    {
                        return;
                    }
                    if (cancelDelegates.TryGetValue(action.name, out var del))
                    {
                       // Debug.Log("Canceled");
                        del.Invoke(null);
                    }
                };
                action.canceled += CanceledDelegate;

                InputActions[action.name] = action; 
            }

           actionMap.Enable();
        }

        
        playerInput.defaultActionMap =PlayerActionMap;

        //AddInputActionDelegate(MyInputNameData.Player_Pointer, ShowPointerEffect);
        //AddInputActionDelegate(MyInputNameData.Other_Pointer, ShowPointerEffect);

        
    }
    void OpenOrCloseInputMap(OpenOrCloseInputMap OpenOrCloseInputMap)
    {
        if (uiAction != null)
        {
            if (OpenOrCloseInputMap.open)
            { 
                uiAction.Enable();
            }
            else
            {
                uiAction.Disable();
            }
        }

        if (playerAction != null)
        {
            if (OpenOrCloseInputMap.open)
            {
                if (!OnlyUI)
                {
                    playerAction.Enable();
                } 
            }
            else
            {
                playerAction.Disable();
            }
        }
    }
    void SwitchInputMap(SwitchInputMap switchInputMap)
    {
        SwitchInputMap(switchInputMap.UI);
    }

    private bool OnlyUI = false;
    public void SwitchInputMap(bool UI)
    {
        if (uiAction != null && playerAction != null)
        {
            OnlyUI = UI;
            if (UI)
            {
                //uiAction.Enable();
                playerAction.Disable();
            }
            else
            {
                //uiAction.Disable();
                playerAction.Enable();
            }
           
          //  playerInput.currentActionMap = UI ? uiAction : playerAction;
        }
       // 
       // playerInput.defaultActionMap = UI ? UIActionMap : PlayerActionMap;
    }

    public void AddInputActionDelegate(string actionName, InputActionDelegate inputActionDelegate,bool cancledAction=false)
    {
        if(performDelegates.TryGetValue(actionName,out InputActionDelegate nowDelegate))
        {
            nowDelegate += inputActionDelegate; 
        }
        else
        {
            nowDelegate = inputActionDelegate;
        }
        performDelegates[actionName] = nowDelegate;

        if (cancledAction)
        { 
            if(cancelDelegates.TryGetValue(actionName, out InputActionDelegate cancledDelegate))
            {
                cancledDelegate += inputActionDelegate;
            }
            else
            {
                cancledDelegate = inputActionDelegate;
            }
            cancelDelegates[actionName] = cancledDelegate;
        }
       
    }
    public void RemoveInputActionDelegate(string actionName, InputActionDelegate inputActionDelegate)
    {
        if (performDelegates.TryGetValue(actionName, out InputActionDelegate nowDelegate))
        {
            nowDelegate -= inputActionDelegate; 
            if (nowDelegate == null)
            {
                performDelegates.Remove(actionName);
            }
            else
            {
                performDelegates[actionName] = nowDelegate;
            }
        }
        if ( cancelDelegates.TryGetValue(actionName, out InputActionDelegate cancledDelegate))
        {
            cancledDelegate -= inputActionDelegate;
            if (cancledDelegate == null)
            {
                cancelDelegates.Remove(actionName);
            }
            else
            {
                cancelDelegates[actionName] = cancledDelegate;
            } 
        }
    }
 
}
