using UnityEngine;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class QuitGame : MonoBehaviour
{
    private bool _isQuitting = false;
    private bool _canQuit = false;

    private void Start()
    {
        Application.wantsToQuit += OnWantsToQuit;
    }

    private bool OnWantsToQuit()
    {
        if (_isQuitting) return _canQuit;

        _isQuitting = true;

        // Запускаем процесс выхода
        StartCoroutine(QuitProcess());

        // Блокируем автоматический выход
        return false;
    }

    private System.Collections.IEnumerator QuitProcess()
    {
        Debug.Log("Начинаем процесс выхода...");

        // Выполняем выход из аккаунта
        var task = ExitGame();

        // Ждем завершения (не блокируя главный поток)
        yield return new WaitUntil(() => task.IsCompleted);

        // Проверяем результат
        if (task.IsFaulted)
        {
            Debug.LogError($"Ошибка при выходе: {task.Exception}");
        }
        else
        {
            Debug.Log("Разлогин успешно выполнен");
        }

        Debug.Log("Игра закрывается!");

        // Разрешаем выход
        _canQuit = true;

        // Закрываем приложение
        Application.Quit();
    }

    public async Task ExitGame()
    {
        if (!string.IsNullOrEmpty(Settings.AuthKey))
        {
            var formData = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("pack[service]", "account"),
                new KeyValuePair<string, string>("pack[method]", "logout"),
                new KeyValuePair<string, string>("pack[access_key]", Settings.AuthKey),
                new KeyValuePair<string, string>("pack[info]", "")
            };

            try
            {
                Debug.Log("Отправляем запрос на выход...");
                Newtonsoft.Json.Linq.JObject result = await Sender.SendAndGet(formData);
                Debug.Log("Ответ от сервера получен");
            }
            catch (Exception e)
            {
                Debug.LogError($"Ошибка при выходе: {e.Message}");
                throw; // Пробрасываем исключение дальше
            }
        }
        else
        {
            Debug.Log("AuthKey пуст, выход не требуется");
        }
    }
}