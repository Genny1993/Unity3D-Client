using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

public class NameInput : MonoBehaviour, IEventSystemHandler
{
    public string id;

    [Header("Игровые обьекты")]
    [SerializeField] TMP_InputField nameEditor;
    [SerializeField] TMP_Text nameText;
    [SerializeField] ChatPanel chat;

    [Header("Аудио SFX")]
    [SerializeField] private AudioClip buttonClick;

    [Header("Громкость")]
    [Range(0f, 1f)]
    [SerializeField] private float clickVolume = 0.7f;

    private char ValidateInput(string text, int charIndex, char addedChar)
    {
        // Если нажат Enter (символ '\n') и не зажат Shift
        if (addedChar == '\n')
        {
            nameEditor.onSubmit.Invoke(nameEditor.text);
            return '\0'; // отменяем вставку
        }
        return addedChar;
    }

    void Start()
    {
        if (nameEditor == null)
            nameEditor = GetComponent<TMP_InputField>();

        // Перехватываем ввод символов
        nameEditor.onValidateInput += ValidateInput;
        // Дополнительно подписываемся на onSubmit, чтобы обработать вызов
        nameEditor.onSubmit.AddListener(OnSubmitCallback);
        nameEditor.onEndEdit.AddListener(OnFocusLost);
    }

    private async void OnSubmitCallback(string text)
    {
        AudioManager.PlayOneShot(buttonClick, clickVolume);
        var formData = new List<KeyValuePair<string, string>>
        {
            new KeyValuePair<string, string>("pack[service]", "chat"),
            new KeyValuePair<string, string>("pack[method]", "rename"),
            new KeyValuePair<string, string>("pack[access_key]", Settings.AuthKey),
            new KeyValuePair<string, string>("pack[info][id]", this.id),
            new KeyValuePair<string, string>("pack[info][name]", nameEditor.text.Trim())
        };

        try
        {
            nameEditor.interactable = false;
            Newtonsoft.Json.Linq.JObject result = await Sender.SendAndGet(formData);
            nameText.text = nameEditor.text;
            RefreshChat();

        }
        catch (Exception) { }
        finally
        {
            nameEditor.text = "";
            nameEditor.gameObject.SetActive(false);
            nameText.gameObject.SetActive(true);
            nameEditor.interactable = true;

            ChatPanel chatpanel = chat.GetComponent<ChatPanel>();
            if (chatpanel != null)
            {
                chatpanel.editingName = false;
            }
        }
    }

    private void OnFocusLost(string text)
    {
        nameEditor.text = "";
        nameEditor.gameObject.SetActive(false);
        nameText.gameObject.SetActive(true);
        ChatPanel chatpanel = chat.GetComponent<ChatPanel>();

        if(chatpanel != null)
        {
            chatpanel.editingName = false;
        }
    }

    async void RefreshChat()
    {
        var formData = new List<KeyValuePair<string, string>>
        {
            new KeyValuePair<string, string>("pack[service]", "chat"),
            new KeyValuePair<string, string>("pack[method]", "getOneAdmin"),
            new KeyValuePair<string, string>("pack[access_key]", Settings.AuthKey),
            new KeyValuePair<string, string>("pack[info][id]", this.id)
        };

        try
        {

            Newtonsoft.Json.Linq.JObject result = await Sender.SendAndGet(formData);

            JArray ChatArray = result["info"] as JArray;

            if (ChatArray != null)
            {
                foreach (JToken item in ChatArray)
                {
                    if (item is JObject obj)
                    {
                        chat.Initializate(
                            item["id"]?.ToString() ?? "",
                            item["regdate"]?.ToString() ?? "",
                            item["name"]?.ToString() ?? "",
                            item["deleted"]?.ToString() ?? ""
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
