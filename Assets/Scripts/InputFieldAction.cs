using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Text;
using UnityEngine.UI;

public class InputFieldAction : MonoBehaviour
{
    public Text shuruTitle;
    public Text NameText;
    public string inputStr;
    public void InputAction(InputField inputField)
    {
        string inputText = inputField.text;
        inputText = Ctr(inputText, 15);
        inputField.text=inputText;
        inputStr = inputText;
    }
	// Use this for initialization
	void Start ()
	{
	    shuruTitle.text = LanguageManage.SwitchStr(shuruTitle.text);
	}

    
    public int Text_Length(string Text)
    {
        int len = 0;

        for (int i = 0; i < Text.Length; i++)
        {
            byte[] byte_len = Encoding.Default.GetBytes(Text.Substring(i, 1));
            if (byte_len.Length > 1)
                len += 2;  //如果长度大于1，是中文，占两个字节，+2
            else
                len += 1;  //如果长度等于1，是英文，占一个字节，+1
        }

        return len;
    }
    public static string Ctr(string Text, int Num)
    {
        string _text = "";
        if (Text.Length > Num)
        {
            
            for (int i = 0; i < Num-1; i++)
            {
                _text += Text.Substring(i, 1);
            }
        }
        else
        {
            _text = Text;
        }
        return _text;
        /*
        for (int i = 0; i < Text.Length; i++)
        {
            byte[] byte_len = Encoding.Default.GetBytes(Text.Substring(i, 1));
            if (byte_len.Length > 1)
                len += 2;  //如果长度大于1，是中文，占两个字节，+2
            else
                len += 1;  //如果长度等于1，是英文，占一个字节，+1
        }


        string StrNum = pstr;
        byte[] bytes1 = System.Text.Encoding.Default.GetBytes(StrNum.Trim());
        List<byte> bytes = new List<byte>();
        int icha = bytes1.Length;
        if (icha > Num)
        {
            for(int i = 0; i< Num; i++)
            {
                bytes.Add(bytes1[i]);
            }
            byte[] bytes2=bytes.ToArray();
            StrNum = System.Text.Encoding.Default.GetString(bytes2);
        }
        return StrNum;
        */
    }

    // Update is called once per frame
    void Update () {
		
	}
}
