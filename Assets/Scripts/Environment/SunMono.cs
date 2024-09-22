using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SunMono : MonoBehaviour
{
    [SerializeField]
    ProFlareBatch ProFlareBatch;
    HashSet<Collider2D> collider2Ds = new HashSet<Collider2D>();
    private void Awake()
    {
        collider2Ds.Clear();
        if (ProFlareBatch)
        {
            ProFlareBatch.SetTrigger2DGameObject(false);
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        collider2Ds.Add(collision);
        if (ProFlareBatch)
        {
            ProFlareBatch.SetTrigger2DGameObject(true);
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        collider2Ds.Remove(collision);
        if (collider2Ds.Count == 0)
        {
            if (ProFlareBatch)
            {
                ProFlareBatch.SetTrigger2DGameObject(false);
            }
        }
    }
     
}
