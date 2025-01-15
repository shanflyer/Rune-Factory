using GooglePlayGames.BasicApi.SavedGame;
using GooglePlayGames;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GooglePlayGames.BasicApi;
using UnityEngine;
using System.Collections;

public class CloudDataManager:Singleton<CloudDataManager>
{
    public bool IsSave = false;
    Texture2D savedImage;
    string userId;
    public void ShowSelectUI(string userId)
    {
        this.userId = userId;

        uint maxNumToDisplay = 5;
        bool allowCreateNew = true;
        bool allowDelete = true;

        ISavedGameClient savedGameClient = PlayGamesPlatform.Instance.SavedGame;



        savedGameClient.ShowSelectSavedGameUI("Select saved game",
            maxNumToDisplay,
            allowCreateNew,
            allowDelete,
            OnSavedGameSelected);
    }

    private string filename = "data1";

    string dataStr = null;
    public bool OpenSavedGame(string dataStr)
    {
        this.dataStr = dataStr;
        if (PlayGamesPlatform.Instance == null || PlayGamesPlatform.Instance.SavedGame == null)
        {
            return false;
        }
        ISavedGameClient savedGameClient = PlayGamesPlatform.Instance.SavedGame;
        savedGameClient.OpenWithAutomaticConflictResolution(filename, DataSource.ReadCacheOrNetwork,
            ConflictResolutionStrategy.UseOriginal, OnSavedGameOpened);
        return true;
    }

    public void OnSavedGameSelected(SelectUIStatus status, ISavedGameMetadata game)
    {
        if (status == SelectUIStatus.SavedGameSelected)
        {  
            OpenSavedGameLoad(game.Filename);
            // handle selected game save
        }
        else if(status==SelectUIStatus.UiBusy|| status == SelectUIStatus.UserClosedUI)
        {

            GameManager.instance.ShowTwoSelectAction("Error", LanguageManage.SwitchStr("获取云存档失败--") + status + LanguageManage.SwitchStr("是否重试？"), () =>
            {
                ShowSelectUI(userId);
            }, () =>
            {
                Application.Quit();
            });
        }
        else
        {
            GameManager.instance.ShowTwoSelectAction("Error", LanguageManage.SwitchStr("获取云存档失败--") + status + LanguageManage.SwitchStr("是否不使用用云存档？"), () =>
            {
                GameDataSaveManager.instance.InitUserSaveData(userId, null);
            }, () =>
            {
                Application.Quit();
            });
        }
         
    }
    void OpenSavedGameLoad(string filename)
    {
        ISavedGameClient savedGameClient = PlayGamesPlatform.Instance.SavedGame; 

        savedGameClient.OpenWithAutomaticConflictResolution(filename, DataSource.ReadCacheOrNetwork,
            ConflictResolutionStrategy.UseLongestPlaytime, OnSavedGameOpened);
    }

    public void OnSavedGameOpened(SavedGameRequestStatus status, ISavedGameMetadata game)
    {
        if (status == SavedGameRequestStatus.Success)
        {
            if (IsSave)
            {
                if (!string.IsNullOrEmpty(this.dataStr))
                {
                    byte[] savedData = System.Text.Encoding.UTF8.GetBytes(this.dataStr);
                    this.SaveGame(game, savedData);
                } 
            }
            else
            {
                LoadGameData(game);
            }
            // handle reading or writing of saved game.
        }
        else
        {
            // handle error
        }
    }
    void SaveGame(ISavedGameMetadata game, byte[] savedData, TimeSpan totalPlaytime)
    {
        ISavedGameClient savedGameClient = PlayGamesPlatform.Instance.SavedGame;

        SavedGameMetadataUpdate.Builder builder = new SavedGameMetadataUpdate.Builder();
        builder = builder
            .WithUpdatedPlayedTime(totalPlaytime)
            .WithUpdatedDescription("Saved game at " + DateTime.Now);
        if (savedImage != null)
        {
            // This assumes that savedImage is an instance of Texture2D
            // and that you have already called a function equivalent to
            // getScreenshot() to set savedImage
            // NOTE: see sample definition of getScreenshot() method below
            byte[] pngData = savedImage.EncodeToPNG();
            builder = builder.WithUpdatedPngCoverImage(pngData);
        }
        SavedGameMetadataUpdate updatedMetadata = builder.Build();
        savedGameClient.CommitUpdate(game, updatedMetadata, savedData, OnSavedGameWritten);
    }

    public void OnSavedGameWritten(SavedGameRequestStatus status, ISavedGameMetadata game)
    {
        Debug.Log($"33 保存游戏存档结果：{status}");
        if (status == SavedGameRequestStatus.Success)
        {
            this.dataStr = null;
            // handle reading or writing of saved game.
        }
        else
        {
            // handle error
        }
    }

    public Texture2D getScreenshot()
    {
        // Create a 2D texture that is 1024x700 pixels from which the PNG will be
        // extracted
        Texture2D screenShot = new Texture2D(1024, 700);

        // Takes the screenshot from top left hand corner of screen and maps to top
        // left hand corner of screenShot texture
        screenShot.ReadPixels(
            new Rect(0, 0, Screen.width, (Screen.width / 1024) * 700), 0, 0);
        return screenShot;
    }
    void LoadGameData(ISavedGameMetadata game)
    {
        ISavedGameClient savedGameClient = PlayGamesPlatform.Instance.SavedGame;
        savedGameClient.ReadBinaryData(game, OnSavedGameDataRead);
       
    }

    public void OnSavedGameDataRead(SavedGameRequestStatus status, byte[] data)
    {
        Debug.Log($"load游戏存档结果：{status}");
        if (status == SavedGameRequestStatus.Success)
        {
            string str = System.Text.Encoding.Default.GetString(data);
            GameDataSaveManager.instance.InitUserSaveData(userId, str);
            GameController.instance.StartGame();
            // handle processing the byte array data
        }
        else
        {
            // handle error
        }
    }

      
   

    private void SaveGame(ISavedGameMetadata game, byte[] savedData)
    {
        Debug.Log($"22 保存游戏存档");
        ISavedGameClient savedGameClient = PlayGamesPlatform.Instance.SavedGame;

        SavedGameMetadataUpdate.Builder builder = new SavedGameMetadataUpdate.Builder();
        builder = builder.WithUpdatedDescription("Saved game at " + DateTime.Now);

        SavedGameMetadataUpdate updatedMetadata = builder.Build();
        savedGameClient.CommitUpdate(game, updatedMetadata, savedData, OnSavedGameWritten);
    }
 

}