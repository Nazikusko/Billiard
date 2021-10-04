using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIMessageBoxScript : MonoBehaviour
{
    Text textCompotent;

    public void ShowMessage(string txtmessage)
    {
        textCompotent = transform.Find("Message").GetComponent<Text>();
        textCompotent.text = txtmessage;
        gameObject.SetActive(true);
    }

    public void HideMessageBox()
    {
        gameObject.SetActive(false);
    }
}
