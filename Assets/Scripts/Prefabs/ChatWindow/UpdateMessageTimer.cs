using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UpdateMessageTimer : MonoBehaviour
{
    [Header("Игровые обьекты")]
    [SerializeField] private ScrollRect messagesList;
    [SerializeField] private TMP_InputField messageInput;

    [SerializeField] private GameObject statusBar;
    [SerializeField] private GameObject quoteBar;
    [SerializeField] private TMP_Text quoteLabel;

    [Header("Аудио SFX")]
    [SerializeField] private AudioClip newMessage;

    [Header("Настройки таймера")]
    [SerializeField] private float interval = 2f; // Частота в секундах


    [Header("События")]
    [SerializeField] private UnityEvent onTimerTick; // Что делать каждый тик

    [Header("Настройки префаба")]
    [SerializeField] private string prefabPath = "Prefabs/MessageBubble";

    public void StartTimer()
    {
        StartCoroutine(TimerCoroutine());
    }

    public void StopTimer()
    {
        StopAllCoroutines();
    }

    private IEnumerator TimerCoroutine()
    {
        while (true) // Бесконечный цикл
        {
            yield return new WaitForSeconds(interval); // Ждем N секунд
            TimerTick();
        }
    }

    private async void TimerTick()
    {
        //Останавливаем таймер, чтобы избежать наслоения запросов
        this.StopTimer();

        var formData = new List<KeyValuePair<string, string>>
        {
            new KeyValuePair<string, string>("pack[service]", "message"),
            new KeyValuePair<string, string>("pack[method]", "getNewest"),
            new KeyValuePair<string, string>("pack[access_key]", Settings.AuthKey),
            new KeyValuePair<string, string>("pack[info][chat_id]", Settings.CurretChat),
            new KeyValuePair<string, string>("pack[info][id]", Settings.LastMessageId)
        };

        try
        {
            JObject result = await Sender.SendRequest(formData);

            string status = result["status"]?.ToString() ?? "";
            if (status == "OK")
            {
                JArray UserArray = result["info"] as JArray;
                if (UserArray.Count > 0)
                {
                    Transform contentTransform = messagesList.content;
                    if (contentTransform == null)
                    {
                        Debug.LogError("ScrollView не имеет контейнера Content!");
                        return;
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
                                item["apreview"]?.ToString() ?? "",
                                messagesList,
                                messageInput,
                                statusBar,
                                quoteBar,
                                quoteLabel
                            );
                            Settings.LastMessageId = item["id"]?.ToString() ?? "";
                            StartCoroutine(message.IgniteWithDelay());
                        }
                    }

                    messagesList.enabled = wasEnabled;
                    AudioManager.PlayOneShot(newMessage);
                    // Прокрутка до самого низа
                    ScrollListOnNewMessage();
                }
            }
        }
        catch (Exception) { }
        finally
        {
            // Запускаем таймер снова
            this.StartTimer();
        }
    }

    void ScrollListOnNewMessage(string none = "")
    {

        float contentHeight = messagesList.content.rect.height;
        float viewportHeight = messagesList.viewport.rect.height;
        float maxScroll = contentHeight - viewportHeight;

        if (maxScroll <= 0)
        {
            return;
        }

        float normalizedPos = messagesList.verticalNormalizedPosition;
        float distanceToBottom = normalizedPos * maxScroll;


        if (distanceToBottom < 300)
        {
            StartCoroutine(ScrollToBottomNextFrame());
        }
    }

    IEnumerator ScrollToBottomNextFrame()
    {
        yield return new WaitForSeconds(0.1f);
        messagesList.verticalNormalizedPosition = 0f;
    }
}
