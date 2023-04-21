using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using System;
using Newtonsoft.Json.Linq;

namespace MyNetwork
{
    public class GameLobby : MonoBehaviour
    {
        public delegate void ReceiveLobbyMessageAction(string text);
        public delegate void ReceiveMessageAction(string text);
        public delegate void EnterMemberAction();
        public delegate void ExitMemberAction();
        public delegate void MatchMakingSuccess();
        public delegate void ConnectionSuccess();
        public delegate void ConnectionFailure();



        public event ReceiveLobbyMessageAction OnReceiveLobbyMessage;
        public event EnterMemberAction OnEnterLobby;
        public event ExitMemberAction OnExitLobby;
        public event ReceiveMessageAction OnLeaveLobby;//needs implementation suggested by vs2022
        public event MatchMakingSuccess OnMatchMakingSuccess;
        public event ConnectionSuccess OnConnectionSuccess;
        public event ConnectionFailure OnConnectionFailure;

        public void Start()
        {
            this.hideFlags = HideFlags.HideInInspector;
            Connection.Instance.OnReceiveLobbyMessage += (e) => ReceiveLobbyMessageMethod(e);
            Connection.Instance.OnEnterLobby += EnterLobbyMethod;
            Connection.Instance.OnExitLobby += ExitLobbyMethod;
            Connection.Instance.OnMatchMakingSuccess+= MatchMakingSuccessMethod;
            Connection.Instance.OnConnectionSuccess += ConnectionSuccessMethod;
            Connection.Instance.OnConnectionFailure += ConnectionFailureMethod;
        }

        

        public async void SendLobbyText(string text)
        {
            JObject keyValuePairs = new JObject();
            keyValuePairs.Add("username", GetUsername());
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
                Connection.Instance.SetUsername(newUsername);
            }
            catch
            {

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

        private void MatchMakingSuccessMethod()
        {
            OnMatchMakingSuccess?.Invoke();
        }

        private void ConnectionFailureMethod()
        {
            OnConnectionFailure?.Invoke();
        }

        private void ConnectionSuccessMethod()
        {
            OnConnectionSuccess?.Invoke();
        }


        public string GetUsername()
        {
            return Connection.Instance.GetUsername();
        }

    }
}
