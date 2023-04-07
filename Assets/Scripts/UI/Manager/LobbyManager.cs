using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MyNetwork;

public class LobbyManager : MonoBehaviour
{
    [SerializeField]
    private InputField _inputText;
    private GameLobby _gameLobby { get; set; }


    public void Start()
    {
        _gameLobby = gameObject.AddComponent<GameLobby>();
        _gameLobby.OnReceiveMessage += (e) => OnReceiveText(e);
        _gameLobby.OnMatchMakingSuccess += () => OnMatchMakingSuccess();
    }

    public void OnClickSendTextMessage()
    {
        SendText(_inputText.text);
    }

    public async void Update()
    {
        if (Input.GetKeyUp(KeyCode.A))
        {
            await Connection.Instance.ConnectToServer();
        }
        if (Input.GetKeyUp(KeyCode.S))
        {
            SendText("سلام!!!");
        }
        if (Input.GetKeyUp(KeyCode.D))
        {
            await Connection.Instance.SendText("test", "NetworkFunctionCall");
        }
    }

    public void OnReceiveText(string text)
    {
        
    }

    public void OnMatchMakingSuccess()
    {
        //load game scene
    }

    public void SendText(string text)
    {
        _gameLobby.SendText(text);
    }
}
