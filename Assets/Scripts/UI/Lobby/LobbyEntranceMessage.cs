using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Lobby;


namespace Lobby.EntranceMessage
{
    public class LobbyEntranceMessage : LobbyMessage
    {
        public void SetEnterChatText(string memberName)
        {
            _contentText.text = memberName + " به لابی وارد شد!";
        }

        public void SetExtChatText(string memberName)
        {
            _contentText.text = memberName + " از لابی خارج شد!";
        }
    }
}
