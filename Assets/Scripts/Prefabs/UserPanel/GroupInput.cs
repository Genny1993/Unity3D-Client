using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class GroupInput : MonoBehaviour, IEventSystemHandler
{
    public string id;

    [Header("Игровые обьекты")]
    [SerializeField] TMP_InputField groupEditor;
    [SerializeField] TMP_Text groupText;
    [SerializeField] UserPanel user;

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
            groupEditor.onSubmit.Invoke(groupEditor.text);
            return '\0'; // отменяем вставку
        }
        return addedChar;
    }

    void Start()
    {
        if (groupEditor == null)
            groupEditor = GetComponent<TMP_InputField>();

        // Перехватываем ввод символов
        groupEditor.onValidateInput += ValidateInput;
        // Дополнительно подписываемся на onSubmit, чтобы обработать вызов
        groupEditor.onSubmit.AddListener(OnSubmitCallback);
        groupEditor.onEndEdit.AddListener(OnFocusLost);
    }

    private async void OnSubmitCallback(string text)
    {
        AudioManager.PlayOneShot(buttonClick, clickVolume);
        var formData = new List<KeyValuePair<string, string>>
        {
            new KeyValuePair<string, string>("pack[service]", "account"),
            new KeyValuePair<string, string>("pack[method]", "changeGroupAdmin"),
            new KeyValuePair<string, string>("pack[access_key]", Settings.AuthKey),
            new KeyValuePair<string, string>("pack[info][id]", this.id),
            new KeyValuePair<string, string>("pack[info][roles]", groupEditor.text.Trim())
        };

        try
        {
            groupEditor.interactable = false;
            Newtonsoft.Json.Linq.JObject result = await Sender.SendAndGet(formData);
            groupText.text = groupEditor.text;

        }
        catch (Exception) { }
        finally
        {
            groupEditor.text = "";
            groupEditor.gameObject.SetActive(false);
            groupText.gameObject.SetActive(true);
            groupEditor.interactable = true;

            UserPanel userpanel = user.GetComponent<UserPanel>();
            if (userpanel != null)
            {
                userpanel.editingGroup = false;
            }
        }
    }

    private void OnFocusLost(string text)
    {
        groupEditor.text = "";
        groupEditor.gameObject.SetActive(false);
        groupText.gameObject.SetActive(true);
        UserPanel userpanel = user.GetComponent<UserPanel>();

        if(userpanel != null)
        {
            userpanel.editingGroup = false;
        }
    }

    async void RefreshUser()
    {
        var formData = new List<KeyValuePair<string, string>>
        {
            new KeyValuePair<string, string>("pack[service]", "account"),
            new KeyValuePair<string, string>("pack[method]", "getOneAdmin"),
            new KeyValuePair<string, string>("pack[access_key]", Settings.AuthKey),
            new KeyValuePair<string, string>("pack[info][id]", this.id)
        };

        try
        {

            Newtonsoft.Json.Linq.JObject result = await Sender.SendAndGet(formData);

            JArray UserArray = result["info"] as JArray;

            if (UserArray != null)
            {
                foreach (JToken item in UserArray)
                {
                    if (item is JObject obj)
                    {
                        user.Initializate(
                            item["id"]?.ToString() ?? "",
                            item["regdate"]?.ToString() ?? "",
                            item["login"]?.ToString() ?? "",
                            item["name"]?.ToString() ?? "",
                            "",
                            item["roles"]?.ToString() ?? "",
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
