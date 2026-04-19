using Newtonsoft.Json.Linq;

#if UNITY_ANDROID && !UNITY_EDITOR
    using NativeFilePickerNamespace;
#elif UNITY_STANDALONE_WIN
using SFB;
#endif

using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;

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
    [SerializeField] private UnityEngine.UI.Button sendButton;
    [SerializeField] private GameObject emojiPanel;
    [SerializeField] private GameObject statusBar;
    [SerializeField] private GameObject quoteBar;
    [SerializeField] private TMP_Text quoteLabel;
    [SerializeField] private GameObject fileBar;
    [SerializeField] private TMP_Text fileLabel;
    [SerializeField] private GameObject backPanel;
    [SerializeField] private GameObject messagePanel;

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
        if (sendButton != null)
            sendButton.onClick.AddListener(OnSendButtonClick);
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

        GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
        foreach (GameObject obj in allObjects)
        {
            if (obj.name == "SwitchButton" && obj.scene.isLoaded)
            {
                obj.SetActive(true);
            }
        }
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
            if (Settings.isPCProgram)
            {
                messageInput.ActivateInputField();
                messageInput.caretPosition = Settings.lastCaretPosition;
                messageInput.selectionFocusPosition = messageInput.caretPosition;
            }
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
                            this.quoteLabel,
                            this.backPanel,
                            this.messagePanel
                        );
                    }
                }
            }

        }
        catch (Exception)
        {
        } finally
        {
            if (Settings.isPCProgram)
            {
                messageInput.ActivateInputField();
                messageInput.caretPosition = Settings.lastCaretPosition;
                messageInput.selectionFocusPosition = messageInput.caretPosition;
            }
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
            if (Settings.isPCProgram)
            {
                messageInput.ActivateInputField();
                messageInput.caretPosition = Settings.lastCaretPosition;
                messageInput.selectionFocusPosition = messageInput.caretPosition;
            }
        }
    }

    async void OnSendButtonClick()
    {
        AudioManager.PlayOneShot(buttonClick, clickVolume);
        // Очищаем текст от символов перевода строки перед отправкой
        string cleanText = messageInput.text.Trim().Replace("\n", "").Replace("\r", "");

        // Если сообщение пустое после очистки - не отправляем
        if (string.IsNullOrWhiteSpace(cleanText) &&
            string.IsNullOrWhiteSpace(FileInfo.FileName) &&
            string.IsNullOrWhiteSpace(Settings.QuotedId))
        {
            messageInput.text = "";
            if (Settings.isPCProgram)
            {
                messageInput.ActivateInputField();
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
            messageInput.interactable = false;
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

            messageInput.text = ""; // Очищаем поле

            if (Settings.isPCProgram)
            {
                messageInput.ActivateInputField();
                messageInput.caretPosition = Settings.lastCaretPosition;
                messageInput.selectionFocusPosition = messageInput.caretPosition;
            }
        }
        catch (Exception)
        {
            // Можно добавить логирование ошибки
        }
        finally
        {
            messageInput.interactable = true;
            if (Settings.isPCProgram)
            {
                messageInput.ActivateInputField();
                messageInput.caretPosition = Settings.lastCaretPosition;
                messageInput.selectionFocusPosition = messageInput.caretPosition;
            }
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

        if (Settings.isPCProgram)
        {
            messageInput.ActivateInputField();
            messageInput.caretPosition = Settings.lastCaretPosition;
            messageInput.selectionFocusPosition = messageInput.caretPosition;
        }
    }

    void OnFileButtonClick()
    {

        AudioManager.PlayOneShot(buttonClick, clickVolume);


#if UNITY_ANDROID && !UNITY_EDITOR
    if (NativeFilePicker.IsFilePickerBusy()) return;
// Вызов диалога выбора ЛЮБОГО файла (параметр null = показываем все файлы)
NativeFilePicker.PickFile((path) =>
{
        if (path != null)
        {
            string selectedFilePath = path;
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

            if (Settings.isPCProgram)
            {
                messageInput.ActivateInputField();
                messageInput.caretPosition = Settings.lastCaretPosition;
                messageInput.selectionFocusPosition = messageInput.caretPosition;
            }

        }
        else
        {
            //MessageBox.Show("Ошибка", "Выбор файла отменен");
        }
}, null);
#elif UNITY_STANDALONE_WIN
        string filePath = null;
        string[] paths = StandaloneFileBrowser.OpenFilePanel("Выберите файл", "", "*", false);
        if (paths != null && paths.Length > 0)
        {
            filePath = paths[0];
        }

        if (filePath != null)
        {
            string selectedFilePath = filePath;
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

            if (Settings.isPCProgram)
            {
                messageInput.ActivateInputField();
                messageInput.caretPosition = Settings.lastCaretPosition;
                messageInput.selectionFocusPosition = messageInput.caretPosition;
            }

        }
        else
        {
            //MessageBox.Show("Ошибка", "Выбор файла отменен");
        }
#endif

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
