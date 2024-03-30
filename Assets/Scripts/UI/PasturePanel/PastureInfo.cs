using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PastureInfo : UIObjReference<Pasture>
{
    [SerializeField]
    Image P0, P1, P2;
    [SerializeField]
    Toggle toggle;
    private void Awake()
    {
        toggle.onValueChanged.AddListener((bool value) =>
        {
            if (SelectAction != null)
            {
                SelectAction(data,value);
            }
        });
    }
    public override void OnEnable()
    {
        base.OnEnable();
        GameActionManager.instance.AddListener<RefreshPasture>(RefreshPasture);
    }
    public override void OnDisable()
    {
        base.OnDisable();
        GameActionManager.instance.RemoveListener<RefreshPasture>(RefreshPasture);
    }
     void RefreshPasture(RefreshPasture refreshPasturee)
    {
        if (data.instanceId == refreshPasturee.instanceId)
        {
           if(PastureManager.instance.GetPasture(data.instanceId,out data))
            {
                P0.enabled = P1.enabled = P2.enabled = false; 
                switch (data.level)
                {
                    case 0:
                        P0.enabled = true;
                        break;
                    case 1:
                        P1.enabled = true;
                        break;
                    case 2:
                        P2.enabled = true;
                        break;
                }
            }
        }
    }
    public override void SetPanelUISerializeObj()
    {
        base.SetPanelUISerializeObj();
        toggle = GetComponent<Toggle>();
        P0 = FindChildGameObject<Image>("P0");
        P1 = FindChildGameObject<Image>("P1");
        P2 = FindChildGameObject<Image>("P2");
    }
    public override void SelectDefault()
    {
        base.SelectDefault();
        toggle.isOn = true;
    }
    public override Task InitData(Pasture t, SelectAction<Pasture> SelectAction = null, ToggleGroup toggleGroup = null)
    {
        data = t;
        P0.enabled = P1.enabled = P2.enabled = false;
        toggle.SetIsOnWithoutNotify(false);
        switch (data.level)
        {
            case 0:
                P0.enabled = true;
                break;
            case 1:
                P1.enabled = true;
                break;
            case 2:
                P2.enabled = true;
                break;
        } 
        return base.InitData(t, SelectAction, toggleGroup);
    }

}
