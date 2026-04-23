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
    [SerializeField] private FastButton usernameButton;
    [SerializeField] private FastButton passwordButton;
    [SerializeField] private FastButton groupButton;
    [SerializeField] private FastButton saveUsernameButton;
    [SerializeField] private FastButton savePasswordButton;
    [SerializeField] private FastButton saveGroupButton;
    [SerializeField] private FastButton deleteButton;
    [SerializeField] private FastButton restoreButton;


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

        if (saveUsernameButton != null)
            saveUsernameButton.onClick.AddListener(OnSaveUsernameClicked);

        if (savePasswordButton != null)
            savePasswordButton.onClick.AddListener(OnSavePasswordClicked);

        if (saveGroupButton != null)
            saveGroupButton.onClick.AddListener(OnSaveGroupClicked);

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

        this.saveUsernameButton.gameObject.SetActive(false);
        this.savePasswordButton.gameObject.SetActive(false);
        this.saveGroupButton.gameObject.SetActive(false);

    }

    // Update is called once per frame
    void Update()
    {
        
    }
    async void OnSaveUsernameClicked()
    {
        AudioManager.PlayOneShot(buttonClick, clickVolume);
        var formData = new List<KeyValuePair<string, string>>
        {
            new KeyValuePair<string, string>("pack[service]", "account"),
            new KeyValuePair<string, string>("pack[method]", "changeNameAdmin"),
            new KeyValuePair<string, string>("pack[access_key]", Settings.AuthKey),
            new KeyValuePair<string, string>("pack[info][id]", this.id),
            new KeyValuePair<string, string>("pack[info][name]", usernameInput.text.Trim())
        };

        try
        {
            usernameInput.interactable = false;
            Newtonsoft.Json.Linq.JObject result = await Sender.SendAndGet(formData);
            usernameText.text = usernameInput.text;
            RefreshUser();

        }
        catch (Exception) { }
        finally
        {
            usernameInput.interactable = true;
            usernameInput.text = "";
            usernameInput.gameObject.SetActive(false);
            usernameText.gameObject.SetActive(true);
            this.editingUsername = false;
            saveUsernameButton.gameObject.SetActive(false);
        }
    }

    async void OnSavePasswordClicked()
    {
        AudioManager.PlayOneShot(buttonClick, clickVolume);
        var formData = new List<KeyValuePair<string, string>>
        {
            new KeyValuePair<string, string>("pack[service]", "account"),
            new KeyValuePair<string, string>("pack[method]", "changePasswordAdmin"),
            new KeyValuePair<string, string>("pack[access_key]", Settings.AuthKey),
            new KeyValuePair<string, string>("pack[info][id]", this.id),
            new KeyValuePair<string, string>("pack[info][password]", passwordInput.text.Trim())
        };

        try
        {
            passwordInput.interactable = false;
            Newtonsoft.Json.Linq.JObject result = await Sender.SendAndGet(formData);
            passwordText.text = passwordInput.text;
            RefreshUser();

        }
        catch (Exception) { }
        finally
        {
            passwordInput.interactable = true;
            passwordInput.text = "";
            passwordInput.gameObject.SetActive(false);
            passwordText.gameObject.SetActive(true);
            this.editingPassword = false;
            savePasswordButton.gameObject.SetActive(false);
        }
    }

    async void OnSaveGroupClicked()
    {
        AudioManager.PlayOneShot(buttonClick, clickVolume);
        var formData = new List<KeyValuePair<string, string>>
        {
            new KeyValuePair<string, string>("pack[service]", "account"),
            new KeyValuePair<string, string>("pack[method]", "changeGroupAdmin"),
            new KeyValuePair<string, string>("pack[access_key]", Settings.AuthKey),
            new KeyValuePair<string, string>("pack[info][id]", this.id),
            new KeyValuePair<string, string>("pack[info][roles]", groupInput.text.Trim())
        };

        try
        {
            groupInput.interactable = false;
            Newtonsoft.Json.Linq.JObject result = await Sender.SendAndGet(formData);
            groupText.text = groupInput.text;
            RefreshUser();

        }
        catch (Exception) { }
        finally
        {
            groupInput.interactable = true;
            groupInput.text = "";
            groupInput.gameObject.SetActive(false);
            groupText.gameObject.SetActive(true);
            this.editingGroup = false;
            saveGroupButton.gameObject.SetActive(false);
        }
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
            this.saveUsernameButton.gameObject.SetActive(false);

        } else
        {
            this.editingUsername = true;
            this.usernameInput.text = this.usernameText.text;
            this.usernameInput.gameObject.SetActive(true);
            this.usernameText.gameObject.SetActive(false);
            usernameInput.ActivateInputField();
            this.saveUsernameButton.gameObject.SetActive(true);
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
            this.savePasswordButton.gameObject.SetActive(false);

        }
        else
        {
            this.editingPassword = true;
            this.passwordInput.text = "";
            this.passwordInput.gameObject.SetActive(true);
            this.passwordText.gameObject.SetActive(false);
            passwordInput.ActivateInputField();
            this.savePasswordButton.gameObject.SetActive(true);
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
            this.saveGroupButton.gameObject.SetActive(false);

        }
        else
        {
            this.editingGroup = true;
            this.groupInput.text = this.groupText.text;
            this.groupInput.gameObject.SetActive(true);
            this.groupText.gameObject.SetActive(false);
            groupInput.ActivateInputField();
            this.saveGroupButton.gameObject.SetActive(true);
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
