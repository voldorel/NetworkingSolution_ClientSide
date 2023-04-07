using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Lobby;

namespace Lobby.Other
{
    public class LobbyMessageOther : LobbyMessage
    {
        [SerializeField]
        private Text _nameText;

        [SerializeField]
        private Image _avatarImage;

        public void InitSenderInfo(string name, Sprite avatar)
        {
            _nameText.text = name;
            _avatarImage.sprite = avatar;
        }
    }
}
