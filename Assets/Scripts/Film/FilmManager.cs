using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LitJson;
using System.IO;
using UnityEngine;
using UnityEngine.Playables;
[System.Serializable]
public enum FilmType
{
    普通=0,
    起始=1,
    结局=2,
    结婚=3,
    开始结束=4,
    怀孕=5,
    生子=6,
    礼物=7
}
public enum FilmDisplayType
{
    对话=1,
    过渡=2,
    创建新影片=3,
    销毁=4,
    播放影片=5,
    初始化=6,
    取名=7,
    选择=8,
    播放BGM=9,
    停止BGM=10
}

[System.Serializable]
public struct FilmData
{
    
    public FilmDisplayType filmDisplayType;
    public string value;
}

public class FilmStr
{
    public string Name;
    public int id;
    public string filmObjName;
    public string filmDatas;
   
    public bool useFilmCamera;
    public string filmMap;
    public int filmType;
    public FilmStr() { }

    public FilmStr(Film film)
    {
        Name = film.Name;
        id = film.id;
        filmObjName = film.filmObjName;
        for (int i = 0; i < film.filmDatas.Count; i++)
        {
            filmDatas = + (int)film.filmDatas[i].filmDisplayType + "," + film.filmDatas[i].value;
            if (i < film.filmDatas.Count - 1)
            {
                filmDatas += ";";
            }
        }

        useFilmCamera = film.useFilmCamera;
        filmMap = film.filmMap;
        filmType = (int) film.filmType;
    }
}
[System.Serializable]
public class Film
{
    
    public string Name;
    public int id;
    public string filmObjName;
    public List<FilmData> filmDatas;
    public int index;
    public bool useFilmCamera;
    public string filmMap;
    public FilmType filmType;
    public Film() { }

    public Film(FilmStr filmStr)
    {
        Name = filmStr.Name;
        id = filmStr.id;
        filmObjName = filmStr.filmObjName;
        filmDatas=new List<FilmData>();
        var x = filmStr.filmDatas.Split(';');
        foreach (var s in x)
        {
            FilmData filmData=new FilmData();
            filmData.filmDisplayType = (FilmDisplayType)int.Parse(s.Split(',')[0]);
            filmData.value = s.Split(',')[1];
            filmDatas.Add(filmData);
        }
        index = 0;
        useFilmCamera = filmStr.useFilmCamera;
        filmMap = filmStr.filmMap;
        filmType = (FilmType)filmStr.filmType;
    }
}
public class FilmManager : MonoBehaviour
{
    public GameObject TopGameObject;
    public GameObject SetNamePanel;
    public Transform filmMapParent;
    public GameObject filmCamera;
    public GameObject camera0;
    public List<Film> films;
    private List<FilmStr> filmStrs;
    public GameObject filmUI;
    [HideInInspector]
    public Film nowFilm;

    private GameObject filmObj;

    public bool startFilmOpen;
	// Use this for initialization
	void Start () {
		
	}


    public void DataToJson()
    {
        filmStrs=new List<FilmStr>();
        foreach (var film in films)
        {
            FilmStr filmStr=new FilmStr(film);
            filmStrs.Add(filmStr);
        }


        string path = Application.dataPath + "/Resources/Datas/Films.json";
        if (File.Exists(path))
        {
            File.Delete(path);
        }
        string jsonStr = JsonMapper.ToJson(filmStrs);
        FileStream fileStream = new FileStream(path, FileMode.OpenOrCreate);
        StreamWriter stream = new StreamWriter(fileStream);
        stream.Write(jsonStr);
        stream.Close();
    }

    public void JsonToData()
    {
        string path = "Datas/Films";
        TextAsset textAsset = Resources.Load<TextAsset>(path);
        if (textAsset != null)
        {
            string jsonStr = textAsset.text;
            filmStrs = JsonMapper.ToObject<List<FilmStr>>(jsonStr);
            films=new List<Film>();

            foreach (var filmStr in filmStrs)
            {
                Film film = new Film(filmStr);
                films.Add(film);
            }
        }
    }

    public void CreatFilm(int filmId)
    {
        AudioController.instance.StopBgm();
        camera0.SetActive(false);
        TopGameObject.SetActive(false);
        Film selectFilm = films.Find(f => f.id == filmId);
        if (selectFilm != null)
        {
            filmUI.SetActive(true);
            
            foreach (Transform child in filmMapParent)
            {
                Destroy(child.gameObject);
            }
            if (selectFilm.filmMap != "")
            {
                GameObject filmMapPro =
                    GameComponentData.gameData.gameManager.mapPros.Find(m => m.name == selectFilm.filmMap);
                GameObject filmMap = Instantiate(filmMapPro);
                filmMap.transform.SetParent(filmMapParent,false);
                filmMap.transform.localScale=Vector3.one;
            }
            nowFilm = selectFilm;
            GameObject filmPro = Resources.Load<GameObject>("FilmObj/"+nowFilm.filmObjName);
            filmObj = Instantiate(filmPro);
            filmObj.transform.SetParent(transform,false);
            if (selectFilm.useFilmCamera)
            {
                Camera.main.transform.position = filmCamera.transform.position;
            }
            else
            {
                if (filmObj.transform.GetChild(0).name == "Camera")
                {

                    Camera.main.transform.position = filmObj.transform.GetChild(0).position;
                }
                else
                {
                    Camera.main.transform.position = new Vector3(0, 0, -10);
                    camera0.SetActive(true);
                }
            }
            

            SetCoupleObjDisplayData();
            StartCoroutine("AfterCreatPlay");
        }
    }

    IEnumerator AfterFilm()
    {
        yield return new WaitForSeconds(0.2f);
        GameComponentData.gameData.guideController.filmId = nowFilm.id;
        GameComponentData.gameData.guideController.CheckGuide();
        nowFilm = null;
    }
    IEnumerator AfterCreatPlay()
    {
        yield return new WaitForSeconds(0.2f);
        PlayNowFilm();
    }
    public void PlayNowFilm()
    {
        filmUI.SetActive(true);
        if (nowFilm.index >= nowFilm.filmDatas.Count)
        {
            filmUI.SetActive(false);
            camera0.SetActive(false);
            TopGameObject.SetActive(true);
            //GameComponentData.gameData.gameManager.playerCharactor.Obj.SetActive(true);
            Camera.main.transform.position = new Vector3(0, 0, -10.0f);
            foreach (Transform child in filmMapParent)
            {
                Destroy(child.gameObject);
            }
            Destroy(filmObj);
            
            switch (nowFilm.filmType)
            {
                    case FilmType.普通:
                        GameComponentData.gameData.passDataManager.PlayerMapBGM();
                    break;
                    case FilmType.结婚:
                    GameComponentData.gameData.loveAction.WeddingEnd();
                    break;
                    case FilmType.结局:
                    break;
                    case FilmType.起始:
                        GameComponentData.gameData.gameManager.ZeroFight();
                    break;
                    case FilmType.开始结束:
                        GameComponentData.gameData.gameManager.gamePlayer.TeamPlayer0 = null;
                        GameComponentData.gameData.gameManager.gamePlayer.TeamPlayer1 = null;
                    GameComponentData.gameData.gameManager.InitRoom();
                    break;
                    case FilmType.怀孕:
                        DataSaveAndLoadTest.gameSaveData.marryData.SavePregnancyTime();
                    break;
                    case FilmType.生子:
                        DataSaveAndLoadTest.gameSaveData.marryData.SaveHaveChildrenTimeTime();
                        DataSaveAndLoadTest.gameSaveData.marryData.SetChildData();
                        GameComponentData.gameData.gameManager.CheckChild();
                    break;
                    case FilmType.礼物:

                    break;
            }
            StartCoroutine("AfterFilm");
        }
        else
        {
            FilmData filmData = nowFilm.filmDatas[nowFilm.index];
            nowFilm.index++;
            DisplayFilmData(filmData);
            
        }
        
    }

    public void DisplayFilmData(FilmData _filmData)
    {
      
        switch (_filmData.filmDisplayType)
        {
                case FilmDisplayType.创建新影片:
                   // GameComponentData.gameData.gameManager.playerCharactor.Obj.SetActive(false);
                    Destroy(this.filmObj);
                    CreatFilm(int.Parse(_filmData.value));

                break;
                case FilmDisplayType.销毁:
                    GameObject obj = transform.Find(_filmData.value).gameObject;
                    if (obj != null)
                    {
                        Destroy(obj);
                    }
                break;
                case FilmDisplayType.对话:
                GameComponentData.gameData.talkTextsManager.TalkAction(_filmData.value,TalkActionType.剧情);
                break;
                case FilmDisplayType.过渡:
                    AudioController.instance.StopBgm();
                    GameComponentData.gameData.sleepObj.SetActive(true);
                    GameComponentData.gameData.sleepObj.GetComponent<WaitPanelAction>().InitData(WaitType.Transition,
                        _filmData.value);
                break;
                case FilmDisplayType.播放影片:
                    var films=transform.GetComponentsInChildren<PlayableDirector>().ToList();
                    if (films.Exists(f => f.gameObject.name == _filmData.value))
                    {
                        films.Find(f => f.gameObject.name == _filmData.value).Play();
                }
                break;
                case FilmDisplayType.初始化:
                   GameComponentData.gameData.gameTimeManager.StopTimeRun();
                  
                
                PlayNowFilm();
                break;
            case FilmDisplayType.取名:
                SetNamePanel.SetActive(true);
                break;
            case FilmDisplayType.播放BGM:
               // AudioManager.PlayBGM(PlayType.CYCLE,_filmData.value);
                break;
            case FilmDisplayType.停止BGM:
                AudioController.instance.StopBgm();
                break;
        }
    }
    public void SetCoupleObjDisplayData()
    {
        GameObject mealObj, femealObj;
        mealObj = GameObject.FindGameObjectWithTag("mealObj");
        femealObj = GameObject.FindGameObjectWithTag("femealObj");
        if (GameComponentData.gameData.NpcManager.Npcxs.Exists(n => n.isMarried))
        {
            if (mealObj != null && femealObj != null)
            {
                GameObject _mealobj, _femealObj;
                GamePlayer gamePlayer = GameComponentData.gameData.gameManager.gamePlayer;
                NPCData npcData = GameComponentData.gameData.NpcManager.Npcxs.Find(n => n.isMarried).npcData;
                if (gamePlayer.gender == Gender.male)
                {
                    _mealobj = GameComponentData.gameData.gameManager.playerCharactor.Obj;
                    _femealObj = GameComponent.models.Find(m => m.name == npcData.ObjName);
                }
                else
                {
                    _femealObj = GameComponentData.gameData.gameManager.playerCharactor.Obj;
                    _mealobj = GameComponent.models.Find(m => m.name == npcData.ObjName);
                }
                List<GameObject> s0 = new List<GameObject>();
                List<GameObject> s1 = new List<GameObject>();
                foreach (Transform child in _femealObj.transform.GetChild(0))
                {
                    s0.Add(child.gameObject);
                }
                foreach (Transform child in femealObj.transform.GetChild(0))
                {
                    s1.Add(child.gameObject);
                }
                for (int i = 0; i < s0.Count; i++)
                {
                    s1[i].GetComponent<SpriteRenderer>().sprite = s0[i].GetComponent<SpriteRenderer>().sprite;
                }

                List<GameObject> s2 = new List<GameObject>();
                List<GameObject> s3 = new List<GameObject>();
                foreach (Transform child in _mealobj.transform.GetChild(0))
                {
                    s2.Add(child.gameObject);
                }
                foreach (Transform child in mealObj.transform.GetChild(0))
                {
                    s3.Add(child.gameObject);
                }
                for (int i = 0; i < s0.Count; i++)
                {
                    s2[i].GetComponent<SpriteRenderer>().sprite = s3[i].GetComponent<SpriteRenderer>().sprite;
                }
            }
        }
        
    }
    public void FilmEndAction()
    {
        PlayNowFilm();
    }
    // Update is called once per frame
    void Update () {
		
	}
}
