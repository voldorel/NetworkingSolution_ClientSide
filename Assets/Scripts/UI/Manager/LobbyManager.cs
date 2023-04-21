using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
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
        _gameLobby.OnReceiveLobbyMessage += (e) => OnReceiveLobbyText(e);
        _gameLobby.OnMatchMakingSuccess += () => OnMatchMakingSuccess();
        _gameLobby.OnConnectionSuccess += () => OnConnectionSuccess();
        _gameLobby.OnConnectionFailure += () => OnConnectionFailure();
        _gameLobby.OnEnterLobby += () => OnEnterLobby();


        _nameSelectionPage.SetActive(false);
        _serverLoginObject.SetActive(true);

        _verifyNameButton.interactable = true;
        _joinGameButton.interactable = false;
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




    public void OnReceiveLobbyText(string text)
    {
        try
        {
            UnityMainThreadDispatcher.Instance().Enqueue(() => {
                InstantiateLobbyMessage(text);
            });
        } catch (System.Exception ex)
        {
            Debug.LogException(ex);
        }
        
    }

    private void InstantiateLobbyMessage(string text)
    {
        JObject keyValuePairs = new JObject();
        keyValuePairs = JObject.Parse(text);
        string senderName = (string)keyValuePairs["username"];
        string lobbyMessage = (string)keyValuePairs["messageText"];

        if (senderName.Equals(_gameLobby.GetUsername()))
        {
            LobbyMessageManager.CreateSelfMessage(lobbyMessage);
        }
        else
        {
            LobbyMessageManager.CreateOtherMessage(lobbyMessage, senderName);
        }
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
        UnityMainThreadDispatcher.Instance().Enqueue(() => {
            SceneManager.LoadScene("GameScene");
        });
    }

    public void SendLobbyText(string text)
    {
        _gameLobby.SendLobbyText(text);
    }



    public void OnClickConnectToServer()
    {
        MessageView.ShowLoadingView(true);
        Connection.Instance.ConnectToServer(_serverAddressInput.text, _serverAddressPortInput.text);
    }



    public void OnClickSetUsername()
    {
        if (_nameTextInput.text.Length >= 3)
        {
            _gameLobby.SetUsername(_nameTextInput.text);
            
            StartCoroutine(UnlockJoinButton());
        }
        else
            MessageView.ShowMessage("نام وارد شده باید حداقل سه حرفی باشد!");
    }



    public void OnClickSendLobbyMessage()
    {
        if (_inputText.text.Length >= 1)
        {
            _gameLobby.SendLobbyText(_inputText.text);
            _inputText.text = "";
        }
        else
        {
            _gameLobby.SendSessionStartRequest();
        }
    }




    public void OnClickJoinLobby()
    {
        _gameLobby.JoinMatchMaking();
    }


    public void OnClickJoinMatchMaking()
    {
        _gameLobby.JoinMatchMaking();
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
