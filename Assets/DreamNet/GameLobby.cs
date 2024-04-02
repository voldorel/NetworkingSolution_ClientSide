using System;
using UnityEngine;
using Newtonsoft.Json.Linq;

namespace DreamNet
{
    public class GameLobby : MonoBehaviour
    {
        public delegate void ReceiveLobbyMessageAction(string text);
        public delegate void ReceiveMessageAction(string text);
        public delegate void EnterMemberAction();
        public delegate void ExitMemberAction();
        public delegate void MatchMakingSuccess(string matchData);
        public delegate void ConnectionSuccess();
        public delegate void ConnectionFailure();
        public delegate void LoginSuccess(string text);
        public delegate void UpdateResources(string text);

        public delegate void UpdateUserMetaData(string text);
        public delegate void ChangeUserNickName(string text);
        public delegate void ChangeProfileMetaData(string text);
        
        public delegate void ChangeResource(string text);

        
        public event ReceiveLobbyMessageAction OnReceiveLobbyMessage;
        public event EnterMemberAction OnEnterLobby;
        public event ExitMemberAction OnExitLobby;
        public event ReceiveMessageAction OnLeaveLobby;//needs implementation suggested by vs2022
        public event MatchMakingSuccess OnMatchMakingSuccess;
        public event ConnectionSuccess OnConnectionSuccess;
        public event ConnectionFailure OnConnectionFailure;
        public event LoginSuccess OnLoginSuccess;
        public event UpdateResources OnUpdateResources;
        public event UpdateUserMetaData OnUpdateUserMetaData;
        public event ChangeUserNickName OnChangeUserNickName;
        public event ChangeProfileMetaData OnChangeProfileMetaData;
        
        public event ChangeResource OnChangeResource;

        public void Start()
        {
            this.hideFlags = HideFlags.HideInInspector;
            Connection.Instance.OnReceiveLobbyMessage += (e) => ReceiveLobbyMessageMethod(e);
            Connection.Instance.OnEnterLobby += EnterLobbyMethod;
            Connection.Instance.OnExitLobby += ExitLobbyMethod;
            Connection.Instance.OnMatchMakingSuccess+= MatchMakingSuccessMethod;
            Connection.Instance.OnConnectionSuccess += ConnectionSuccessMethod;
            Connection.Instance.OnConnectionFailure += ConnectionFailureMethod;
            Connection.Instance.OnLoginSuccessAction += (e) => LoginSuccessMethod(e);
            Connection.Instance.OnUpdateResourcesAction += (e) => UpdateResourcesMethod(e);
            Connection.Instance.OnUpdateUserMetaData += (e) => UpdateUserMetaDataMethod(e);
            Connection.Instance.OnChangeUserNickName+= (e) => ChangeUserNickNameMethod(e);
            Connection.Instance.OnChangeProfileMetaData+= (e) => ChangeProfileMetaDataMethod(e);
            Connection.Instance.OnChangeResource+= (e) => ChangeResourceMethod(e);
        }

        private void OnDestroy()
        {
            Connection.Instance.OnUpdateResourcesAction -= (e) => UpdateResourcesMethod(e);
            Connection.Instance.OnUpdateUserMetaData -= (e) => UpdateUserMetaDataMethod(e);
            Connection.Instance.OnChangeUserNickName-= (e) => ChangeUserNickNameMethod(e);
            Connection.Instance.OnChangeProfileMetaData-= (e) => ChangeProfileMetaDataMethod(e);
            Connection.Instance.OnChangeResource-= (e) => OnChangeResource(e);

        }


        public async void SendLobbyText(string text)
        {
            JObject keyValuePairs = new JObject();

            keyValuePairs.Add("UserId", GetUserId());
            keyValuePairs.Add("messageText", text);
            await Connection.Instance.SendText(keyValuePairs.ToString(), "LobbyMessage");
        }

        public async void SendSessionStartRequest()
        {
            await Connection.Instance.SendText("", "SessionStartRequest");
        }

        public async void SetUsername(string newUsername)
        {
            try
            {
                await Connection.Instance.SendText(newUsername, "UsernameRegister");
                Connection.Instance.SetUserId(newUsername);
            }
            catch
            {
                Debug.LogError("Setting username failed");
            }
        }

        public async void DoLogin(string newUsername)
        {
            try
            {
                await Connection.Instance.SendText(newUsername, "UserLogin");
                //MessageView.ShowLoadingView(true);
                Connection.Instance.SetUserId(newUsername);
            }
            catch
            {
                Debug.LogError("Login Failed");
            }
        }

        public async void JoinMatchMaking()
        {
            try
            {
                await Connection.Instance.SendText("", "MatchMakingRequest");
            }
            catch
            {
                Debug.LogError("Join Matchmaking failed");
            }
        }


        private void EnterLobbyMethod()
        {
            OnEnterLobby?.Invoke();
        }

        private void ExitLobbyMethod()
        {
            OnExitLobby?.Invoke();
        }

        private void ReceiveLobbyMessageMethod(string text)
        {
            OnReceiveLobbyMessage?.Invoke(text);
        }

        private void LoginSuccessMethod(string text)
        {
            OnLoginSuccess?.Invoke(text);
        }

        private void UpdateResourcesMethod(string text)
        {
            OnUpdateResources?.Invoke(text);
           
        }

        private void UpdateUserMetaDataMethod(string text)
        {
            OnUpdateUserMetaData?.Invoke(text);
        }
        private void ChangeUserNickNameMethod(string text)
        {
            OnChangeUserNickName?.Invoke(text);
        }
        private void ChangeProfileMetaDataMethod(string text)
        {
            OnChangeProfileMetaData?.Invoke(text);
            DreamNetwork.ProfileMetaData.OnChangeUserInfo(text);
        }

        private void ChangeResourceMethod(string text)
        {
            OnChangeResource?.Invoke(text);
            DreamNet.Resources.OnChangeResource(text);
        }


        private void MatchMakingSuccessMethod(string matchData)
        {
            OnMatchMakingSuccess?.Invoke(matchData);
        }

        private void ConnectionFailureMethod()
        {
            OnConnectionFailure?.Invoke();
        }

        private void ConnectionSuccessMethod()
        {
            OnConnectionSuccess?.Invoke();
        }


        public string GetUserId()
        {
            return Connection.Instance.GetUserId();
        }

    }
}
