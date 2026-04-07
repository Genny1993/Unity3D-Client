using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class ChatWindow : MonoBehaviour
{
    [Header("Ссылки на UI")]
    [SerializeField] private ScrollRect chatsList;
    [SerializeField] private ScrollRect messagesList;
    [SerializeField] private TMP_InputField messageInput;

    [Header("Настройки префаба")]
    [SerializeField] private string prefabPath = "Prefabs/ChatPrefab";

    [Header("Таймеры")]
    [SerializeField] public UpdateMessageTimer timer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GetChats();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private async void GetChats()
    {
        var formData = new List<KeyValuePair<string, string>>
        {
            new KeyValuePair<string, string>("pack[service]", "chat"),
            new KeyValuePair<string, string>("pack[method]", "getList"),
            new KeyValuePair<string, string>("pack[access_key]", Settings.AuthKey),
            new KeyValuePair<string, string>("pack[info]", "")
        };

        try
        {

            Newtonsoft.Json.Linq.JObject result = await Sender.SendAndGet(formData);

            JArray UserArray = result["info"] as JArray;

            if (UserArray != null)
            {
                
                Transform contentTransform = chatsList.content;
                if (contentTransform == null)
                {
                    Debug.LogError("ScrollView не имеет контейнера Content!");
                    return;
                }

                foreach (Transform child in contentTransform)
                {
                    Destroy(child.gameObject);
                }

                GameObject chatPrefab = Resources.Load<GameObject>(prefabPath);
                if (chatPrefab == null)
                {
                    Debug.LogError($"Не удалось загрузить префаб по пути: {prefabPath}");
                    return;
                }

                foreach (JToken item in UserArray)
                {
                    if (item is JObject obj)
                    {
                        // Создаём экземпляр префаба
                        GameObject newChatItem = Instantiate(chatPrefab, contentTransform);

                        ChatPrefab chat = newChatItem.GetComponent<ChatPrefab>();
                        chat.Initializate(
                            item["id"]?.ToString() ?? "",
                            item["name"]?.ToString() ?? "",
                            messagesList,
                            messageInput,
                            this.timer
                        );
                    }
                }
            }

        }
        catch (Exception)
        {
        } finally
        {
            messageInput.ActivateInputField();
            messageInput.caretPosition = Settings.lastCaretPosition;
            messageInput.selectionFocusPosition = messageInput.caretPosition;
        }
    }
}
