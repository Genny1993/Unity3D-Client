using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using static LoginWindow;

public class AdminWindow : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private FastButton addUserButton;
    [SerializeField] private FastButton refreshUserButton;
    [SerializeField] private FastButton addChatButton;
    [SerializeField] private FastButton refreshChatButton;
    [SerializeField] private FastButton chatButton;
    [SerializeField] private FastButton userButton;

    [SerializeField] private ScrollRect userList;
    [SerializeField] private ScrollRect chatList;

    [SerializeField] private GameObject userGroup;
    [SerializeField] private GameObject chatGroup;

    [Header("Настройки префаба")]
    [SerializeField] private string userPrefabPath = "Prefabs/UserPanel";
    [SerializeField] private string chatPrefabPath = "Prefabs/ChatPanel";

    [Header("Аудио SFX")]
    [SerializeField] private AudioClip buttonClick;

    [Header("Громкость")]
    [Range(0f, 1f)]
    [SerializeField] private float clickVolume = 0.7f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    async void Start()
    {
        if (addUserButton != null)
            addUserButton.onClick.AddListener(AddUserButtonClick);
        if (refreshUserButton != null)
            refreshUserButton.onClick.AddListener(RefreshUserButtonClick);
        if (addChatButton != null)
            addChatButton.onClick.AddListener(AddChatButtonClick);
        if (refreshChatButton != null)
            refreshChatButton.onClick.AddListener(RefreshChatButtonClick);
        if (chatButton != null)
            chatButton.onClick.AddListener(chatButtonClick);
        if (userButton != null)
            userButton.onClick.AddListener(userButtonClick);

        await GetUserList();
        await GetChatList();
    }

    void chatButtonClick()
    {
        AudioManager.PlayOneShot(buttonClick, clickVolume);
        chatGroup.SetActive(true);
        userGroup.SetActive(false);
    }

    void userButtonClick()
    {
        AudioManager.PlayOneShot(buttonClick, clickVolume);
        chatGroup.SetActive(false);
        userGroup.SetActive(true);
    }

    void AddUserButtonClick()
    {
        AudioManager.PlayOneShot(buttonClick, clickVolume);
        NewUserWindowStart.Show(this.gameObject);
    }

    async void RefreshUserButtonClick()
    {
        refreshUserButton.interactable = false;
        AudioManager.PlayOneShot(buttonClick, clickVolume);
        await GetUserList();
        refreshUserButton.interactable = true;
    }

    void AddChatButtonClick()
    {
        AudioManager.PlayOneShot(buttonClick, clickVolume);
        NewChatWindowStart.Show(this.gameObject);
    }

    async void RefreshChatButtonClick()
    {
        refreshChatButton.interactable = false;
        AudioManager.PlayOneShot(buttonClick, clickVolume);
        await GetChatList();
        refreshChatButton.interactable = true;
    }

    public async Task GetUserList ()
    {
        var formData = new List<KeyValuePair<string, string>>
        {
            new KeyValuePair<string, string>("pack[service]", "account"),
            new KeyValuePair<string, string>("pack[method]", "getListAdmin"),
            new KeyValuePair<string, string>("pack[access_key]", Settings.AuthKey),
            new KeyValuePair<string, string>("pack[info]", "")
        };

        try
        {

            Newtonsoft.Json.Linq.JObject result = await Sender.SendAndGet(formData);

            JArray UserArray = result["info"] as JArray;

            if (UserArray != null)
            {

                Transform contentTransform = userList.content;
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
                        GameObject newUserItem = Instantiate(userPrefab, contentTransform);

                        UserPanel user = newUserItem.GetComponent<UserPanel>();
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

    public async Task GetChatList()
    {
        var formData = new List<KeyValuePair<string, string>>
        {
            new KeyValuePair<string, string>("pack[service]", "chat"),
            new KeyValuePair<string, string>("pack[method]", "getListAdmin"),
            new KeyValuePair<string, string>("pack[access_key]", Settings.AuthKey),
            new KeyValuePair<string, string>("pack[info]", "")
        };

        try
        {

            Newtonsoft.Json.Linq.JObject result = await Sender.SendAndGet(formData);

            JArray ChatArray = result["info"] as JArray;

            if (ChatArray != null)
            {

                Transform contentTransform = chatList.content;
                if (contentTransform == null)
                {
                    Debug.LogError("ScrollView не имеет контейнера Content!");
                    return;
                }

                foreach (Transform child in contentTransform)
                {
                    Destroy(child.gameObject);
                }

                GameObject chatPrefab = Resources.Load<GameObject>(chatPrefabPath);
                if (chatPrefab == null)
                {
                    Debug.LogError($"Не удалось загрузить префаб по пути: {chatPrefabPath}");
                    return;
                }

                foreach (JToken item in ChatArray)
                {
                    if (item is JObject obj)
                    {
                        // Создаём экземпляр префаба
                        GameObject newChatItem = Instantiate(chatPrefab, contentTransform);

                        ChatPanel chat = newChatItem.GetComponent<ChatPanel>();
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

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Close()
    {
        Destroy(gameObject);
    }
}
