using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Object = System.Object;

[CreateAssetMenu(menuName = "Data/GameActionData")]
 
[System.Serializable]
public class GameActionData : ScriptableObject,IGameData
{ 
    public string dataName;
    public int id;
   
    public string typeName;
    public List<Parameter> _parameters;

    public void Action()
    { 
        switch (typeName)
        {
            case "StopFilm":
                StopFilm stopFilm = new StopFilm();
                stopFilm.Init(_parameters);
                GameActionManager.instance.QueueAction(stopFilm);
                break;
            case "PlayFilm":
                PlayFilm playFilm = new PlayFilm();
                playFilm.Init(_parameters);
                GameActionManager.instance.QueueAction(playFilm);
                break;
            case "PauseFilm":
                PauseFilm pauseFilm = new PauseFilm();
                pauseFilm.Init(_parameters);
                GameActionManager.instance.QueueAction(pauseFilm);
                break;
            case "SetItemAnimation":
                SetItemAnimation SetItemAnimation = new SetItemAnimation();
                SetItemAnimation.Init(_parameters);

                GameActionManager.instance.QueueAction(SetItemAnimation);
                break;
            case "RemovePackageItem":
                RemovePackageItem RemovePackageItem = new RemovePackageItem();
                RemovePackageItem.Init(_parameters);

                GameActionManager.instance.QueueAction(RemovePackageItem);
                break;
            case "AddPackageItem":
                AddPackageItem AddPackageItem = new AddPackageItem();
                AddPackageItem.Init(_parameters);

                GameActionManager.instance.QueueAction(AddPackageItem);
                break;
            case "TriggerEnter":
                TriggerEnter TriggerEnter = new TriggerEnter();
                TriggerEnter.Init(_parameters);

                GameActionManager.instance.QueueAction(TriggerEnter);
                break;
            case "ChangeMapItem":
                ChangeMapItem changeMapItem = new ChangeMapItem();
                changeMapItem.Init(_parameters);
                GameActionManager.instance.QueueAction(changeMapItem);
                break;
            case "TriggerExit":
                TriggerExit TriggerExit = new TriggerExit();
                TriggerExit.Init(_parameters);

                GameActionManager.instance.QueueAction(TriggerExit);
                break;
            case "DeleteMapItem":
                DeleteMapItem DeleteMapItem = new DeleteMapItem();
                DeleteMapItem.Init(_parameters);

                GameActionManager.instance.QueueAction(DeleteMapItem);
                break;
            case "AddMapItem":
                AddMapItem AddMapItem = new AddMapItem();
                AddMapItem.Init(_parameters);

                GameActionManager.instance.QueueAction(AddMapItem);
                break;
            case "AttachMapItemData":
                AttachMapItemData AttachMapItemData = new AttachMapItemData();
                AttachMapItemData.Init(_parameters);

                GameActionManager.instance.QueueAction(AttachMapItemData);
                break;
            case "CreatRuntimePackage":
                CreatRuntimePackage CreatRuntimePackage = new CreatRuntimePackage();
                CreatRuntimePackage.Init(_parameters);

                GameActionManager.instance.QueueAction(CreatRuntimePackage);
                break;
            case "RemoveRuntimePackage":
                RemoveRuntimePackage RemoveRuntimePackage = new RemoveRuntimePackage();
                RemoveRuntimePackage.Init(_parameters);

                GameActionManager.instance.QueueAction(RemoveRuntimePackage);
                break;
            case "ItemUseAction":
                ItemUseAction ItemUseAction = new ItemUseAction();
                ItemUseAction.Init(_parameters);

                GameActionManager.instance.QueueAction(ItemUseAction);
                break;
            case "ClosePanelAction":
                ClosePanelAction ClosePanelAction = new ClosePanelAction();
                ClosePanelAction.Init(_parameters);

                GameActionManager.instance.QueueAction(ClosePanelAction);
                break;
            case "OpenPanelAction":
                OpenPanelAction OpenPanelAction = new OpenPanelAction();
                OpenPanelAction.Init(_parameters);

                GameActionManager.instance.QueueAction(OpenPanelAction);
                break;
            case "SetCharacterProperty":
                SetCharacterProperty SetCharacterProperty = new SetCharacterProperty();
                SetCharacterProperty.Init(_parameters);

                GameActionManager.instance.QueueAction(SetCharacterProperty);
                break;
            case "ChangeCharacterProperty":
                ChangeCharacterProperty ChangeCharacterProperty = new ChangeCharacterProperty();
                ChangeCharacterProperty.Init(_parameters);

                GameActionManager.instance.QueueAction(ChangeCharacterProperty);
                break;
            case "CharacterPropertyTrigger":
                CharacterPropertyTrigger CharacterPropertyTrigger = new CharacterPropertyTrigger();
                CharacterPropertyTrigger.Init(_parameters);

                GameActionManager.instance.QueueAction(CharacterPropertyTrigger);
                break;
            case "SetCharacterCoordinate":
                SetCharacterCoordinate SetCharacterCoordinate = new SetCharacterCoordinate();
                SetCharacterCoordinate.Init(_parameters);

                GameActionManager.instance.QueueAction(SetCharacterCoordinate);
                break;
            case "CharacterCoordinateTrigger":
                CharacterCoordinateTrigger CharacterCoordinateTrigger = new CharacterCoordinateTrigger();
                CharacterCoordinateTrigger.Init(_parameters);

                GameActionManager.instance.QueueAction(CharacterCoordinateTrigger);
                break;
        }         
    }
#if UNITY_EDITOR
    public void SetReferenceData()
    {
    }
#endif
    public string GetKey()
    {
        return id.ToString();
    }
}
[System.Serializable]
public struct Parameter
{
    public string value;
    public List<Parameter> parameters;
}