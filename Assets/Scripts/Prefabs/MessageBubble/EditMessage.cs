using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;

public class EditMessage : MonoBehaviour, IEventSystemHandler
{
    public string id;

    [Header("Игровые обьекты")]
    [SerializeField] public TMP_InputField message;
    [SerializeField] TMP_InputField messageEditor;
    [SerializeField] public MessageBubble mb;
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    private char ValidateInput(string text, int charIndex, char addedChar)
    {
        // Если нажат Enter (символ '\n') и не зажат Shift
        if (addedChar == '\n')
        {
            messageEditor.onSubmit.Invoke(messageEditor.text);
            return '\0'; // отменяем вставку
        }
        return addedChar;
    }

    void Start()
    {
        if (messageEditor == null)
            messageEditor = GetComponent<TMP_InputField>();

        // Перехватываем ввод символов
        messageEditor.onValidateInput += ValidateInput;
        // Дополнительно подписываемся на onSubmit, чтобы обработать вызов
        messageEditor.onSubmit.AddListener(OnSubmitCallback);
        messageEditor.onEndEdit.AddListener(OnFocusLost);
    }

    private async void OnSubmitCallback(string text)
    {
        var formData = new List<KeyValuePair<string, string>>
        {
            new KeyValuePair<string, string>("pack[service]", "message"),
            new KeyValuePair<string, string>("pack[method]", "edit"),
            new KeyValuePair<string, string>("pack[access_key]", Settings.AuthKey),
            new KeyValuePair<string, string>("pack[info][id]", this.id),
            new KeyValuePair<string, string>("pack[info][message]", messageEditor.text.Trim())
        };

        try
        {
            messageEditor.interactable = false;
            Newtonsoft.Json.Linq.JObject result = await Sender.SendAndGet(formData);
            message.text = messageEditor.text;

        }
        catch (Exception){}
        finally
        {
            messageEditor.text = "";
            messageEditor.gameObject.SetActive(false);
            message.gameObject.SetActive(true);
            mb.edited = false;
            
            if (mb.messageInput != null)
            {
                mb.messageInput.ActivateInputField();
                mb.messageInput.caretPosition = Settings.lastCaretPosition;
                mb.messageInput.selectionFocusPosition = mb.messageInput.caretPosition;
            }

            messageEditor.interactable = true;
        }
    }

    private void OnFocusLost(string text)
    {
        messageEditor.text = "";
        messageEditor.gameObject.SetActive(false);
        message.gameObject.SetActive(true);

        mb.edited = false;
    }

}
