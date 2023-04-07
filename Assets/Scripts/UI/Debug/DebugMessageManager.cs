using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Lobby;
using Lobby.EntranceMessage;
using Lobby.Other;
using Lobby.SelfMessage;

public class DebugMessageManager : MonoBehaviour
{
    public Transform ChatParent;
    public LobbyMessage LobbyMessagePrefab;
    public LobbyMessage LobbyMessageSelfPrefab;
    public LobbyMessage LobbyMessageEntrancePrefab;
    public Sprite TestAvatarSprite;
    public ScrollRect ScrollRect;
    
    void Start()
    {
        LayoutRebuilder.MarkLayoutForRebuild((RectTransform)ChatParent);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.H))
        {
            LobbyMessageOther message = Instantiate(LobbyMessagePrefab, ChatParent) as LobbyMessageOther;
            message.SetText("سلام و عرض ادب خدمت دوستان ارجمند . گرانقدر و به درد نخور و زشت و دوست نداشتنی . این پیام برای تست این بازی آنلاین طراحی شده است!");
            //if (ScrollRect.normalizedPosition.y > 0.4f)
                StartCoroutine(ScrollToBottomDelayed(ScrollRect));
            //ScrollToBottom(ScrollRect);
        }

        if (Input.GetKeyDown(KeyCode.J))
        {
            LobbyMessageOther message = Instantiate(LobbyMessagePrefab, ChatParent) as LobbyMessageOther;
            message.SetText("سلام و عرض ادب خدمت دوستان ارجمند . گرانقدر و به درد نخور و زشت و دوست نداشتنی . این پیام برای تست این بازی آنلاین طراحی شده است!");
            message.InitSenderInfo("فریدریش نیچه", TestAvatarSprite);
            //if (ScrollRect.normalizedPosition.y > 0.4f)
                StartCoroutine(ScrollToBottomDelayed(ScrollRect));
            //ScrollToBottom(ScrollRect);
        }

        if (Input.GetKeyDown(KeyCode.K))
        {
            LobbyMessageSelf message = Instantiate(LobbyMessageSelfPrefab, ChatParent) as LobbyMessageSelf;
            message.SetText("سلام و عرض ادب خدمت دوستان ارجمند . گرانقدر و به درد نخور و زشت و دوست نداشتنی . این پیام برای تست این بازی آنلاین طراحی شده است!");
            //if (ScrollRect.normalizedPosition.y > 0.4f)
                StartCoroutine(ScrollToBottomDelayed(ScrollRect));
            //ScrollToBottom(ScrollRect);
        }

        if (Input.GetKeyDown(KeyCode.L))
        {
            LobbyEntranceMessage message = Instantiate(LobbyMessageEntrancePrefab, ChatParent) as LobbyEntranceMessage;
            message.SetEnterChatText("مهدی");
            //if (ScrollRect.normalizedPosition.y > 0.4f)
                StartCoroutine(ScrollToBottomDelayed(ScrollRect));
            //ScrollToBottom(ScrollRect);
        }

    }
    public IEnumerator ScrollToBottomDelayed(ScrollRect scrollRect)
    {
        yield return new WaitForFixedUpdate();
        yield return new WaitForEndOfFrame();
        ScrollToBottom(scrollRect);
    }

    public void ScrollToBottom(ScrollRect scrollRect)
    {
        scrollRect.normalizedPosition = new Vector2(0, 0);
    }
}
