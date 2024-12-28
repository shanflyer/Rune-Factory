 
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline; 

public class FilmController : Singleton<FilmController>
{
    public struct Film
    {
        public GameObject obj;
        public PlayableDirector playableDirector;
    }


    private Dictionary<string, Film> nowFilms = new Dictionary<string, Film>();
    private Transform filmParent;

    async void PlayFilm(PlayFilm playFilm) 
    { 
        if(!nowFilms.TryGetValue(playFilm.filmName,out Film film))
        {
           await CreatAndPlayFilm(playFilm.filmName,playFilm.assetName);
        }
        else
        {
            FilmData filmData = await GameDataManager.instance.GetAsyncData<FilmData>(playFilm.filmName);
            if (filmData != null)
            {
                var assetData = filmData.GetTimeLineAsset(playFilm.assetName);

                if (film.playableDirector.playableAsset!=null&&!string.IsNullOrEmpty(playFilm.assetName)&&
                    playFilm.assetName != film.playableDirector.playableAsset.name)
                {
                    BindFilm(assetData, film.playableDirector);
                }
                else
                {
                    film.playableDirector.transform.localScale = Vector3.one;
                    film.playableDirector.Play(); 
                }
            }
            else
            {
                film.playableDirector.transform.localScale = Vector3.one;
                film.playableDirector.Play();
            }
        };
    }

    void BindFilm(TimelineAssetData assetData, PlayableDirector playableDirector)
    {
        if (assetData.asset == null)
        {
            playableDirector.transform.localScale = Vector3.one;
            //playableDirector.Stop();
            playableDirector.time = 0;
            playableDirector.Play();
            return;
        }
        playableDirector.playableAsset = assetData.asset;
        TimelineAsset timelineAsset = assetData.asset;
        using (var playBindings = timelineAsset.outputs.GetEnumerator())
        {
            int i = 0;
            while (playBindings.MoveNext() && i < assetData.pathes.Count)
            {
                string streamName = playBindings.Current.streamName;
                if (streamName == "Markers")
                {
                    continue;
                }

                Object sourceObject = playBindings.Current.sourceObject; 
                
                Transform child =playableDirector.transform.Find(assetData.pathes[i]);
                if (child != null)
                {
                    if(child.gameObject.TryGetComponent(out Animator component))
                    {
                        playableDirector.SetGenericBinding(sourceObject, component.gameObject);
                    }
                }
                else
                {
                    var strs = assetData.pathes[i].Split('/');
                    if (strs.Length>0&&strs[0] == "Camera")
                    {
                        var path = assetData.pathes[i].Replace($"{strs[0]}/", "");
                        child = CameraManager.instance.mainCamera.transform.parent.Find(path);
                        if (child)
                        {
                            if (child.gameObject.TryGetComponent(out Animator component))
                            {
                                playableDirector.SetGenericBinding(sourceObject, component.gameObject);
                            } 
                        }
                    }
                    else
                    {
                        playableDirector.SetGenericBinding(sourceObject, playableDirector);
                    }
                    
                }
                 
                i++;
            }
        }
        playableDirector.transform.localScale = Vector3.one;
        //playableDirector.Stop();
        playableDirector.time = 0;
        playableDirector.Play();
    }
    void DisplayFilm(DisplayFilm DisplayFilm)
    {
        if (nowFilms.TryGetValue(DisplayFilm.filmName, out var film))
        {
            if (film.playableDirector != null)
            {
                if (string.IsNullOrEmpty(DisplayFilm.path))
                {
                    film.playableDirector.transform.localScale = Vector3.one;
                }
                else
                {
                    Transform child = film.playableDirector.transform.Find(DisplayFilm.path);
                    child.localScale = Vector3.one;
                }

            }
        }
    }
    void HideFilm(HideFilm hideFilm)
    {
        if (nowFilms.TryGetValue(hideFilm.filmName, out var film))
        {
           if(film.playableDirector != null)
            {
                if (string.IsNullOrEmpty(hideFilm.path))
                {
                    film.playableDirector.transform.localScale = Vector3.zero;
                }
                else
                {
                    Transform child = film.playableDirector.transform.Find(hideFilm.path);
                    child.localScale = Vector3.zero;
                }
                
            }
        }
    }

    void JumpFilm(JumpFilm jumpFilm)
    {
        if(nowFilms.TryGetValue(jumpFilm.filmName,out var film))
        {
            var filmAsset =(TimelineAsset) film.playableDirector.playableAsset;

            var Track = filmAsset.GetOutputTrack(0);
            Track.muted = true;
            film.playableDirector.time = jumpFilm.jumpTime;
            Track.muted = false;
        }
    }
    void StopFilm(StopFilm stopFilm) 
    {
        if (nowFilms.TryGetValue(stopFilm.filmName, out Film film))
        {
            film.playableDirector.Stop();
            DestoryFilm(stopFilm.filmName);
        }
        
    }
    void PauseFilm(PauseFilm pauseFilm) 
    {
        if (nowFilms.TryGetValue(pauseFilm.filmName, out Film film))
        {
            film.playableDirector.Pause();
        }
    }

    async Task CreatAndPlayFilm(string filmName,string assetName)
    {
        FilmData filmData=await GameDataManager.instance.GetAsyncData<FilmData>(filmName);
        if (filmData != null)
        {
            if (filmData.FilmObj)
            {
                var asyncInstantiateOperation= GameObject.InstantiateAsync(filmData.FilmObj, filmParent);
                await asyncInstantiateOperation;
                GameObject filmObj = asyncInstantiateOperation.Result[0];
                PlayableDirector playableDirector = filmObj.GetComponent<PlayableDirector>();
                playableDirector.stopped += (PlayableDirector) => 
                {
                    if (filmData.stopTimeRun)
                    {
                        TimeRun timeRun = new TimeRun
                        {
                            run = true,
                        };
                        GameActionManager.instance.QueueAction(timeRun);
                    }
                    if (UIManager.instance!=null)
                        UIManager.instance.CloseGamePanel<FilmPanel>();
                };
                BindFilm(filmData.GetTimeLineAsset(assetName), playableDirector);
                Film film = new Film
                {
                    obj = filmObj,
                    playableDirector = playableDirector
                };
                playableDirector.transform.localScale = Vector3.one;
                playableDirector.Play();
                nowFilms.Add(filmName, film);

                if (filmData.stopTimeRun)
                {
                    TimeRun timeRun = new TimeRun
                    {
                        run = false,
                    };
                    GameActionManager.instance.QueueAction(timeRun);
                }
            }
        }
        else
        {
            string filmPath = GameCommon.AddString(DataPath.filmDataPath, filmName);
            var filmPrefab = await GameSourceManager.instance.GetPrefab(filmPath);
            if (filmPrefab != null)
            {
                var asyncInstantiateOperation = GameObject.InstantiateAsync(filmPrefab, filmParent);
                await asyncInstantiateOperation;
                var filmObj = asyncInstantiateOperation.Result[0];
                PlayableDirector playableDirector = filmObj.GetComponent<PlayableDirector>();
                playableDirector.stopped += (PlayableDirector) => 
                { 
                    UIManager.instance.CloseGamePanel<FilmPanel>();
                    if (filmData.displayCharacter)
                    {
                        SetCharacterStopCreate setCharacterStopCreate = new SetCharacterStopCreate
                        {
                            hide = false
                        };
                        GameActionManager.instance.QueueAction(setCharacterStopCreate);
                    }
                };
                 
                Film film = new Film
                {
                    obj = filmObj,
                    playableDirector = playableDirector
                };
                playableDirector.transform.localScale = Vector3.one;
                playableDirector.Play();
                nowFilms.Add(filmName, film);

                if (filmData.stopTimeRun)
                {
                    TimeRun timeRun = new TimeRun
                    {
                        run = false,
                    };
                    GameActionManager.instance.QueueAction(timeRun);
                }
            }
        } 
    }
    void DestoryFilm(string filmName)
    {
        if(nowFilms.TryGetValue(filmName,out Film film))
        {
            GameObject.Destroy(film.obj);
            nowFilms.Remove(filmName);
        }
        
    }

    public void SetParent(Transform filmParent)
    {
        this.filmParent = filmParent;
    }
    public override void Init()
    {
        base.Init();
        GameActionManager.instance.AddListener<PlayFilm>(PlayFilm);
        GameActionManager.instance.AddListener<StopFilm>(StopFilm);
        GameActionManager.instance.AddListener<PauseFilm>(PauseFilm);
        GameActionManager.instance.AddListener<JumpFilm>(JumpFilm);
        GameActionManager.instance.AddListener<HideFilm>(HideFilm);
        GameActionManager.instance.AddListener<DisplayFilm>(DisplayFilm);
    }
    protected override void Clear()
    {
        base.Clear();
        //GameActionManager.instance.RemoveListener<PlayFilm>(PlayFilm);
        //GameActionManager.instance.RemoveListener<StopFilm>(StopFilm);
        //GameActionManager.instance.RemoveListener<PauseFilm>(PauseFilm);
    }
}
