using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Lobby
{
    public class LobbyMessage : MonoBehaviour
    {
        [SerializeField]
        protected Text _contentText;


        public void SetText(string text)
        {
            _contentText.text = text;
        }
    }

}

