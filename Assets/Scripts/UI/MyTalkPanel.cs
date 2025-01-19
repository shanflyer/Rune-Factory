using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using VoxelBusters.CoreLibrary;
using VoxelBusters.EssentialKit;

public class MyTalkPanel : GamePanel<IReferenceData>
{ 
    [SerializeField]
    Button Rate, Share, Committer, Help0, Help1, Help2;
    [SerializeField]
    Button CloseBtn;
    protected override void Awake()
    {
        base.Awake();
        Rate.onClick.AddListener(() =>
        {
            RateMyApp.AskForReviewNow(skipConfirmation: false);
        });
        Share.onClick.AddListener(ShareAction);

        Committer.onClick.AddListener(()=> { UIManager.instance.ShowGamePanel<CommitterPanel>(); } );

        Help0.onClick.AddListener(() =>
        {
            AppStoreManager.instance.BuyProduct("help0");
        });
        Help1.onClick.AddListener(() =>
        {
            AppStoreManager.instance.BuyProduct("help1");
        });
        Help2.onClick.AddListener(() =>
        {
            AppStoreManager.instance.BuyProduct("help2");
        });

        CloseBtn.onClick.AddListener(Close);
    }
    void ShareAction()
    {
        ShareSheet shareSheet = ShareSheet.CreateInstance();
        shareSheet.AddText("Text");
        shareSheet.AddURL(URLString.URLWithPath("https://www.google.com"));
        shareSheet.SetCompletionCallback((result, error) => {
            Debug.Log("Share Sheet was closed. Result code: " + result.ResultCode);
        });
        shareSheet.Show();
    }
    public override void SetPanelUISerializeObj()
    {
        base.SetPanelUISerializeObj();
        CloseBtn = FindChildGameObject<Button>("Close");
        Rate = FindChildGameObject<Button>("Rate");
        Share = FindChildGameObject<Button>("Share");
        Committer = FindChildGameObject<Button>("Committer");
        Help0 = FindChildGameObject<Button>("Help0");
        Help1 = FindChildGameObject<Button>("Help1"); 
        Help2 = FindChildGameObject<Button>("Help2"); 
    }
    public override void InitReferenceData(IReferenceData v)
    {
        base.InitReferenceData(v);
    }
    public override Task InitData(string dataKey)
    {
        return base.InitData(dataKey);
    }
}
