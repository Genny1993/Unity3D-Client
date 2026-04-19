using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SendMessage : MonoBehaviour, IEventSystemHandler
{
    [SerializeField] private TMP_InputField inputField;

    [SerializeField] private GameObject quoteBar;
    [SerializeField] private GameObject fileBar;
    [SerializeField] private GameObject statusBar;
    [SerializeField] private ScrollRect messagesList;
    [SerializeField] private TMP_Text quoteLabel;

    // Флаг для отслеживания реального нажатия Enter
    private bool isRealEnterPressed = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (inputField == null)
            inputField = GetComponent<TMP_InputField>();

        // Перехватываем ввод символов
        inputField.onValidateInput += ValidateInput;
        // Дополнительно подписываемся на onSubmit, чтобы обработать вызов
        inputField.onSubmit.AddListener(OnSubmitCallback);
    }

    void Update()
    {
        // Проверяем реальное нажатие Enter на клавиатуре
        if (inputField != null && inputField.isFocused && Input.GetKeyDown(KeyCode.Return))
        {
            isRealEnterPressed = true;
            // Вызываем отправку сообщения
            inputField.onSubmit.Invoke(inputField.text);
            // Сбрасываем флаг в следующем кадре
            Invoke(nameof(ResetEnterFlag), 0.1f);
        }
    }

    private void ResetEnterFlag()
    {
        isRealEnterPressed = false;
    }

    private char ValidateInput(string text, int charIndex, char addedChar)
    {
        // Если это символ Enter
        if (addedChar == '\n')
        {
            // Отправляем сообщение ТОЛЬКО если это реальное нажатие клавиши
            if (isRealEnterPressed)
            {
                return '\0';
            }
            // Всегда отменяем вставку символа \n
            return '\n';
        }
        return addedChar;
    }

    private async void OnSubmitCallback(string text)
    {
        // Очищаем текст от символов перевода строки перед отправкой
        string cleanText = inputField.text.Trim().Replace("\n", "").Replace("\r", "");

        // Если сообщение пустое после очистки - не отправляем
        if (string.IsNullOrWhiteSpace(cleanText) &&
            string.IsNullOrWhiteSpace(FileInfo.FileName) &&
            string.IsNullOrWhiteSpace(Settings.QuotedId))
        {
            inputField.text = "";
            if (Settings.isPCProgram)
            {
                inputField.ActivateInputField();
            }
            return;
        }

        var formData = new List<KeyValuePair<string, string>>
        {
            new KeyValuePair<string, string>("pack[service]", "message"),
            new KeyValuePair<string, string>("pack[method]", "post"),
            new KeyValuePair<string, string>("pack[access_key]", Settings.AuthKey),
            new KeyValuePair<string, string>("pack[info][chat_id]", Settings.CurretChat),
            new KeyValuePair<string, string>("pack[info][message]", cleanText), // Используем очищенный текст
            new KeyValuePair<string, string>("pack[info][quoted_id]", Settings.QuotedId.Trim()),
            new KeyValuePair<string, string>("pack[info][file][name]", FileInfo.FileName.Trim()),
            new KeyValuePair<string, string>("pack[info][file][size]", FileInfo.FileSize.ToString()),
            new KeyValuePair<string, string>("pack[info][file][type]", FileInfo.FileType.Trim()),
            new KeyValuePair<string, string>("pack[info][file][content]", FileInfo.FileContentBase64.Trim()),
        };

        try
        {
            inputField.interactable = false;
            Newtonsoft.Json.Linq.JObject result = await Sender.SendAndGet(formData);
            Settings.QuotedId = "";
            FileInfo.Clear();
            quoteLabel.text = "";
            quoteBar.SetActive(false);
            fileBar.SetActive(false);
            statusBar.SetActive(false);

            RectTransform rect = messagesList.GetComponent<RectTransform>();
            Vector2 size = rect.sizeDelta;
            if (Settings.fileBar == true || Settings.quoteBar == true)
            {
                Settings.fileBar = false;
                Settings.quoteBar = false;
                size.y = Settings.currentMessagesListHeight + 40;
                Settings.currentMessagesListHeight = (int)size.y;
                rect.sizeDelta = size;
            }

            inputField.text = ""; // Очищаем поле

            if (Settings.isPCProgram)
            {
                inputField.ActivateInputField();
                inputField.caretPosition = Settings.lastCaretPosition;
                inputField.selectionFocusPosition = inputField.caretPosition;
            }
        }
        catch (Exception)
        {
            // Можно добавить логирование ошибки
        }
        finally
        {
            inputField.interactable = true;
            if (Settings.isPCProgram)
            {
                inputField.ActivateInputField();
                inputField.caretPosition = Settings.lastCaretPosition;
                inputField.selectionFocusPosition = inputField.caretPosition;
            }
        }
    }
}