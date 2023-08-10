using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitJson;
using System.IO;
[System.Serializable]
public class ShopItem
{
    public int ItemId;
    public int OpenTitleId;
    public bool IsOpen;
}
[System.Serializable]
public class ShopPackage
{
    public string PackageName;
    public List<ShopItem> ShopItems;
}

[System.Serializable]
public enum ShopType
{
    植物店=1,
    动物店=2,
    工具店=3,
    家具店=4,
    饮食店=5,
    魔法小屋=6,
    花店 =7,

}

[System.Serializable]
public class ShopStr
{
    public int id;
    public string Name;
    public int mapid;
    public int Npcid;
    public string packageName0;
    public string ShopItems0;
    public string packageName1;
    public string ShopItems1;
    public ShopType shopType;
    public ShopStr() { }

    public ShopStr(Shop shop)
    {
        id = shop.id;
        Name = shop.Name;
        mapid = shop.mapid;
        Npcid = shop.Npcid;
        packageName0 = "";
        ShopItems0 = "";
        packageName1 = "";
        ShopItems1 = "";
        packageName0 = shop.ShopPackages[0].PackageName;
        foreach (var shopItem in shop.ShopPackages[0].ShopItems)
        {
            if (shopItem.IsOpen)
            {
                ShopItems0 += shopItem.ItemId + ",1";
            }
            else
            {
                ShopItems0 += shopItem.ItemId + ",0";
            }
            ShopItems0 += ";";
        }

        if (shop.ShopPackages.Count == 2)
        {
            packageName1 = shop.ShopPackages[1].PackageName;
            
            foreach (var shopItem in shop.ShopPackages[1].ShopItems)
            {
                if (shopItem.IsOpen)
                {
                    ShopItems1 += shopItem.ItemId + ",1";
                }
                else
                {
                    ShopItems1 += shopItem.ItemId + ",0";
                }
                ShopItems1 += ";";
            }
        }
        shopType = shop.shopType;
    }
}
[System.Serializable]
public class Shop
{
    public int id;
    public string Name;
    public int mapid;
    public int Npcid;
    public List<ShopPackage> ShopPackages;
    [HideInInspector]
    public List<Package> packages;
    public ShopType shopType;
    public Shop() { }

    public Shop(ShopStr shopStr)
    {
        id = shopStr.id;
        Name = shopStr.Name;
        mapid = shopStr.mapid;
        Npcid = shopStr.Npcid;
        shopType = shopStr.shopType;
        ShopPackages=new List<ShopPackage>();
        var x = shopStr.ShopItems0.Split(';');
        ShopPackage shopPackage = new ShopPackage
        {
            PackageName = shopStr.packageName0,
            ShopItems = new List<ShopItem>()
        };

        foreach (var s in x)
        {
            ShopItem shopItem = new ShopItem
            {
                ItemId = int.Parse(s.Split(',')[0]),
                OpenTitleId = int.Parse(s.Split(',')[1])
            };

            if (s.Split(',')[1] == "1")
            {
                shopItem.IsOpen = true;
            }
            else
            {
                shopItem.IsOpen = false;
            }
            shopPackage.ShopItems.Add(shopItem);
        }
        ShopPackages.Add(shopPackage);
        if (shopStr.packageName1 !="")
        {
            var y = shopStr.ShopItems1.Split(';');
            ShopPackage shopPackage1 = new ShopPackage
            {
                PackageName = shopStr.packageName1,
                ShopItems = new List<ShopItem>()
            };
            foreach (var s in y)
            {
                ShopItem shopItem = new ShopItem
                {
                    ItemId = int.Parse(s.Split(',')[0]),
                    OpenTitleId = int.Parse(s.Split(',')[1])
                };
                if (s.Split(',')[1] == "1")
                {
                    shopItem.IsOpen = true;
                }
                else
                {
                    shopItem.IsOpen = false;
                }
                shopPackage1.ShopItems.Add(shopItem);
            }
            ShopPackages.Add(shopPackage1);
        }
        if (GameComponentData.gameData != null)
        {
            InitPackageData();
        }
        
    }

    public void CheckCharactorTitle()
    {
        foreach (var shopPackage in ShopPackages)
        {
            foreach (var shopPackageShopItem in shopPackage.ShopItems)
            {
                if (shopPackageShopItem.OpenTitleId == 1)
                {
                    shopPackageShopItem.IsOpen = true;
                }
                if (shopPackageShopItem.OpenTitleId != 0&&shopPackageShopItem.OpenTitleId != 1)
                {
                    if (GameComponentData.gameData.charactorTitleAction.CharactorTitles
                        .Exists(c => c.id == shopPackageShopItem.OpenTitleId&&c.isGet))
                    {
                        shopPackageShopItem.IsOpen = true;
                    }
                }
                
            }
        }
    }
    public void InitPackageData()
    {
        packages=new List<Package>();
        foreach (var shopPackage in ShopPackages)
        {
            Package package = new Package
            {
                name = shopPackage.PackageName,
                items = new List<Item>()
            };
            foreach (var shopPackageShopItem in shopPackage.ShopItems)
            {
                if (shopPackageShopItem.IsOpen)
                {
                    Item item=new Item(shopPackageShopItem.ItemId,1);
                    package.items.Add(item);
                }
            }
            packages.Add(package);
        }
    }
}
public class ShopManager : MonoBehaviour
{
    public List<Shop> Shops;
    [HideInInspector]
    public List<ShopStr> ShopStrs;
    public GameObject ShopPanelObj;
    // Use this for initialization
    void Start ()
    {
        

    }

    public void InitData()
    {
        JsonToData();
    }
    public void DataToJson()
    {
        string filePath = Application.dataPath + @"/Resources/Datas/" + "ShopDatas.json";
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
        ShopStrs=new List<ShopStr>();
        foreach (var shop in Shops)
        {
            ShopStr shopStr=new ShopStr(shop);
            ShopStrs.Add(shopStr);
        }

        string jsonStr = JsonMapper.ToJson(ShopStrs);
        FileStream fileStream = new FileStream(filePath, FileMode.OpenOrCreate);
        StreamWriter sw = new StreamWriter(fileStream);
        sw.Write(jsonStr);
        sw.Close();
    }
    public void JsonToData()
    {
        TextAsset file = Resources.Load<TextAsset>("Datas/ShopDatas");
        if (file != null)
        {
            ShopStrs = JsonMapper.ToObject<List<ShopStr>>(file.text);
            Shops=new List<Shop>();
            foreach (var shopStr in ShopStrs)
            {
                Shop shop=new Shop(shopStr);
               
                if (GameComponentData.gameData != null)
                {
                    shop.Name = LanguageManage.SwitchStr(shop.Name);
                    foreach (var shopPackage in shop.packages)
                    {
                        shopPackage.name = LanguageManage.SwitchStr(shopPackage.name);
                    }
                    foreach (var shopShopPackage in shop.ShopPackages)
                    {
                        shopShopPackage.PackageName = LanguageManage.SwitchStr(shopShopPackage.PackageName);
                    }
                }
               
               
                Shops.Add(shop);
            }

       
        }
        else
        {
            Debug.Log(file.name + "不存在");
        }


    }
    public void InitShopData(int shopId)
    {
        Shop shop = Shops.Find(s => s.id == shopId);
        shop.CheckCharactorTitle();
        shop.InitPackageData();
        ShopPanelObj.SetActive(true);
        ShopPanelObj.GetComponent<ShopPanelAction>().InitShopPanelData(shop);
    }
	// Update is called once per frame
	void Update () {
		
	}
}
