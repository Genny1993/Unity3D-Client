using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class ChatWindow : MonoBehaviour
{
    [Header("Ссылки на UI")]
    [SerializeField] private ScrollRect chatsList;

    [Header("Настройки префаба")]
    [SerializeField] private string prefabPath = "Prefabs/ChatPrefab";

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

                // 2. Очищаем старые элементы (чтобы не дублировать при повторном вызове)
                foreach (Transform child in contentTransform)
                {
                    Destroy(child.gameObject);
                }

                // 3. Загружаем префаб из Resources
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
                            item["name"]?.ToString() ?? ""
                        );
                    }
                }

            }

        }
        catch (Exception)
        {
        }
    }
}
