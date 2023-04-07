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
        public delegate void ReceiveMessageAction(string text);
        public delegate void EnterMemberAction();
        public delegate void ExitMemberAction();
        public delegate void MatchMakingSuccess();



        public event ReceiveMessageAction OnReceiveMessage;
        public event EnterMemberAction OnEnterLobby;
        public event ExitMemberAction OnExitLobby;
        public event ReceiveMessageAction OnLeaveLobby;//needs implementation suggested by vs2022
        public event MatchMakingSuccess OnMatchMakingSuccess;

        public void Start()
        {
            this.hideFlags = HideFlags.HideInInspector;
            Connection.Instance.OnReceiveLobbyMessage += (e) => OnReceiveMessageMethod(e);
            Connection.Instance.OnEnterLobby += OnEnterLobbyMethod;
            Connection.Instance.OnExitLobby += OnExitLobbyMethod;
            Connection.Instance.OnMatchMakingSuccess+= OnMatchMakingSuccessMethod;
        }

        public async void SendText(string text)
        {
            await Connection.Instance.SendText(text, "LobbyMessage");
        }

        private void OnEnterLobbyMethod()
        {
            try
            {
                if (OnEnterLobby != null)
                {
                    OnEnterLobby?.Invoke();
                }
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        private void OnExitLobbyMethod()
        {
            OnExitLobby?.Invoke();
        }

        private void OnReceiveMessageMethod(string text)
        {
            OnReceiveMessage?.Invoke(text);
        }

        private void OnMatchMakingSuccessMethod()
        {
            OnMatchMakingSuccess?.Invoke();
        }




    }
}
