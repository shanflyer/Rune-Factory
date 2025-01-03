using System.Collections.Generic;
using UnityEngine;

public class GameVolumeObject : MonoBehaviour
{
    [SerializeField]
    private List<GameObject> ObjList0;
    [SerializeField]
    private List<GameObject> ObjList1;
    [SerializeField]
    private List<GameObject> ObjList2;
    private void OnEnable()
    {
        GameVolumeManager.instance.AddVolumeObject(this);
    }
    private void OnDisable()
    {
        if(!SingletonType.Cleared)
            GameVolumeManager.instance.RemoveVolumeObject(this);
    }
    private int nowLevel=-1;
    public void SetVolumeLevel(int level)
    {
        if (nowLevel == level)
        {
            return;
        }
        nowLevel = level;
        switch(level)
        {
            case 0:
                for(int i = 0; i < ObjList1.Count; i++)
                {
                    if (ObjList1[i])
                        ObjList1[i].SetActive(false);
                }
                for (int i = 0; i < ObjList2.Count; i++)
                {
                    if (ObjList2[i])
                        ObjList2[i].SetActive(false);
                }

                for (int i = 0; i < ObjList0.Count; i++)
                {
                    if (ObjList0[i])
                        ObjList0[i].SetActive(true);
                }
                break;
            case 1:
                for (int i = 0; i < ObjList0.Count; i++)
                {
                    if (ObjList0[i])
                        ObjList0[i].SetActive(false);
                }
                for (int i = 0; i < ObjList2.Count; i++)
                {
                    if (ObjList2[i])
                        ObjList2[i].SetActive(false);
                }

                for (int i = 0; i < ObjList1.Count; i++)
                {
                    if (ObjList1[i])
                        ObjList1[i].SetActive(true);
                }
                break;
            case 2:
                for (int i = 0; i < ObjList1.Count; i++)
                {
                    if (ObjList1[i])
                        ObjList1[i].SetActive(false);
                }
                for (int i = 0; i < ObjList1.Count; i++)
                {
                    if (ObjList1[i])
                        ObjList1[i].SetActive(false);
                }

                for (int i = 0; i < ObjList2.Count; i++)
                {
                    if (ObjList2[i])
                        ObjList2[i].SetActive(true);
                }
                break;
        }
    }
}
