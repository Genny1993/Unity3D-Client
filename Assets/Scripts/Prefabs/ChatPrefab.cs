using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ChatPrefab : MonoBehaviour
{
    public string chatId;
    public TMP_InputField messageInput;
    public ScrollRect messagesList;

    [Header("UI Elements")]
    [SerializeField] private TMP_Text chatName;
    [SerializeField] private Button historyButton;
    [SerializeField] private Button chatButton;

    [SerializeField] private GameObject statusBar;
    [SerializeField] private GameObject quoteBar;
    [SerializeField] private TMP_Text quoteLabel;
    private GameObject chats;
    private GameObject messages;

    [Header("Настройки префаба")]
    [SerializeField] private string prefabPath = "Prefabs/MessageBubble";

    [Header("Таймеры")]
    [SerializeField] public UpdateMessageTimer timer;

    [Header("Аудио SFX")]
    [SerializeField] private AudioClip buttonClick;

    [Header("Громкость")]
    [Range(0f, 1f)]
    [SerializeField] private float clickVolume = 0.7f;

    public void Initializate(string chat_id, string chat_name, ScrollRect messages_list, TMP_InputField message_input, UpdateMessageTimer timer, GameObject status_bar, GameObject quote_bar, TMP_Text quote_label, GameObject chats, GameObject messages)
    {
        chatId = chat_id;
        chatName.text = chat_name;
        messagesList = messages_list;
        messageInput = message_input;
        this.timer = timer;
        this.statusBar = status_bar;
        this.quoteBar = quote_bar;
        this.quoteLabel = quote_label;
        this.chats = chats;
        this.messages = messages;

        if (historyButton != null)
            historyButton.onClick.AddListener(OnHistoryButtonClick);

        if (chatButton != null)
            chatButton.onClick.AddListener(OnChatButtonClick);
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnHistoryButtonClick()
    {
        AudioManager.PlayOneShot(buttonClick, clickVolume);

        HistoryWindowStart.Show(chatId, messagesList, messageInput, statusBar, quoteBar, quoteLabel);
    }

    async void OnChatButtonClick()
    {
        AudioManager.PlayOneShot(buttonClick, clickVolume);
        var formData = new List<KeyValuePair<string, string>>
        {
            new KeyValuePair<string, string>("pack[service]", "message"),
            new KeyValuePair<string, string>("pack[method]", "getLasts"),
            new KeyValuePair<string, string>("pack[access_key]", Settings.AuthKey),
            new KeyValuePair<string, string>("pack[info][chat_id]", chatId)
        };

        try
        {
            chatButton.interactable = false;
            Newtonsoft.Json.Linq.JObject result = await Sender.SendAndGet(formData);
            Settings.CurretChat = chatId;

            JArray UserArray = result["info"] as JArray;

            if (UserArray != null)
            {

                Transform contentTransform = messagesList.content;
                if (contentTransform == null)
                {
                    Debug.LogError("ScrollView не имеет контейнера Content!");
                    return;
                }

                foreach (Transform child in contentTransform)
                {
                    Destroy(child.gameObject);
                }

                GameObject messagePrefab = Resources.Load<GameObject>(prefabPath);
                if (messagePrefab == null)
                {
                    Debug.LogError($"Не удалось загрузить префаб по пути: {prefabPath}");
                    return;
                }

                bool wasEnabled = messagesList.enabled;
                messagesList.enabled = false;

                foreach (JToken item in UserArray)
                {
                    if (item is JObject obj)
                    {
                        // Создаём экземпляр префаба
                        GameObject newMessageItem = Instantiate(messagePrefab, contentTransform);

                        MessageBubble message = newMessageItem.GetComponent<MessageBubble>();
                        message.Initialize(
                            item["id"]?.ToString() ?? "",
                            item["name"]?.ToString() ?? "",
                            item["message"]?.ToString() ?? "",
                            item["date"]?.ToString() ?? "",
                            item["qid"]?.ToString() ?? "",
                            item["qname"]?.ToString() ?? "",
                            item["qmessage"]?.ToString() ?? "",
                            (item["is_my"]?.ToString() ?? "") == "1" ? true : false,
                            item["aid"]?.ToString() ?? "",
                            item["aname"]?.ToString() ?? "",
                            item["asize"]?.ToString() ?? "",
                            messagesList,
                            messageInput,
                            statusBar,
                            quoteBar,
                            quoteLabel
                        );
                        Settings.LastMessageId = item["id"]?.ToString() ?? "";
                    }
                }

                messagesList.enabled = wasEnabled;
                this.timer.StartTimer();
                // Прокрутка до самого низа

                GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
                foreach (GameObject obj in allObjects)
                {
                    if (obj.name == "SwitchButton" && obj.scene.isLoaded)
                    {
                        ChatSwitcher cs = obj.GetComponent<ChatSwitcher>();
                        cs.Switch();
                    }
                }
                Invoke(nameof(ScrollToBottomNextFrame), 0.2f);
            }

        }
        catch (Exception)
        {
        } finally
        {
            chatButton.interactable = true;
            if (Settings.isPCProgram)
            {
                messageInput.ActivateInputField();
                messageInput.caretPosition = Settings.lastCaretPosition;
                messageInput.selectionFocusPosition = messageInput.caretPosition;
            }
        }
    }

    void ScrollToBottomNextFrame()
    {
        messagesList.verticalNormalizedPosition = 0f;
    }
}
