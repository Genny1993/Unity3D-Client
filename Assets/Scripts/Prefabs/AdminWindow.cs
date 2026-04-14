using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class AdminWindow : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Button addUserButton;
    [SerializeField] private Button refreshUserButton;
    [SerializeField] private Button addChatButton;
    [SerializeField] private Button refreshChatButton;

    [SerializeField] private ScrollRect userList;
    [SerializeField] private ScrollRect chatList;

    [Header("Настройки префаба")]
    [SerializeField] private string userPrefabPath = "Prefabs/UserPanel";

    [Header("Аудио SFX")]
    [SerializeField] private AudioClip buttonClick;

    [Header("Громкость")]
    [Range(0f, 1f)]
    [SerializeField] private float clickVolume = 0.7f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (addUserButton != null)
            addUserButton.onClick.AddListener(AddUserButtonClick);
        if (refreshUserButton != null)
            refreshUserButton.onClick.AddListener(RefreshUserButtonClick);
        if (addChatButton != null)
            addChatButton.onClick.AddListener(AddChatButtonClick);
        if (refreshChatButton != null)
            refreshChatButton.onClick.AddListener(RefreshChatButtonClick);

        GetUserList();
    }

    void AddUserButtonClick()
    {
        AudioManager.PlayOneShot(buttonClick, clickVolume);
    }

    void RefreshUserButtonClick()
    {
        AudioManager.PlayOneShot(buttonClick, clickVolume);
        GetUserList();
    }

    void AddChatButtonClick()
    {
        AudioManager.PlayOneShot(buttonClick, clickVolume);
    }

    void RefreshChatButtonClick()
    {
        AudioManager.PlayOneShot(buttonClick, clickVolume);
    }

    async void GetUserList ()
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

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Close()
    {
        Destroy(gameObject);
    }
}
