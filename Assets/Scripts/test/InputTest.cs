using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputTest : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        InputManager.instance.AddInputActionDelegate(MyInputNameData.Player_Move, MoveAction);
    }
    private void MoveAction(object obj)
    {
        var moveValue = (Vector2)obj;
        Debug.Log($"Move:{moveValue}");
    }

    // Update is called once per frame
    void Update()
    {

    }
}
