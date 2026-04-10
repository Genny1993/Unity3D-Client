using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class SendMessage : MonoBehaviour, IEventSystemHandler
{
    [SerializeField] private TMP_InputField inputField;

    [SerializeField] private GameObject quoteBar;
    [SerializeField] private GameObject fileBar;
    [SerializeField] private GameObject statusBar;
    [SerializeField] private ScrollRect messagesList;
    [SerializeField] private TMP_Text quoteLabel;

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

    private char ValidateInput(string text, int charIndex, char addedChar)
    {
        // Если нажат Enter (символ '\n') и не зажат Shift
        if (addedChar == '\n')
        {
            inputField.onSubmit.Invoke(inputField.text);
            return '\0'; // отменяем вставку
        }
        return addedChar;
    }

    private async void OnSubmitCallback(string text)
    {
        var formData = new List<KeyValuePair<string, string>>
        {
            new KeyValuePair<string, string>("pack[service]", "message"),
            new KeyValuePair<string, string>("pack[method]", "post"),
            new KeyValuePair<string, string>("pack[access_key]", Settings.AuthKey),
            new KeyValuePair<string, string>("pack[info][chat_id]", Settings.CurretChat),
            new KeyValuePair<string, string>("pack[info][message]", inputField.text.Trim()),
            new KeyValuePair<string, string>("pack[info][quoted_id]", Settings.QuotedId.Trim()),
            new KeyValuePair<string, string>("pack[info][file][name]", FileInfo.FileName.Trim()),
            new KeyValuePair<string, string>("pack[info][file][size]", FileInfo.FileSize.ToString()),
            new KeyValuePair<string, string>("pack[info][file][type]", FileInfo.FileType.Trim()),
            new KeyValuePair<string, string>("pack[info][file][content]", FileInfo.FileContentBase64.Trim()),
        };

        try
        {
            Newtonsoft.Json.Linq.JObject result = await Sender.SendAndGet(formData);
            Settings.QuotedId = "";
            FileInfo.Clear();
            quoteLabel.text = "";
            quoteBar.SetActive(false);
            fileBar.SetActive(false);
            statusBar.SetActive(false);


            RectTransform rect = messagesList.GetComponent<RectTransform>();
            Vector2 size = rect.sizeDelta;
            size.y = Settings.currentMessagesListHeight + 40;
            Settings.currentMessagesListHeight = (int)size.y;
            rect.sizeDelta = size;

            inputField.ActivateInputField();
            inputField.caretPosition = Settings.lastCaretPosition;
            inputField.selectionFocusPosition = inputField.caretPosition;

        }
        catch (Exception)
        {
        }
        finally
        {
            inputField.interactable = true;
            inputField.ActivateInputField();
            inputField.text = "";
        }
    }
}
