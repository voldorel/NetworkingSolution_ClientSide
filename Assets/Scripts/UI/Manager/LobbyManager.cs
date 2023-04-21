using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using MyNetwork;

public class LobbyManager : MonoBehaviour
{
    [SerializeField]
    private InputField _inputText;
    [SerializeField]
    private InputField _nameTextInput;
    [SerializeField]
    private InputField _serverAddressInput;
    [SerializeField]
    private InputField _serverAddressPortInput;



    [SerializeField]
    private Button _joinGameButton;

    [SerializeField]
    private Button _verifyNameButton;




    [SerializeField]
    private GameObject _serverLoginObject;
    [SerializeField]
    private GameObject _nameSelectionPage;

    private GameLobby _gameLobby { get; set; }




    public void Start()
    {
        _gameLobby = gameObject.AddComponent<GameLobby>();
        _gameLobby.OnReceiveMessage += (e) => OnReceiveText(e);
        _gameLobby.OnMatchMakingSuccess += () => OnMatchMakingSuccess();
        _gameLobby.OnConnectionSuccess += () => OnConnectionSuccess();
        _gameLobby.OnConnectionFailure += () => OnConnectionFailure();
        _gameLobby.OnEnterLobby += () => OnEnterLobby();


        _nameSelectionPage.SetActive(false);
        _serverLoginObject.SetActive(true);

        _verifyNameButton.interactable = true;
        _joinGameButton.interactable = false;
    }

    

    public void OnClickSendTextMessage()
    {
        SendText(_inputText.text);
    }

    public async void Update()
    {
        /*if (Input.GetKeyUp(KeyCode.A))
        {
            await Connection.Instance.ConnectToServer("localhost", "7004");
        }
        if (Input.GetKeyUp(KeyCode.S))
        {
            SendText("سلام!!!");
        }
        if (Input.GetKeyUp(KeyCode.D))
        {
            await Connection.Instance.SendText("test", "NetworkFunctionCall");
        }*/
        
    }




    public void OnReceiveText(string text)
    {
        
    }

    private void OnConnectionSuccess()
    {
        MessageView.ShowLoadingView(false);
        _nameSelectionPage.SetActive(true);
        _serverLoginObject.SetActive(false);
    }

    private void OnConnectionFailure()
    {
        MessageView.ShowMessage("خطا در برقراری اتصال");
    }


    public void OnMatchMakingSuccess()
    {
        //load game scene
    }

    public void SendText(string text)
    {
        _gameLobby.SendText(text);
    }



    public void OnClickConnectToServer()
    {
        MessageView.ShowLoadingView(true);
        Connection.Instance.ConnectToServer(_serverAddressInput.text, _serverAddressPortInput.text);
    }



    public void OnClickSetUsername()
    {
        if (_nameTextInput.text.Length > 3)
        {
            _gameLobby.SetUsername(_nameTextInput.text);
            
            StartCoroutine(UnlockJoinButton());
        }
        else
            MessageView.ShowMessage("نام وارد شده باید حداقل سه حرفی باشد!");
    }


    public void OnClickJoinLobby()
    {
        _gameLobby.JoinMatchMaking();
    }


    public void OnClickJoinMatchMaking()
    {
        _gameLobby.JoinMatchMaking();
    }

    public void OnClickSendLobbyMessage()
    {
        Connection.Instance.ConnectToServer("localhost", "7004");
    }

    public void OnEnterLobby()
    {
        UnityMainThreadDispatcher.Instance().Enqueue(() => {
            _nameSelectionPage.SetActive(false);
            LobbyMessageManager.CreateEntranceMessage(_gameLobby.GetUsername()); 
        });
    }
    


    private IEnumerator UnlockJoinButton()
    {
        _verifyNameButton.interactable = false;
        _joinGameButton.interactable = false;
        yield return new WaitForSeconds(1);
        _verifyNameButton.interactable = false;
        _joinGameButton.interactable = true;
    }
}
