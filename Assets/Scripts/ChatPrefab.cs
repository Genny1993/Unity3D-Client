using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
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

    [Header("Настройки префаба")]
    [SerializeField] private string prefabPath = "Prefabs/MessageBubble";

    [Header("Таймеры")]
    [SerializeField] public UpdateMessageTimer timer;

    public void Initializate(string chat_id, string chat_name, ScrollRect messages_list, TMP_InputField message_input, UpdateMessageTimer timer)
    {
        chatId = chat_id;
        chatName.text = chat_name;
        messagesList = messages_list;
        messageInput = message_input;
        this.timer = timer;

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
        chatButton.interactable = false;
        MessageBox.Show("Информация", "Нажата кнопка истории");
        chatButton.interactable = true;
    }

    async void OnChatButtonClick()
    {
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
                            (item["is_my"]?.ToString() ?? "") == "1" ? true : false,
                            messagesList,
                            messageInput
                        );
                        Settings.LastMessageId = item["id"]?.ToString() ?? "";
                    }
                }

                messagesList.enabled = wasEnabled;
                this.timer.StartTimer();
                // Прокрутка до самого низа
                StartCoroutine(ScrollToBottomNextFrame());
            }

        }
        catch (Exception)
        {
        } finally
        {
            chatButton.interactable = true;
            messageInput.ActivateInputField();
            messageInput.caretPosition = Settings.lastCaretPosition;
            messageInput.selectionFocusPosition = messageInput.caretPosition;
        }
    }

    IEnumerator ScrollToBottomNextFrame()
    {
        yield return new WaitForSeconds(0.1f);
        messagesList.verticalNormalizedPosition = 0f;
    }
}
