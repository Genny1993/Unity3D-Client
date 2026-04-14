using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;

public class UserPanel : MonoBehaviour
{
    private string id = "";
    private string regdate = "";
    private string login = "";
    private string username = "";
    private string password = "";
    private string group = "";
    private string deleted = "";

    public bool editingUsername = false;
    public bool editingPassword = false;
    public bool editingGroup = false;


    [Header("UI Elements")]
    [SerializeField] private TMP_Text userDLogin;
    [SerializeField] private TMP_Text usernameText;
    [SerializeField] private TMP_Text passwordText;
    [SerializeField] private TMP_Text groupText;
    [SerializeField] private TMP_InputField usernameInput;
    [SerializeField] private TMP_InputField passwordInput;
    [SerializeField] private TMP_InputField groupInput;
    [SerializeField] private Button usernameButton;
    [SerializeField] private Button passwordButton;
    [SerializeField] private Button groupButton;
    [SerializeField] private Button deleteButton;
    [SerializeField] private Button restoreButton;

    [Header("Аудио SFX")]
    [SerializeField] private AudioClip buttonClick;

    [Header("Громкость")]
    [Range(0f, 1f)]
    [SerializeField] private float clickVolume = 0.7f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (usernameButton != null)
            usernameButton.onClick.AddListener(OnUsernameClicked);

        if (passwordButton != null)
            passwordButton.onClick.AddListener(OnPasswordClicked);

        if (groupButton != null)
            groupButton.onClick.AddListener(OnGroupClicked);

        if (deleteButton != null)
            deleteButton.onClick.AddListener(OnDeleteClicked);

        if (restoreButton != null)
            restoreButton.onClick.AddListener(OnRestoreClicked);
    }

    public void Initializate(string id, string regdate, string login, string username, string password, string group, string deleted)
    {
        this.id = id;
        this.regdate = regdate;
        this.login = login;
        this.username = username;
        this.password = password;
        this.group = group;
        this.deleted = deleted;

        userDLogin.text = "ID: " + id + " (" + login + ") " + (this.deleted == "" ? regdate : "Деактивирован");
        this.usernameText.text = username;
        this.passwordText.text = "**********";
        this.groupText.text = group;

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnUsernameClicked()
    {
        AudioManager.PlayOneShot(buttonClick, clickVolume);
        
        if(this.editingUsername)
        {
            this.editingUsername = false;
            this.usernameInput.text = "";
            this.usernameInput.gameObject.SetActive(false);
            this.usernameText.gameObject.SetActive(true);

        } else
        {
            this.editingUsername = true;
            this.usernameInput.text = this.usernameText.text;
            this.usernameInput.gameObject.SetActive(true);
            this.usernameText.gameObject.SetActive(false);
            usernameInput.GetComponent<UsernameInput>().id = this.id;
            usernameInput.ActivateInputField();
        }
    }

    void OnPasswordClicked()
    {
        AudioManager.PlayOneShot(buttonClick, clickVolume);

        if (this.editingPassword)
        {
            this.editingPassword = false;
            this.passwordInput.text = "";
            this.passwordInput.gameObject.SetActive(false);
            this.passwordText.gameObject.SetActive(true);

        }
        else
        {
            this.editingPassword = true;
            this.passwordInput.text = "";
            this.passwordInput.gameObject.SetActive(true);
            this.passwordText.gameObject.SetActive(false);
            passwordInput.GetComponent<PasswordInput>().id = this.id;
            passwordInput.ActivateInputField();
        }
    }

    void OnGroupClicked()
    {
        AudioManager.PlayOneShot(buttonClick, clickVolume);

        if (this.editingGroup)
        {
            this.editingGroup = false;
            this.groupInput.text = "";
            this.groupInput.gameObject.SetActive(false);
            this.groupText.gameObject.SetActive(true);

        }
        else
        {
            this.editingGroup = true;
            this.groupInput.text = this.groupText.text;
            this.groupInput.gameObject.SetActive(true);
            this.groupText.gameObject.SetActive(false);
            groupInput.GetComponent<GroupInput>().id = this.id;
            groupInput.ActivateInputField();
        }
    }

    async void OnDeleteClicked()
    {
        AudioManager.PlayOneShot(buttonClick, clickVolume);
        this.deleteButton.interactable = false;

        var formData = new List<KeyValuePair<string, string>>
        {
            new KeyValuePair<string, string>("pack[service]", "account"),
            new KeyValuePair<string, string>("pack[method]", "deleteAccount"),
            new KeyValuePair<string, string>("pack[access_key]", Settings.AuthKey),
            new KeyValuePair<string, string>("pack[info][id]", this.id)
        };

        try
        {
            JObject result = await Sender.SendAndGet(formData);
            this.RefreshUser();
        }
        catch (Exception)
        {
        }
        finally
        {
            this.deleteButton.interactable = true;
        }
    }

    async void OnRestoreClicked()
    {
        AudioManager.PlayOneShot(buttonClick, clickVolume);
        this.restoreButton.interactable = false;

        var formData = new List<KeyValuePair<string, string>>
        {
            new KeyValuePair<string, string>("pack[service]", "account"),
            new KeyValuePair<string, string>("pack[method]", "restoreAccount"),
            new KeyValuePair<string, string>("pack[access_key]", Settings.AuthKey),
            new KeyValuePair<string, string>("pack[info][id]", this.id)
        };

        try
        {
            JObject result = await Sender.SendAndGet(formData);
            this.RefreshUser();
        }
        catch (Exception)
        {
        }
        finally
        {
            this.restoreButton.interactable = true;
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
                        Initializate(
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
