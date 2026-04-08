using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class MessageBubble : MonoBehaviour
{
    public string id;
    public bool showed;
    public bool edited;
    private string timestring;

    [Header("UI Elements")]
    [SerializeField] private TMP_Text username;
    [SerializeField] private TMP_Text message;
    [SerializeField] private TMP_Text time;
    [SerializeField] private GameObject panel;
    [SerializeField] private TMP_InputField messageEditor;
    [SerializeField] private ScrollRect messages;
    [SerializeField] public TMP_InputField messageInput;


    [SerializeField] private Button burgerButton;
    [SerializeField] private Button editButton;
    [SerializeField] private Button restoreButton;
    [SerializeField] private Button deleteButton;
    [SerializeField] private Button quoteButton;


    [Header("Canvas")]
    public Canvas targetCanvas; // Ссылка на Canvas, куда создавать звездочки

    [Header("Настройки частиц")]
    public GameObject starParticlePrefab; // Префаб звездочки (должен иметь Image или SpriteRenderer)
    public int starCount = 15;            // Количество звездочек

    [Header("Настройки падения")]
    public float destroyAfter = 3f;       // Через сколько секунд удалить звездочки


    public void Initialize(string id, string username, string message, string time, bool my_message, ScrollRect messageList, TMP_InputField i_f)
    {
        this.id = id;
        this.username.text = username;
        this.message.text = message;
        this.time.text = time;
        this.timestring = time;
        this.messages = messageList;
        this.messageInput = i_f;


        if (my_message)
        {
            VerticalLayoutGroup layoutGroup = GetComponent<VerticalLayoutGroup>();
            if (layoutGroup != null)
            {
                layoutGroup.padding.left = 600;
                layoutGroup.padding.right = 20;
            }

            Image img = panel.GetComponent<Image>();
            if (img != null)
            {
                img.color = new Color32(128, 166, 255, 180);
            }

            this.username.enabled = false;
        }
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        showed = false;
        editButton.gameObject.SetActive(false);
        restoreButton.gameObject.SetActive(false);
        deleteButton.gameObject.SetActive(false);
        quoteButton.gameObject.SetActive(false);
        messageEditor.gameObject.SetActive(false);

        if (burgerButton != null)
            burgerButton.onClick.AddListener(OnBurgerButtonClick);

        if (deleteButton != null)
            deleteButton.onClick.AddListener(OnDeleteButtonClick);

        if (restoreButton != null)
            restoreButton.onClick.AddListener(OnRestoreButtonClick);

        if (editButton != null)
            editButton.onClick.AddListener(OnEditButtonClick);

        if (quoteButton != null)
            quoteButton.onClick.AddListener(OnQuoteButtonClick);
    }

    void Update()
    {
    }

    void OnBurgerButtonClick()
    {
        if(showed == false)
        {
            foreach (Transform child in messages.content)
            {
                child.GetComponent<MessageBubble>()?.HideButtons();
            }

            showed = true;
            editButton.gameObject.SetActive(true);
            restoreButton.gameObject.SetActive(true);
            deleteButton.gameObject.SetActive(true);
            quoteButton.gameObject.SetActive(true);
        }
        else
        {
            showed = false;
            editButton.gameObject.SetActive(false);
            restoreButton.gameObject.SetActive(false);
            deleteButton.gameObject.SetActive(false);
            quoteButton.gameObject.SetActive(false);
        }

        messageInput.ActivateInputField();
        messageInput.caretPosition = Settings.lastCaretPosition;
        messageInput.selectionFocusPosition = messageInput.caretPosition;
    }

    async void OnDeleteButtonClick()
    {
        var formData = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("pack[service]", "message"),
                new KeyValuePair<string, string>("pack[method]", "delete"),
                new KeyValuePair<string, string>("pack[access_key]", Settings.AuthKey),
                new KeyValuePair<string, string>("pack[info][id]", this.id),
            };

        try
        {
            deleteButton.interactable = false;
            Newtonsoft.Json.Linq.JObject result = await Sender.SendAndGet(formData);
            this.time.text = "Удалено";
        }
        catch (Exception){}
        finally
        {
            deleteButton.interactable = true;
            messageInput.ActivateInputField();
            messageInput.caretPosition = Settings.lastCaretPosition;
            messageInput.selectionFocusPosition = messageInput.caretPosition;
        }
    }

    async void OnRestoreButtonClick()
    {
        var formData = new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("pack[service]", "message"),
                    new KeyValuePair<string, string>("pack[method]", "restore"),
                    new KeyValuePair<string, string>("pack[access_key]", Settings.AuthKey),
                    new KeyValuePair<string, string>("pack[info][id]", this.id),
                };

        try
        {
            restoreButton.interactable = false;
            Newtonsoft.Json.Linq.JObject result = await Sender.SendAndGet(formData);
            this.time.text = this.timestring;
        }
        catch (Exception) { }
        finally
        {
            restoreButton.interactable = true;
            messageInput.ActivateInputField();
            messageInput.caretPosition = Settings.lastCaretPosition;
            messageInput.selectionFocusPosition = messageInput.caretPosition;
        }
    }

    void OnEditButtonClick()
    {
        if (edited == false)
        {
            edited = true;
            messageEditor.gameObject.SetActive(true);
            message.gameObject.SetActive(false);
            messageEditor.text = message.text;
            messageEditor.ActivateInputField();
            messageEditor.caretPosition = messageEditor.text.Length;
            messageEditor.selectionFocusPosition = messageEditor.caretPosition;
            messageEditor.GetComponent<EditMessage>().id = this.id;
            messageEditor.GetComponent<EditMessage>().message = this.message;
            messageEditor.GetComponent<EditMessage>().mb = this;

        } else
        {
            edited = false;
            messageEditor.gameObject.SetActive(false);
            message.gameObject.SetActive(true);
            messageEditor.text = "";
            messageInput.ActivateInputField();
            messageInput.caretPosition = Settings.lastCaretPosition;
            messageInput.selectionFocusPosition = messageInput.caretPosition;
        }
    }

    void OnQuoteButtonClick()
    {
        MessageBox.Show("Информация", "Нажата кнопка цитирования");
    }

    public void HideButtons()
    {
        showed = false;
        editButton.gameObject.SetActive(false);
        restoreButton.gameObject.SetActive(false);
        deleteButton.gameObject.SetActive(false);
        quoteButton.gameObject.SetActive(false);
        messageEditor.gameObject.SetActive(false);
        message.gameObject.SetActive(true);
    }



    public void Ignite()
    {
        if (targetCanvas == null)
        {
            GameObject canvasObj = GameObject.Find("MainCanvas");

            if (canvasObj == null)
            {
                Debug.LogError("Canvas не найден!");
                return;
            }
            else
            {
               targetCanvas = canvasObj.GetComponent<Canvas>();
            }
        }

        // Вспышка света

        // Выброс звездочек
        if (starParticlePrefab == null)
        {
            Debug.LogError("STAR PREFAB = NULL! Префаб звездочки не назначен!");
            return;
        }

        if (targetCanvas == null)
        {
            Debug.LogError("TARGET CANVAS = NULL!");
            return;
        }

        for (int i = 0; i < starCount; i++)
        {
            // Создаем звездочку как дочерний объект Canvas
            GameObject star = Instantiate(starParticlePrefab, targetCanvas.transform);

            // Позиция как у текущего сообщения
            star.transform.position = transform.position;

            // Случайное смещение
            Vector3 randomOffset = new Vector3(
                UnityEngine.Random.Range(-100f, 100f),
                UnityEngine.Random.Range(-50f, 150f),
                0
            );
            star.transform.position += randomOffset;

            // поднимаем звездочку на передний план
            RectTransform starRect = star.GetComponent<RectTransform>();
            if (starRect != null)
            {
                // Делаем звездочку последним дочерним объектом (рисуется поверх всех)
                starRect.SetAsLastSibling();
            }

            // Добавляем UI анимацию вместо Rigidbody
            StartCoroutine(AnimateUIStar(star));

            Destroy(star, destroyAfter);
        }
    }

    private IEnumerator AnimateUIStar(GameObject star)
    {
        RectTransform rect = star.GetComponent<RectTransform>();
        if (rect == null) yield break;

        Vector2 velocity = new Vector2(
            UnityEngine.Random.Range(-200f, 200f),
            UnityEngine.Random.Range(200f, 400f)
        );

        float gravity = 600f;
        float elapsed = 0f;

        while (elapsed < destroyAfter && star != null)
        {
            elapsed += Time.deltaTime;
            velocity.y -= gravity * Time.deltaTime;
            rect.anchoredPosition += velocity * Time.deltaTime;
            rect.Rotate(0, 0, 360f * Time.deltaTime);

            // Затухание
            Image img = star.GetComponent<Image>();
            if (img != null)
            {
                Color c = img.color;
                c.a = Mathf.Lerp(1f, 0f, elapsed / destroyAfter);
                img.color = c;
            }

            yield return null;
        }
    }

    public IEnumerator IgniteWithDelay()
    {
        yield return new WaitForSeconds(0.2f);
        this.Ignite();
    }
}
