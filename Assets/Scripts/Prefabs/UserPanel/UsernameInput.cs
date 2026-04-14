using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

public class UsernameInput : MonoBehaviour, IEventSystemHandler
{
    public string id;

    [Header("Игровые обьекты")]
    [SerializeField] TMP_InputField usernameEditor;
    [SerializeField] TMP_Text usernameText;
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
            usernameEditor.onSubmit.Invoke(usernameEditor.text);
            return '\0'; // отменяем вставку
        }
        return addedChar;
    }

    void Start()
    {
        if (usernameEditor == null)
            usernameEditor = GetComponent<TMP_InputField>();

        // Перехватываем ввод символов
        usernameEditor.onValidateInput += ValidateInput;
        // Дополнительно подписываемся на onSubmit, чтобы обработать вызов
        usernameEditor.onSubmit.AddListener(OnSubmitCallback);
        usernameEditor.onEndEdit.AddListener(OnFocusLost);
    }

    private async void OnSubmitCallback(string text)
    {
        AudioManager.PlayOneShot(buttonClick, clickVolume);
        var formData = new List<KeyValuePair<string, string>>
        {
            new KeyValuePair<string, string>("pack[service]", "account"),
            new KeyValuePair<string, string>("pack[method]", "changeNameAdmin"),
            new KeyValuePair<string, string>("pack[access_key]", Settings.AuthKey),
            new KeyValuePair<string, string>("pack[info][id]", this.id),
            new KeyValuePair<string, string>("pack[info][name]", usernameEditor.text.Trim())
        };

        try
        {
            usernameEditor.interactable = false;
            Newtonsoft.Json.Linq.JObject result = await Sender.SendAndGet(formData);
            usernameText.text = usernameEditor.text;

        }
        catch (Exception) { }
        finally
        {
            usernameEditor.text = "";
            usernameEditor.gameObject.SetActive(false);
            usernameText.gameObject.SetActive(true);
            usernameEditor.interactable = true;

            UserPanel userpanel = user.GetComponent<UserPanel>();
            if (userpanel != null)
            {
                userpanel.editingUsername = false;
            }
        }
    }

    private void OnFocusLost(string text)
    {
        usernameEditor.text = "";
        usernameEditor.gameObject.SetActive(false);
        usernameText.gameObject.SetActive(true);
        UserPanel userpanel = user.GetComponent<UserPanel>();

        if(userpanel != null)
        {
            userpanel.editingUsername = false;
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
