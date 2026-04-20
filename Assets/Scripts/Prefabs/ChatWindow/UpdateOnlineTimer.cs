using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UpdateOnlineTimer : MonoBehaviour
{

    [Header("Настройки таймера")]
    [SerializeField] private float interval = 5f; // Частота в секундах


    [Header("События")]
    [SerializeField] private UnityEvent onTimerTick; // Что делать каждый тик

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
        Debug.Log("Отправка статуса онлайн");
        //Останавливаем таймер, чтобы избежать наслоения запросов
        this.StopTimer();

        var formData = new List<KeyValuePair<string, string>>
        {
            new KeyValuePair<string, string>("pack[service]", "account"),
            new KeyValuePair<string, string>("pack[method]", "setLastActivity"),
            new KeyValuePair<string, string>("pack[access_key]", Settings.AuthKey),
            new KeyValuePair<string, string>("pack[info][chat_id]", Settings.CurretChat),
            new KeyValuePair<string, string>("pack[info][id]", Settings.LastMessageId)
        };

        try
        {
            this.StopTimer();
            JObject result = await Sender.SendAndGet(formData);

        }
        catch (Exception) { }
        finally
        {
            // Запускаем таймер снова
            this.StartTimer();
        }
    }
}
