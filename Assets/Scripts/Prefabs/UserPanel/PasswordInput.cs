using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class PasswordInput : MonoBehaviour, IEventSystemHandler
{
    public string id;

    [Header("Игровые обьекты")]
    [SerializeField] TMP_InputField passwordEditor;
    [SerializeField] TMP_Text passwordText;
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
            passwordEditor.onSubmit.Invoke(passwordEditor.text);
            return '\0'; // отменяем вставку
        }
        return addedChar;
    }

    void Start()
    {
        if (passwordEditor == null)
            passwordEditor = GetComponent<TMP_InputField>();

        // Перехватываем ввод символов
        passwordEditor.onValidateInput += ValidateInput;
        // Дополнительно подписываемся на onSubmit, чтобы обработать вызов
        passwordEditor.onSubmit.AddListener(OnSubmitCallback);
        passwordEditor.onEndEdit.AddListener(OnFocusLost);
    }

    private async void OnSubmitCallback(string text)
    {
        AudioManager.PlayOneShot(buttonClick, clickVolume);
        var formData = new List<KeyValuePair<string, string>>
        {
            new KeyValuePair<string, string>("pack[service]", "account"),
            new KeyValuePair<string, string>("pack[method]", "changePasswordAdmin"),
            new KeyValuePair<string, string>("pack[access_key]", Settings.AuthKey),
            new KeyValuePair<string, string>("pack[info][id]", this.id),
            new KeyValuePair<string, string>("pack[info][password]", passwordEditor.text)
        };

        try
        {
            passwordEditor.interactable = false;
            Newtonsoft.Json.Linq.JObject result = await Sender.SendAndGet(formData);

        }
        catch (Exception) { }
        finally
        {
            passwordEditor.text = "";
            passwordEditor.gameObject.SetActive(false);
            passwordText.gameObject.SetActive(true);
            passwordEditor.interactable = true;

            UserPanel userpanel = user.GetComponent<UserPanel>();
            if (userpanel != null)
            {
                userpanel.editingPassword = false;
            }
        }
    }

    private void OnFocusLost(string text)
    {
        passwordEditor.text = "";
        passwordEditor.gameObject.SetActive(false);
        passwordText.gameObject.SetActive(true);
        UserPanel userpanel = user.GetComponent<UserPanel>();

        if(userpanel != null)
        {
            userpanel.editingPassword = false;
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
