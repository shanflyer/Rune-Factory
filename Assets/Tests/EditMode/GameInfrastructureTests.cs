using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public sealed class GameInfrastructureTests
{
    private struct TestGameAction : GameAction
    {
        public SetValue setValue { get; set; }
        public SetResult setResult { get; set; }

        public void Clear()
        {
            this = default;
        }

        public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1,
            SetResult setResult = null, SetValue setValue = null, bool immediately = false)
        {
            this.setResult = setResult;
            this.setValue = setValue;
            GameActionManager.instance.QueueAction(this, immediately);
        }
    }

    [Test]
    public void DataPath_HasCoreRegistrations()
    {
        Assert.That(DataPath.GetDataPath(typeof(GameGlobalData)), Is.EqualTo("Data/GameGlobalData"));
        Assert.That(DataPath.GetDataPath(typeof(ItemData)), Is.EqualTo("Data/ItemData"));
        Assert.That(DataPath.GetDataPath(typeof(GameActionAsset)), Is.EqualTo("Data/GameActionAssets"));
    }

    [Test]
    public void DataIntegrityReport_ChecksRegisteredPaths()
    {
        var report = GameDataManager.ValidateDataPathRegistry();

        Assert.That(report, Is.Not.Null);
        Assert.That(report.checkedCount, Is.EqualTo(DataPath.dataPathDic.Count));
    }

    [Test]
    public void SaveEncryption_RoundTripsPlainText()
    {
        const string raw = "{\"save\":\"ok\"}";

        string encrypted = GameDataSaveManager.EncryptDES(raw);
        string decrypted = GameDataSaveManager.DecryptDES(encrypted);

        Assert.That(decrypted, Is.EqualTo(raw));
    }

    [Test]
    public void SaveSerialization_RoundTripsCommonData()
    {
        var saveDataList = new UserGameSaveDataList
        {
            commonSaveData = new CommonSaveData
            {
                saveVersion = 1,
                diamond = 99
            }
        };

        string json = GameDataSaveManager.ObjToString(saveDataList);
        var result = GameDataSaveManager.StringToObj<UserGameSaveDataList>(json);

        Assert.That(result.commonSaveData.diamond, Is.EqualTo(99));
        Assert.That(result.commonSaveData.saveVersion, Is.EqualTo(1));
    }

    [Test]
    public void ActionDispatch_ContinuesAfterListenerException()
    {
        int callCount = 0;
        GameActionManager.ActionDelegate<TestGameAction> throwingListener = _ => throw new System.InvalidOperationException("test");
        GameActionManager.ActionDelegate<TestGameAction> countingListener = _ => callCount++;

        try
        {
            LogAssert.Expect(LogType.Error, "GameActionManager listener failed: type=GameInfrastructureTests+TestGameAction, listener=GameInfrastructureTests.<ActionDispatch_ContinuesAfterListenerException>b__5_0");
            GameActionManager.instance.AddListener(throwingListener);
            GameActionManager.instance.AddListener(countingListener);

            GameActionManager.instance.TriggerAction(new TestGameAction());

            Assert.That(callCount, Is.EqualTo(1));
        }
        finally
        {
            GameActionManager.instance.RemoveListener(throwingListener);
            GameActionManager.instance.RemoveListener(countingListener);
        }
    }
}
