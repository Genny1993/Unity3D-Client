using Newtonsoft.Json.Linq;
using SFB;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class ChatWindow : MonoBehaviour
{
    bool emojis;
    [Header("Ссылки на UI")]
    [SerializeField] private ScrollRect chatsList;
    [SerializeField] private ScrollRect usersList;
    [SerializeField] private ScrollRect messagesList;
    [SerializeField] private TMP_InputField messageInput;
    [SerializeField] private UnityEngine.UI.Button emojiButton;
    [SerializeField] private UnityEngine.UI.Button fileButton;
    [SerializeField] private GameObject emojiPanel;
    [SerializeField] private GameObject statusBar;
    [SerializeField] private GameObject quoteBar;
    [SerializeField] private TMP_Text quoteLabel;
    [SerializeField] private GameObject fileBar;
    [SerializeField] private TMP_Text fileLabel;

    [Header("Настройки префаба")]
    [SerializeField] private string prefabPath = "Prefabs/ChatPrefab";
    [SerializeField] private string userPrefabPath = "Prefabs/UserPrefab";

    [Header("Таймеры")]
    [SerializeField] public UpdateMessageTimer timer;
    [SerializeField] public UpdateUserTimer userTimer;
    [SerializeField] public UpdateOnlineTimer onlineTimer;

    [Header("Аудио SFX")]
    [SerializeField] private AudioClip buttonClick;

    [Header("Громкость")]
    [Range(0f, 1f)]
    [SerializeField] private float clickVolume = 0.7f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        emojis = false;
        if (emojiButton != null)
            emojiButton.onClick.AddListener(OnEmojiButtonClick);
        if (fileButton != null)
            fileButton.onClick.AddListener(OnFileButtonClick);
        RectTransform rect = messagesList.GetComponent<RectTransform>();
        Vector2 size = rect.sizeDelta;
        size.y = Settings.currentMessagesListHeight;
        rect.sizeDelta = size;
        emojiPanel.SetActive(false);
        statusBar.SetActive(false);
        quoteBar.SetActive(false);
        fileBar.SetActive(false);
        GetChats();
        GetUsers();
        IsAdmin();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private async void IsAdmin()
    {
        var formData = new List<KeyValuePair<string, string>>
        {
            new KeyValuePair<string, string>("pack[service]", "account"),
            new KeyValuePair<string, string>("pack[method]", "isAdmin"),
            new KeyValuePair<string, string>("pack[access_key]", Settings.AuthKey),
            new KeyValuePair<string, string>("pack[info]", "")
        };

        try
        {
            Newtonsoft.Json.Linq.JObject result = await Sender.SendAndGet(formData);
            Settings.isAdmin = result["info"].ToString() == "1" ? true : false;

            var allObjects = Resources.FindObjectsOfTypeAll<GameObject>();

            foreach (var obj in allObjects)
            {
                // Проверяем, что объект не является prefab'ом и находится в сцене
                if (obj.name == "AdminPanelButton" && obj.scene.isLoaded)
                {
                    if (Settings.isAdmin)
                    {
                        obj.SetActive(true);
                    }
                    else
                    {
                        obj.SetActive(false);
                    }
                    break;
                }
            }
        }
        catch (Exception)
        {
        }
        finally
        {
            messageInput.ActivateInputField();
            messageInput.caretPosition = Settings.lastCaretPosition;
            messageInput.selectionFocusPosition = messageInput.caretPosition;
        }
    }
    private async void GetChats()
    {
        var formData = new List<KeyValuePair<string, string>>
        {
            new KeyValuePair<string, string>("pack[service]", "chat"),
            new KeyValuePair<string, string>("pack[method]", "getList"),
            new KeyValuePair<string, string>("pack[access_key]", Settings.AuthKey),
            new KeyValuePair<string, string>("pack[info]", "")
        };

        try
        {

            Newtonsoft.Json.Linq.JObject result = await Sender.SendAndGet(formData);

            JArray UserArray = result["info"] as JArray;

            if (UserArray != null)
            {
                
                Transform contentTransform = chatsList.content;
                if (contentTransform == null)
                {
                    Debug.LogError("ScrollView не имеет контейнера Content!");
                    return;
                }

                foreach (Transform child in contentTransform)
                {
                    Destroy(child.gameObject);
                }

                GameObject chatPrefab = Resources.Load<GameObject>(prefabPath);
                if (chatPrefab == null)
                {
                    Debug.LogError($"Не удалось загрузить префаб по пути: {prefabPath}");
                    return;
                }

                foreach (JToken item in UserArray)
                {
                    if (item is JObject obj)
                    {
                        // Создаём экземпляр префаба
                        GameObject newChatItem = Instantiate(chatPrefab, contentTransform);

                        ChatPrefab chat = newChatItem.GetComponent<ChatPrefab>();
                        chat.Initializate(
                            item["id"]?.ToString() ?? "",
                            item["name"]?.ToString() ?? "",
                            messagesList,
                            messageInput,
                            this.timer,
                            this.statusBar,
                            this.quoteBar,
                            this.quoteLabel
                        );
                    }
                }
            }

        }
        catch (Exception)
        {
        } finally
        {
            messageInput.ActivateInputField();
            messageInput.caretPosition = Settings.lastCaretPosition;
            messageInput.selectionFocusPosition = messageInput.caretPosition;
        }
    }

    async void GetUsers()
    {
        var formData = new List<KeyValuePair<string, string>>
        {
            new KeyValuePair<string, string>("pack[service]", "account"),
            new KeyValuePair<string, string>("pack[method]", "getList"),
            new KeyValuePair<string, string>("pack[access_key]", Settings.AuthKey),
            new KeyValuePair<string, string>("pack[info]","")
        };

        try
        {

            Newtonsoft.Json.Linq.JObject result = await Sender.SendAndGet(formData);

            JArray UserArray = result["info"] as JArray;

            if (UserArray != null)
            {

                Transform contentTransform = usersList.content;
                if (contentTransform == null)
                {
                    Debug.LogError("ScrollView не имеет контейнера Content!");
                    return;
                }

                foreach (Transform child in contentTransform)
                {
                    Destroy(child.gameObject);
                }

                GameObject userPrefab = Resources.Load<GameObject>(userPrefabPath);
                if (userPrefab == null)
                {
                    Debug.LogError($"Не удалось загрузить префаб по пути: {userPrefabPath}");
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
            }
            this.userTimer.StartTimer();
            this.onlineTimer.StartTimer();
        }
        catch (Exception)
        {
        }
        finally
        {
            messageInput.ActivateInputField();
            messageInput.caretPosition = Settings.lastCaretPosition;
            messageInput.selectionFocusPosition = messageInput.caretPosition;
        }
    }

    void OnEmojiButtonClick()
    {
        AudioManager.PlayOneShot(buttonClick, clickVolume);

        if (emojis)
        {
            emojis = false;
            emojiPanel.SetActive(false);
            RectTransform rect = messagesList.GetComponent<RectTransform>();
            Vector2 size = rect.sizeDelta;
            size.y = Settings.currentMessagesListHeight + 220;
            Settings.currentMessagesListHeight = (int)size.y;
            rect.sizeDelta = size;
        } else
        {
            emojis = true;
            emojiPanel.SetActive(true);
            RectTransform rect = messagesList.GetComponent<RectTransform>();
            Vector2 size = rect.sizeDelta;
            size.y = Settings.currentMessagesListHeight - 220;
            Settings.currentMessagesListHeight = (int)size.y;
            rect.sizeDelta = size;
        }

        messageInput.ActivateInputField();
        messageInput.caretPosition = Settings.lastCaretPosition;
        messageInput.selectionFocusPosition = messageInput.caretPosition;
    }

    void OnFileButtonClick()
    {
        AudioManager.PlayOneShot(buttonClick, clickVolume);

        // Последний параметр false — выбираем только один файл
        string[] paths = StandaloneFileBrowser.OpenFilePanel("Выберите файл", "", "*", false);

        if (paths != null && paths.Length > 0)
        {
            string selectedFilePath = paths[0];

            if (FileInfo.LoadFile(selectedFilePath))
            {

                Settings.fileBar = true;
                fileLabel.text = FileInfo.FileName;

                if (statusBar.activeInHierarchy == false)
                {
                    statusBar.SetActive(true);

                    RectTransform rect = messagesList.GetComponent<RectTransform>();
                    Vector2 size = rect.sizeDelta;
                    size.y = Settings.currentMessagesListHeight - 40;
                    Settings.currentMessagesListHeight = (int)size.y;
                    rect.sizeDelta = size;
                }

                fileBar.SetActive(true);
            }
            else
            {
                MessageBox.Show("Ошибка", "Ошибка при загрузке файла!");
            }

            messageInput.ActivateInputField();
            messageInput.caretPosition = Settings.lastCaretPosition;
            messageInput.selectionFocusPosition = messageInput.caretPosition;

        }
        else
        {
            //MessageBox.Show("Ошибка", "Выбор файла отменен");
        }
    }

    IEnumerator ScrollToBottomNextFrame()
    {
        yield return new WaitForSeconds(0.1f);
        messagesList.verticalNormalizedPosition = 0f;
    }

    public void Close()
    {
        Destroy(gameObject);
    }
}
