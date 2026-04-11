using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UpdateUserTimer : MonoBehaviour
{
    [Header("Игровые обьекты")]
    [SerializeField] private ScrollRect usersList;

    [Header("Настройки таймера")]
    [SerializeField] private float interval = 30f; // Частота в секундах


    [Header("События")]
    [SerializeField] private UnityEvent onTimerTick; // Что делать каждый тик

    [Header("Настройки префаба")]
    [SerializeField] private string prefabPath = "Prefabs/UserPrefab";

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
        Debug.Log("Проверка пользователей онлайн");
        //Останавливаем таймер, чтобы избежать наслоения запросов
        this.StopTimer();

        var formData = new List<KeyValuePair<string, string>>
        {
            new KeyValuePair<string, string>("pack[service]", "account"),
            new KeyValuePair<string, string>("pack[method]", "getList"),
            new KeyValuePair<string, string>("pack[access_key]", Settings.AuthKey),
            new KeyValuePair<string, string>("pack[info]","")
        };

        try
        {
            JObject result = await Sender.SendRequest(formData);

            string status = result["status"]?.ToString() ?? "";
            if (status == "OK")
            {
                JArray UserArray = result["info"] as JArray;

                if (UserArray != null)
                {

                    Transform contentTransform = usersList.content;
                    if (contentTransform == null)
                    {
                        Debug.LogError("ScrollView не имеет контейнера Content!");
                        return;
                    }

                    bool wasEnabled = usersList.enabled;
                    usersList.enabled = false;

                    foreach (Transform child in contentTransform)
                    {
                        Destroy(child.gameObject);
                    }

                    GameObject userPrefab = Resources.Load<GameObject>(prefabPath);
                    if (userPrefab == null)
                    {
                        Debug.LogError($"Не удалось загрузить префаб по пути: {prefabPath}");
                        return;
                    }


                    foreach (JToken item in UserArray)
                    {
                        if (item is JObject obj)
                        {
                            // Создаём экземпляр префаба
                            GameObject newChatItem = Instantiate(userPrefab, contentTransform);

                            UserPrefab user = newChatItem.GetComponent<UserPrefab>();
                            user.Initializate(
                                item["id"]?.ToString() ?? "",
                                item["name"]?.ToString() ?? "",
                                item["online"]?.ToString() ?? "",
                                item["last_activity"]?.ToString() ?? "",
                                item["is_i"]?.ToString() ?? "",
                                item["is_admin"]?.ToString() ?? ""
                            );
                        }
                    }
                    usersList.enabled = wasEnabled;
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
}
