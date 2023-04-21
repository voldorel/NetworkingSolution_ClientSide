using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MessageView : MonoBehaviour
{
    [SerializeField]
    private Text _messageText;

    [SerializeField]
    private GameObject _messageViewObject;

    [SerializeField]
    private GameObject _loadingObject;


    [SerializeField]
    private GameObject _messageWindow;


    public static MessageView instance;


    private void Awake()
    {
        DontDestroyOnLoad(this);
        if (instance != null)
            DestroyImmediate(instance);
        instance = this;
    }


    private void Start()
    {
        _loadingObject.SetActive(false);
        _messageViewObject.SetActive(false);
        _messageWindow.SetActive(false);
    }




    public static void ShowLoadingView(bool value)
    {
        instance._messageWindow.SetActive(false);
        instance._loadingObject.SetActive(value);
        instance._messageViewObject.SetActive(value);
    }


    public static void ShowMessage(string textValue)
    {
        instance._loadingObject.SetActive(false);
        instance._messageWindow.SetActive(true);
        instance._messageViewObject.SetActive(true);
        instance._messageText.text = textValue;
    }

    public void CloseMessage()
    {
        instance._messageViewObject.SetActive(false);
    }
}
