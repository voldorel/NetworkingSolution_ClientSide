using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DreamNet;
using Newtonsoft.Json;

public class LobbyManager : MonoBehaviour
{

    public int avatar;
    
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
        _gameLobby.OnMatchMakingSuccess += (e) => OnMatchMakingSuccess(e);
        _gameLobby.OnConnectionSuccess += () => OnConnectionSuccess();
        _gameLobby.OnConnectionFailure += () => OnConnectionFailure();
        _gameLobby.OnEnterLobby += () => OnEnterLobby();
        _gameLobby.OnLoginSuccess += (e) => OnLoginSuccess(e);


        _nameSelectionPage.SetActive(false);
        _serverLoginObject.SetActive(true);

        _verifyNameButton.interactable = true;
        _joinGameButton.interactable = true;
    }

    

    

    public void Update()
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
            InstantiateLobbyMessage(text);
        } catch (System.Exception ex)
        {
            Debug.LogException(ex);
        }
        
    }


    public void OnLoginSuccess(string text)
    {
        try
        {
            MessageView.ShowLoadingView(false);
            JObject keyValuePairs = JObject.Parse(text);
            Connection.Instance.SetUserId((string)keyValuePairs["UserId"]);
            DreamNetwork.DreamNetworkInstance.InitUserMetaData((string)keyValuePairs["UserMetaData"]);
            DreamNetwork.DreamNetworkInstance.InitProfileMetaData(keyValuePairs["NickName"].ToString(),keyValuePairs["UserProfileInfo"].ToString());
            DreamNet.Resources.Init(keyValuePairs["Resources"].ToString());
            if ((bool)keyValuePairs["IsInGameSession"])
            {
                /////Debug.Log((string)keyValuePairs["matchData"]); // TODO needs a function to parse member list
                JObject jObject = JObject.Parse((string)keyValuePairs["matchData"]);
                Connection.Instance.SetRandomSeed(Int32.Parse((string)jObject["randomSeed"]));
                SceneManager.LoadScene("GameScene");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Login Initialization failed  " +ex);
        }

    }

    private void InstantiateLobbyMessage(string text)
    {
        JObject keyValuePairs = new JObject();
        keyValuePairs = JObject.Parse(text);

        string senderId = (string)keyValuePairs["UserId"];
        string lobbyMessage = (string)keyValuePairs["messageText"];

        if (senderId.Equals(_gameLobby.GetUserId()))
        {
            LobbyMessageManager.CreateSelfMessage(lobbyMessage);
        }
        else
        {
            LobbyMessageManager.CreateOtherMessage(lobbyMessage, senderId);
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


    public void OnMatchMakingSuccess(string matchData)
    {
        //Debug.Log("match data is " + matchData); // TODO needs a function to parse member list
        JObject keyValuePairs = JObject.Parse(matchData);
        JObject jObject = JObject.Parse((string)keyValuePairs["matchData"]);
        Connection.Instance.SetRandomSeed(Int32.Parse((string)jObject["randomSeed"]));
        SceneManager.LoadScene("GameScene");
    }

    public void SendLobbyText(string text)
    {
        _gameLobby.SendLobbyText(text);
    }



    public void OnClickConnectToServer()
    {
        MessageView.ShowLoadingView(true);
        Connection.Instance.StartDreamNet();
        //Connection.Instance.DoConnectToServer("localhost:5000");
        //Connection.Instance.ConnectToServer(_serverAddressInput.text, _serverAddressPortInput.text);
    }



    public void OnClickSetUsername()
    {
        if (_nameTextInput.text.Length >= 3)
        {
            //_gameLobby.SetUsername(_nameTextInput.text);
            _gameLobby.DoLogin(_nameTextInput.text);

            UnlockJoinButton();
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
        _nameSelectionPage.SetActive(false);
        LobbyMessageManager.CreateEntranceMessage(_gameLobby.GetUserId()); 
    }
    


    private void UnlockJoinButton()
    {
        _verifyNameButton.interactable = false;
        _joinGameButton.interactable = false;
        _verifyNameButton.interactable = false;
        _joinGameButton.interactable = true;
    }

    [ContextMenu("Change Resource")]
    public void ChangeResource()
    {
        try
        {
           // DreamNet.Resources.EarnResources("Coin",30,"Test");
           print(DreamNetwork.UserMetaData["Taha"]["IsKewni"].AsString);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
        
    }
    [ContextMenu("Change Resource 22222222222")]
    public void ChangeResource2()
    {
        // DreamNetwork.DreamNetworkInstance.ProfileMetaData.ChangeAvatarId(avatar)
       print( " coin          "+DreamNet.Resources.GetResourceValue("Coin"));
    }
}
