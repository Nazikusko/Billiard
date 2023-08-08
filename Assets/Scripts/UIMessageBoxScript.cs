using System;
using UnityEngine;
using UnityEngine.UI;

public class UIMessageBoxScript : MonoBehaviour
{
    [SerializeField] private Text _textMessage;
    [SerializeField] private Button _okButton;

    public void ShowMessage(string message, Action onButtonClick)
    {
        _textMessage.text = message;
        gameObject.SetActive(true);
        
        _okButton.onClick.AddListener(() =>
        {
            onButtonClick?.Invoke();
            HideMessageBox();
        });
    }

    void OnDisable()
    {
        _okButton.onClick.RemoveAllListeners();
    }

    public void HideMessageBox()
    {
        gameObject.SetActive(false);
    }
}
