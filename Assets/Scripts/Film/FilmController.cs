using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Playables;

public class FilmController : Singleton<FilmController>
{
    public struct Film
    {
        public GameObject obj;
        public PlayableDirector playableDirector;
    }


    private Dictionary<string, Film> nowFilms = new Dictionary<string, Film>();
    private Transform filmParent;

    void PlayFilm(PlayFilm playFilm) 
    { 
        if(!nowFilms.TryGetValue(playFilm.filmName,out Film film))
        {
            CreatAndPlayFilm(playFilm.filmName);
        }
        else
        {
            film.playableDirector.Play();
        }
       ;
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

    async Task CreatAndPlayFilm(string filmName)
    {
        string filmPath = GameCommon.AddString(DataPath.filmDataPath, filmName);
        var filmPrefab =await GameSourceManager.instance.GetPrefab(filmPath);
        if (filmPrefab != null)
        {
            GameObject filmObj = GameObject.Instantiate(filmPrefab, filmParent);
            PlayableDirector playableDirector=filmObj.GetComponent<PlayableDirector>();
            Film film = new Film
            {
                obj = filmObj,
                playableDirector = playableDirector
            };
            playableDirector.Play();
            nowFilms.Add(filmName, film);
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
    }
    protected override void Clear()
    {
        base.Clear();
        //GameActionManager.instance.RemoveListener<PlayFilm>(PlayFilm);
        //GameActionManager.instance.RemoveListener<StopFilm>(StopFilm);
        //GameActionManager.instance.RemoveListener<PauseFilm>(PauseFilm);
    }
}
