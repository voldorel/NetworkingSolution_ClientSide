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



        public event ReceiveLobbyMessageAction OnReceiveLobbyMessage;
        public event EnterMemberAction OnEnterLobby;
        public event ExitMemberAction OnExitLobby;
        public event ReceiveMessageAction OnLeaveLobby;//needs implementation suggested by vs2022
        public event MatchMakingSuccess OnMatchMakingSuccess;
        public event ConnectionSuccess OnConnectionSuccess;
        public event ConnectionFailure OnConnectionFailure;
        public event LoginSuccess OnLoginSuccess;

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
