using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Lobby;
using Lobby.EntranceMessage;
using Lobby.Other;
using Lobby.SelfMessage;

public class LobbyMessageManager : MonoBehaviour
{
    public Transform ChatParent;
    public LobbyMessage LobbyMessagePrefab;
    public LobbyMessage LobbyMessageSelfPrefab;
    public LobbyMessage LobbyMessageEntrancePrefab;
    public Sprite TestAvatarSprite;
    public ScrollRect ScrollRect;
    public static LobbyMessageManager Instance;

    void Start()
    {
        LayoutRebuilder.MarkLayoutForRebuild((RectTransform)ChatParent);
    }


    public void Awake()
    {
        if (Instance != null)
            DestroyImmediate(Instance);
        Instance = this;
    }

    void Update()
    {
        /*if (Input.GetKeyDown(KeyCode.H))
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
        }*/

    }
    private static IEnumerator ScrollToBottomDelayed(ScrollRect scrollRect)
    {
        yield return new WaitForFixedUpdate();
        yield return new WaitForEndOfFrame();
        ScrollToBottom(scrollRect);
    }

    private static void ScrollToBottom(ScrollRect scrollRect)
    {
        scrollRect.normalizedPosition = new Vector2(0, 0);
    }


    public static void CreateEntranceMessage(string senderName)
    {
        LobbyEntranceMessage lobbyMessage = Instantiate(Instance.LobbyMessageEntrancePrefab, Instance.ChatParent) as LobbyEntranceMessage;
        lobbyMessage.SetEnterChatText(senderName);
        //if (ScrollRect.normalizedPosition.y > 0.4f)
        Instance.StartCoroutine(ScrollToBottomDelayed(Instance.ScrollRect));
        //ScrollToBottom(ScrollRect);
    }

    public static void CreateOtherMessage(string message, string senderName)
    {
        LobbyMessageOther lobbyMessage = Instantiate(Instance.LobbyMessagePrefab, Instance.ChatParent) as LobbyMessageOther;
        lobbyMessage.SetText(message);
        lobbyMessage.InitSenderInfo(senderName, Instance.TestAvatarSprite);
        //if (ScrollRect.normalizedPosition.y > 0.4f)
        Instance.StartCoroutine(ScrollToBottomDelayed(Instance.ScrollRect));
        //ScrollToBottom(ScrollRect);
    }

    public static void CreateSelfMessage(string message)
    {
        LobbyMessageSelf lobbyMessage = Instantiate(Instance.LobbyMessageSelfPrefab, Instance.ChatParent) as LobbyMessageSelf;
        lobbyMessage.SetText(message);
        //if (ScrollRect.normalizedPosition.y > 0.4f)
        Instance.StartCoroutine(ScrollToBottomDelayed(Instance.ScrollRect));
        //ScrollToBottom(ScrollRect);
    }

}
