using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;

public class ChatPanel : MonoBehaviour
{
    private string id = "";
    private string regdate = "";
    private string cname = "";
    private string deleted = "";

    public bool editingName = false;


    [Header("UI Elements")]
    [SerializeField] private TMP_Text chatDName;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_InputField nameInput;
    [SerializeField] private Button nameButton;
    [SerializeField] private Button deleteButton;
    [SerializeField] private Button restoreButton;
    [SerializeField] private Button historyButton;
    [SerializeField] private Button saveNameButton;

    [Header("Аудио SFX")]
    [SerializeField] private AudioClip buttonClick;

    [Header("Громкость")]
    [Range(0f, 1f)]
    [SerializeField] private float clickVolume = 0.7f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (nameButton != null)
            nameButton.onClick.AddListener(OnNameClicked);

        if (deleteButton != null)
            deleteButton.onClick.AddListener(OnDeleteClicked);

        if (restoreButton != null)
            restoreButton.onClick.AddListener(OnRestoreClicked);

        if (historyButton != null)
            historyButton.onClick.AddListener(OnHistoryClicked);
        
        if (saveNameButton != null)
            saveNameButton.onClick.AddListener(OnSaveNameClicked);
    }

    public void Initializate(string id, string regdate, string name, string deleted)
    {
        this.id = id;
        this.regdate = regdate;
        this.cname = name;
        this.deleted = deleted;

        chatDName.text = "ID: " + id + " " + (this.deleted == "" ? regdate : "Деактивирован");
        this.nameText.text = name;

        this.saveNameButton.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    async void OnSaveNameClicked()
    {
        AudioManager.PlayOneShot(buttonClick, clickVolume);

        var formData = new List<KeyValuePair<string, string>>
        {
            new KeyValuePair<string, string>("pack[service]", "chat"),
            new KeyValuePair<string, string>("pack[method]", "rename"),
            new KeyValuePair<string, string>("pack[access_key]", Settings.AuthKey),
            new KeyValuePair<string, string>("pack[info][id]", this.id),
            new KeyValuePair<string, string>("pack[info][name]", nameInput.text.Trim())
        };

        try
        {
            nameInput.interactable = false;
            Newtonsoft.Json.Linq.JObject result = await Sender.SendAndGet(formData);
            nameText.text = nameInput.text;
            RefreshChat();

        }
        catch (Exception) { }
        finally
        {
            nameInput.interactable = true;
            if (this.editingName)
            {
                this.editingName = false;
                this.nameInput.text = "";
                this.nameInput.gameObject.SetActive(false);
                this.nameText.gameObject.SetActive(true);
                this.saveNameButton.gameObject.SetActive(false);

            }
        }

    }

    void OnNameClicked()
    {
        AudioManager.PlayOneShot(buttonClick, clickVolume);

        if (this.editingName)
        {
            this.editingName = false;
            this.nameInput.text = "";
            this.nameInput.gameObject.SetActive(false);
            this.nameText.gameObject.SetActive(true);
            this.saveNameButton.gameObject.SetActive(false);

        }
        else
        {
            this.editingName = true;
            this.nameInput.text = this.nameText.text;
            this.nameInput.gameObject.SetActive(true);
            this.nameText.gameObject.SetActive(false);
            nameInput.ActivateInputField();
            this.saveNameButton.gameObject.SetActive(true);
        }
    }

    async void OnDeleteClicked()
    {
        AudioManager.PlayOneShot(buttonClick, clickVolume);
        this.deleteButton.interactable = false;

        var formData = new List<KeyValuePair<string, string>>
        {
            new KeyValuePair<string, string>("pack[service]", "chat"),
            new KeyValuePair<string, string>("pack[method]", "delete"),
            new KeyValuePair<string, string>("pack[access_key]", Settings.AuthKey),
            new KeyValuePair<string, string>("pack[info][id]", this.id)
        };

        try
        {
            JObject result = await Sender.SendAndGet(formData);
            this.RefreshChat();
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
            new KeyValuePair<string, string>("pack[service]", "chat"),
            new KeyValuePair<string, string>("pack[method]", "restore"),
            new KeyValuePair<string, string>("pack[access_key]", Settings.AuthKey),
            new KeyValuePair<string, string>("pack[info][id]", this.id)
        };

        try
        {
            JObject result = await Sender.SendAndGet(formData);
            this.RefreshChat();
        }
        catch (Exception)
        {
        }
        finally
        {
            this.restoreButton.interactable = true;
        }
    }

    void OnHistoryClicked()
    {
        AudioManager.PlayOneShot(buttonClick, clickVolume);
        HistoryWindowStart.Show(this.id, null, null, null, null, null);
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
                        Initializate(
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
