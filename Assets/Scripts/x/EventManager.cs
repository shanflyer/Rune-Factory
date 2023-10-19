using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using LitJson;
using System.IO;

namespace OldName{
    [System.Serializable]
    public enum EventTriggerType
    {
        时间 = 0,
        事件已发生 = 1,
        事件未发生 = 2,
        结局已达成 = 3,
        指引结束 = 4,
        切换地图 = 5,
        特定物体Action = 6,
        开放清单 = 7,
        新开始 = 8
    }
    [System.Serializable]
    public class EventTrigger
    {
        public EventTriggerType endTriggerType;
        public string endTriggerValue;

        public bool CheckTriger()
        {
            switch (endTriggerType)
            {
                case EventTriggerType.事件已发生:
                    GameEvent gameEvent =
                        GameComponentData.gameData.eventManager.gameEvents
                            .Find(g => g.id == int.Parse(endTriggerValue));
                    return gameEvent.end;

                case EventTriggerType.事件未发生:
                    GameEvent gameEvent1 =
                        GameComponentData.gameData.eventManager.gameEvents
                            .Find(g => g.id == int.Parse(endTriggerValue));
                    return !gameEvent1.end;

                case EventTriggerType.时间:
                    var x = endTriggerValue.Split('|');
                    GameDate gameDate = GameTimeManager.instance.nowGameTime.gameDate;
                    if ((int)gameDate.season >= int.Parse(x[0]) && gameDate.date >= int.Parse(x[1]))
                    {
                        return true;
                    }
                    return false;
                //case EventTriggerType.切换地图:
                 //   int passId = int.Parse(endTriggerValue.Split(',')[0]);
                    //return passId == GameComponentData.gameData.passDataManager.nowPass;
                case EventTriggerType.特定物体Action:
                     
                    break;
                case EventTriggerType.开放清单:
                    if (GameComponentData.gameData.heritageAction.nowOpen == int.Parse(endTriggerValue))
                    {
                        GameComponentData.gameData.heritageAction.nowOpen = 0;
                        return true;
                    }
                    break;
                case EventTriggerType.指引结束:
                    Guide nowGuide = GameComponentData.gameData.guideController.nowGuide;
                    if (nowGuide != null && nowGuide.id == int.Parse(endTriggerValue))
                    {
                        return true;
                    }
                    break;
                case EventTriggerType.新开始:
                    return !DataSaveAndLoadTest.isJsonData;

            }

            return false;
        }
    }
    [System.Serializable]
    public enum EventDisplayType
    {
        创建人物 = 1,
        移动到位置 = 2,
        对话 = 3,
        销毁人物 = 4,
        创建动画对象 = 5
    }


    [System.Serializable]
    public class GameEvent
    {
        public string name;
        public int id;
        public int filmId;
        public List<EventTrigger> EventTriggers;
        public bool end;
        public GameEvent() { }

        public GameEvent(GameEventStr gameEventStr)
        {
            name = LanguageManage.SwitchStr(gameEventStr.name);
            id = gameEventStr.id;
            end = false;
            filmId = gameEventStr.filmId;
            EventTriggers = new List<EventTrigger>();
            var x = gameEventStr.EventTriggers.Split(';');
            foreach (var s in x)
            {
                EventTrigger endTrigger = new EventTrigger()
                {
                    endTriggerType = (EventTriggerType)int.Parse(s.Split(',')[0]),
                    endTriggerValue = s.Split(',')[1]
                };
                EventTriggers.Add(endTrigger);
            }
        }

        public bool ChectEventTrigger()
        {
            if (GameComponentData.gameData.gameDebugAction.GameStartTest)
            {
                return false;
            }
            if (end)
            {
                return false;
            }
            foreach (var endTrigger in EventTriggers)
            {
                if (!endTrigger.CheckTriger())
                {
                    return false;
                }
            }
            return true;
        }
    }

    public class GameEventStr
    {
        public string name;
        public int id;
        public int filmId;
        public string EventTriggers;
        public bool end;
        //public string actionDisplays;
        // public string partenrPoses;

        public GameEventStr() { }

        public GameEventStr(GameEvent gameEvent)
        {
            name = gameEvent.name;
            id = gameEvent.id;
            filmId = gameEvent.filmId;
            end = false;
            EventTriggers = "";
            for (int i = 0; i < gameEvent.EventTriggers.Count; i++)
            {
                var gameEndEndTrigger = gameEvent.EventTriggers[i];
                EventTriggers += (int)gameEndEndTrigger.endTriggerType + "," + gameEndEndTrigger.endTriggerValue;
                if (i != gameEvent.EventTriggers.Count - 1)
                {
                    EventTriggers += ";";
                }
            }
        }
    }
    public class EventManager : MonoBehaviour
    {
        public List<GameEvent> gameEvents;

        public List<GameEventStr> gameEventStrs;

        // Use this for initialization
        void Start()
        {

        }
        public void DataToJson()
        {
            gameEventStrs = new List<GameEventStr>();
            foreach (var gameEvent in gameEvents)
            {
                GameEventStr gameEventStr = new GameEventStr(gameEvent);
                gameEventStrs.Add(gameEventStr);
            }


            string path = Application.dataPath + "/Resources/Datas/GameEvents.json";
            if (File.Exists(path))
            {
                File.Delete(path);
            }
            string jsonStr = JsonMapper.ToJson(gameEventStrs);
            FileStream fileStream = new FileStream(path, FileMode.OpenOrCreate);
            StreamWriter stream = new StreamWriter(fileStream);
            stream.Write(jsonStr);
            stream.Close();
        }

        public void JsonToData()
        {
            string path = "Datas/GameEvents";
            TextAsset textAsset = Resources.Load<TextAsset>(path);
            if (textAsset != null)
            {
                string jsonStr = textAsset.text;
                gameEventStrs = JsonMapper.ToObject<List<GameEventStr>>(jsonStr);
                gameEvents = new List<GameEvent>();

                foreach (var gameEventStr in gameEventStrs)
                {
                    GameEvent gameEvent = new GameEvent(gameEventStr);
                    gameEvents.Add(gameEvent);
                }
            }
        }

        public void PlayGameEvent(GameEvent gameEvent)
        {
            GameComponentData.gameData.filmManager.CreatFilm(gameEvent.filmId);
            gameEvent.end = true;

        }

        public bool CheckEvents()
        {
            GameEvent gameEvent = gameEvents.Find(g => g.ChectEventTrigger());
            if (gameEvent != null)
            {
                PlayGameEvent(gameEvent);
                return true;
            }
            return false;
        }


        // Update is called once per frame
        void Update()
        {

        }
    }
}

