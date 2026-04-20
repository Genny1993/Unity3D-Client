using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

#if UNITY_ANDROID && !UNITY_EDITOR
    using NativeFilePickerNamespace;
#elif UNITY_STANDALONE_WIN
using SFB;
#endif



public class MessageBubble : MonoBehaviour
{
    public string id;
    public string qid;
    public bool showed;
    public bool edited;
    private string timestring;
    private string aid;
    private string aname;
    private string asize;
    public bool my_message = false;
    private bool is_admin_history = false;

    [Header("UI Elements")]
    [SerializeField] private TMP_Text quoteName;
    [SerializeField] private TMP_Text quoteMessage;
    [SerializeField] private Image quoteColorBar;
    [SerializeField] private GameObject quoteBar;
    [SerializeField] private TMP_Text username;
    [SerializeField] private TMP_InputField message;
    [SerializeField] private TMP_Text time;
    [SerializeField] private GameObject panel;
    [SerializeField] private TMP_InputField messageEditor;
    [SerializeField] private ScrollRect messages;
    [SerializeField] public TMP_InputField messageInput;
    [SerializeField] private GameObject fileBar;
    [SerializeField] public TMP_Text fileName;
    [SerializeField] private GameObject leftPadding;
    [SerializeField] private GameObject rightPadding;



    [SerializeField] private UnityEngine.UI.Button burgerButton;
    [SerializeField] private UnityEngine.UI.Button editButton;
    [SerializeField] private UnityEngine.UI.Button restoreButton;
    [SerializeField] private UnityEngine.UI.Button deleteButton;
    [SerializeField] private UnityEngine.UI.Button quoteButton;
    [SerializeField] private UnityEngine.UI.Button inQuoteButton;
    [SerializeField] private UnityEngine.UI.Button inQuoteButton2;
    [SerializeField] private UnityEngine.UI.Button fileButton;
    [SerializeField] private UnityEngine.UI.Button editOkButton;
    [SerializeField] private UnityEngine.UI.Button messageSelectButton;
    [SerializeField] private UnityEngine.UI.Button copyButton;

    [SerializeField] private GameObject statusBar;
    [SerializeField] private GameObject quoteBarMain;
    [SerializeField] private TMP_Text quoteLabel;

    [Header("Аудио SFX")]
    [SerializeField] private AudioClip buttonClick;

    [Header("Громкость")]
    [Range(0f, 1f)]
    [SerializeField] private float clickVolume = 0.7f;


    [Header("Canvas")]
    public Canvas targetCanvas; // Ссылка на Canvas, куда создавать звездочки

    [Header("Настройки частиц")]
    public GameObject starParticlePrefab; // Префаб звездочки (должен иметь Image или SpriteRenderer)
    public int starCount = 15;            // Количество звездочек

    [Header("Настройки падения")]
    public float destroyAfter = 3f;       // Через сколько секунд удалить звездочки


    public void Initialize(string id, string username, string message, string time, string quoteid, string quotename, string quotemessage, bool my_message, string a_id, string a_name, string a_size, ScrollRect messageList, TMP_InputField i_f, GameObject status_bar, GameObject quote_bar, TMP_Text quoteLabel, bool show_window = false)
    {
        this.id = id;
        this.username.text = username;
        this.message.text = message;
        this.time.text = FormatDate(time);
        this.timestring = FormatDate(time);
        this.messages = messageList;
        this.messageInput = i_f;
        this.qid = quoteid;
        this.quoteName.text = quotename;
        this.quoteMessage.text = quotemessage;
        this.statusBar = status_bar;
        this.quoteBarMain = quote_bar;
        this.quoteLabel = quoteLabel;
        this.aid = a_id;
        this.aname = a_name;
        this.asize = a_size;
        this.my_message = my_message;

        if( i_f == null
            && status_bar == null
            && quote_bar == null
            && quoteLabel == null)
        {
            this.is_admin_history = true;
        }


        if (my_message)
        {
            HorizontalLayoutGroup layoutGroup = GetComponent<HorizontalLayoutGroup>();
            if (layoutGroup != null)
            {
                layoutGroup.padding.left = 100;
                layoutGroup.padding.right = 0;
                leftPadding.SetActive(true);
                rightPadding.SetActive(false);
            }

            Image img = panel.GetComponent<Image>();
            if (img != null)
            {
                img.color = new Color32(128, 166, 255, 180);
                quoteColorBar.color = new Color32(0, 0, 255, 150);
            }

            this.username.enabled = false;

        } else
        {
            HorizontalLayoutGroup layoutGroup = GetComponent<HorizontalLayoutGroup>();
            if (layoutGroup != null)
            {
                layoutGroup.padding.left = 5;
                layoutGroup.padding.right = 100;
                leftPadding.SetActive(false);
                rightPadding.SetActive(true);
            }
        }

        if(my_message || Settings.isAdmin)
        {
            editButton.gameObject.SetActive(false);
            restoreButton.gameObject.SetActive(false);
            deleteButton.gameObject.SetActive(false);
            quoteButton.gameObject.SetActive(false);
            messageEditor.gameObject.SetActive(false);
            burgerButton.gameObject.SetActive(true);
        } else
        {
            editButton.gameObject.SetActive(false);
            restoreButton.gameObject.SetActive(false);
            deleteButton.gameObject.SetActive(false);
            if (!this.is_admin_history)
            {
                quoteButton.gameObject.SetActive(true);
            }
            messageEditor.gameObject.SetActive(false);
            burgerButton.gameObject.SetActive(false);
        }

        if(quotemessage == "")
        {
            quoteBar.SetActive(false);
        }

        if(this.aid == "")
        {
            fileBar.SetActive(false);
        } else
        {
            fileName.text = "📄  " + this.aname + ", " + FormatBytes(long.Parse(this.asize));
        }

        if (show_window)
        {
            HorizontalLayoutGroup layoutGroup = GetComponent<HorizontalLayoutGroup>();
            layoutGroup.padding.left = 5;
            layoutGroup.padding.right = 5;
            leftPadding.SetActive(false);
            rightPadding.SetActive(false);
        }

        editOkButton.gameObject.SetActive(false);
        this.message.interactable = false;
        this.copyButton.gameObject.SetActive(false);
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

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

        if (inQuoteButton != null)
            inQuoteButton.onClick.AddListener(OnInQuoteButtonClick);

        if (inQuoteButton2 != null)
            inQuoteButton2.onClick.AddListener(OnInQuoteButtonClick);

        if (fileButton != null)
            fileButton.onClick.AddListener(OnFileButtonClick);

        if (editOkButton != null)
            editOkButton.onClick.AddListener(OnEditOkButtonClick);

        if (messageSelectButton != null)
            messageSelectButton.onClick.AddListener(OnMessageSelectButtonClick);

        if (copyButton != null)
            copyButton.onClick.AddListener(OnCopyButtonClick);

    }

    void Update()
    {
    }

    void OnCopyButtonClick()
    {
        AudioManager.PlayOneShot(buttonClick, clickVolume);
        this.message.interactable = false;
        this.messageSelectButton.gameObject.SetActive(true);
        this.copyButton.gameObject.SetActive(false);

        int anchorPos = this.message.selectionAnchorPosition;
        int focusPos = this.message.selectionFocusPosition;

        int startIndex = Mathf.Min(anchorPos, focusPos);
        int endIndex = Mathf.Max(anchorPos, focusPos);

        string fullText = this.message.text;

        if (startIndex < 0 || endIndex > fullText.Length)
        {
            Debug.LogWarning($"Индексы выделения выходят за границы: start={startIndex}, end={endIndex}, text.Length={fullText.Length}");
            return;
        }

        // Правильное извлечение текста с учётом эмодзи
        string selectedText = GetSubstringWithEmojis(fullText, startIndex, endIndex - startIndex);

        CopyToClipboard(selectedText);
    }

    private void CopyToClipboard(string text)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
    using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        {
            AndroidJavaObject context = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

            using (AndroidJavaObject clipboard = context.Call<AndroidJavaObject>("getSystemService", "clipboard"))
            {
                using (AndroidJavaClass clipDataClass = new AndroidJavaClass("android.content.ClipData"))
                {
                    AndroidJavaObject clipData = clipDataClass.CallStatic<AndroidJavaObject>(
                        "newPlainText",
                        "label",
                        text
                    );

                    clipboard.Call("setPrimaryClip", clipData);
                }
            }
        }
#else
        GUIUtility.systemCopyBuffer = text;
#endif
    }

    private string GetSubstringWithEmojis(string text, int startIndex, int length)
    {
        if (string.IsNullOrEmpty(text))
            return "";

        // Преобразуем строку в массив символов Unicode (с учётом суррогатных пар)
        var stringInfo = new System.Globalization.StringInfo(text);

        int charCount = stringInfo.LengthInTextElements; // реальное количество символов (эмодзи считаются за 1)

        if (startIndex < 0 || startIndex >= charCount)
            return "";

        int endCharIndex = Mathf.Min(startIndex + length, charCount);
        int actualLength = endCharIndex - startIndex;

        if (actualLength <= 0)
            return "";

        return stringInfo.SubstringByTextElements(startIndex, actualLength);
    }


    void OnMessageSelectButtonClick()
    {
        foreach (Transform child in messages.content)
        {
            child.GetComponent<MessageBubble>()?.HideButtons();
        }

        this.message.interactable = true;
        this.messageSelectButton.gameObject.SetActive(false);
        this.copyButton.gameObject.SetActive(true);

    }

    async void OnEditOkButtonClick()
    {
        AudioManager.PlayOneShot(buttonClick, clickVolume);

        var formData = new List<KeyValuePair<string, string>>
        {
            new KeyValuePair<string, string>("pack[service]", "message"),
            new KeyValuePair<string, string>("pack[method]", "edit"),
            new KeyValuePair<string, string>("pack[access_key]", Settings.AuthKey),
            new KeyValuePair<string, string>("pack[info][id]", this.id),
            new KeyValuePair<string, string>("pack[info][message]", messageEditor.text.Trim())
        };

        try
        {
            messageEditor.interactable = false;
            Newtonsoft.Json.Linq.JObject result = await Sender.SendAndGet(formData);
            message.text = messageEditor.text.Trim();

        }
        catch (Exception) { }
        finally
        {
            messageEditor.text = "";
            messageEditor.gameObject.SetActive(false);
            message.gameObject.SetActive(true);
            edited = false;

            if (messageInput != null)
            {
                if (Settings.isPCProgram)
                {
                    messageInput.ActivateInputField();
                    messageInput.caretPosition = Settings.lastCaretPosition;
                    messageInput.selectionFocusPosition = messageInput.caretPosition;
                }
            }

            messageEditor.interactable = true;
            editOkButton.gameObject.SetActive(false);
        }
    }

    void OnBurgerButtonClick()
    {
        AudioManager.PlayOneShot(buttonClick, clickVolume);

        if (showed == false)
        {
            foreach (Transform child in messages.content)
            {
                child.GetComponent<MessageBubble>()?.HideButtons();
            }

            showed = true;
            editButton.gameObject.SetActive(true);
            restoreButton.gameObject.SetActive(true);
            deleteButton.gameObject.SetActive(true);

            if (!this.is_admin_history)
            {
                quoteButton.gameObject.SetActive(true);
            }
        }
        else
        {
            showed = false;
            editButton.gameObject.SetActive(false);
            restoreButton.gameObject.SetActive(false);
            deleteButton.gameObject.SetActive(false);
            quoteButton.gameObject.SetActive(false);
        }

        if (!this.is_admin_history)
        {
            if (Settings.isPCProgram)
            {
                messageInput.ActivateInputField();
                messageInput.caretPosition = Settings.lastCaretPosition;
                messageInput.selectionFocusPosition = messageInput.caretPosition;
            }
        }
    }

    async void OnDeleteButtonClick()
    {
        AudioManager.PlayOneShot(buttonClick, clickVolume);

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

            if (!this.is_admin_history)
            {
                if (Settings.isPCProgram)
                {
                    messageInput.ActivateInputField();
                    messageInput.caretPosition = Settings.lastCaretPosition;
                    messageInput.selectionFocusPosition = messageInput.caretPosition;
                }
            }
        }
    }

    async void OnRestoreButtonClick()
    {
        AudioManager.PlayOneShot(buttonClick, clickVolume);

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

            if (!this.is_admin_history)
            {
                if (Settings.isPCProgram)
                {
                    messageInput.ActivateInputField();
                    messageInput.caretPosition = Settings.lastCaretPosition;
                    messageInput.selectionFocusPosition = messageInput.caretPosition;
                }
            }
        }
    }

    void OnEditButtonClick()
    {
        AudioManager.PlayOneShot(buttonClick, clickVolume);

        if (edited == false)
        {
            editOkButton.gameObject.SetActive(true);
            edited = true;
            messageEditor.gameObject.SetActive(true);
            message.gameObject.SetActive(false);
            messageEditor.text = message.text;

            if (Settings.isPCProgram)
            {
                messageEditor.ActivateInputField();
                messageEditor.caretPosition = messageEditor.text.Length;
                messageEditor.selectionFocusPosition = messageEditor.caretPosition;
            }

            messageEditor.GetComponent<EditMessage>().id = this.id;
            messageEditor.GetComponent<EditMessage>().message = this.message;
            messageEditor.GetComponent<EditMessage>().mb = this;
            messageEditor.ActivateInputField();
            StartCoroutine(CenterCoroutine());

        } else
        {
            editOkButton.gameObject.SetActive(false);
            edited = false;
            messageEditor.gameObject.SetActive(false);
            message.gameObject.SetActive(true);
            messageEditor.text = "";

            if (!this.is_admin_history)
            {
                if (Settings.isPCProgram)
                {
                    messageInput.ActivateInputField();
                    messageInput.caretPosition = Settings.lastCaretPosition;
                    messageInput.selectionFocusPosition = messageInput.caretPosition;
                }
            }
        }
    }

    void OnQuoteButtonClick()
    {
        AudioManager.PlayOneShot(buttonClick, clickVolume);

        Settings.quoteBar = true;

        if (!this.is_admin_history)
        {
            if (statusBar.activeInHierarchy == false)
            {
                RectTransform rect = messages.GetComponent<RectTransform>();
                Vector2 size = rect.sizeDelta;
                size.y = Settings.currentMessagesListHeight - 40;
                Settings.currentMessagesListHeight = (int)size.y;
                rect.sizeDelta = size;
            }

            Settings.QuotedId = this.id;
            statusBar.SetActive(true);
            quoteBarMain.SetActive(true);
            quoteLabel.text = "Цитата: " + this.id;
        }

        if (!this.is_admin_history)
        {
            if (Settings.isPCProgram)
            {
                messageInput.ActivateInputField();
                messageInput.caretPosition = Settings.lastCaretPosition;
                messageInput.selectionFocusPosition = messageInput.caretPosition;
            }
        }
    }

    void OnInQuoteButtonClick()
    {
        AudioManager.PlayOneShot(buttonClick, clickVolume);

        MessageShowerWindow.Show(this.qid, this.messages, this.messageInput, this.statusBar, this.quoteBarMain, this.quoteLabel);
    }

    async void OnFileButtonClick()
    {
        AudioManager.PlayOneShot(buttonClick, clickVolume);

        var formData = new List<KeyValuePair<string, string>>
        {
            new KeyValuePair<string, string>("pack[service]", "file"),
            new KeyValuePair<string, string>("pack[method]", "download"),
            new KeyValuePair<string, string>("pack[access_key]", Settings.AuthKey),
            new KeyValuePair<string, string>("pack[info][id]", this.aid)
        };

        try
        {

            using (HttpClient client = new HttpClient())
            {
                // Отправляем POST запрос с form-urlencoded данными
                string json = JsonConverter.To(formData);
                json = Crypt.Encrypt(json);
                var content = new StringContent(json, Encoding.UTF8, "text/plain");

                MessageBox.Show("Информация", "Началось скачивание файла. Пожалуйста, ожидайте конца");
                using (var response = await client.PostAsync(Settings.Url, content))
                {
                    response.EnsureSuccessStatusCode();

                    // Получаем файл как массив байтов
                    byte[] fileBytes = await response.Content.ReadAsByteArrayAsync();


#if UNITY_ANDROID && !UNITY_EDITOR
string tempPath = Path.Combine(Application.persistentDataPath, this.aname);
        File.WriteAllBytes(tempPath, fileBytes);
        
        // Открываем диалог сохранения (экспорта)
        NativeFilePicker.ExportFile(tempPath, (success) => {
            Debug.Log(success ? "Файл сохранён" : "Сохранение отменено");
        });
#elif UNITY_STANDALONE_WIN
                    string path = StandaloneFileBrowser.SaveFilePanel("Сохранить файл", "", this.aname, "");
                    // Проверяем, не нажал ли пользователь "Отмена"
                    if (string.IsNullOrEmpty(path))
                    {
                        //MessageBox.Show("Ошибка", "Сохранение отменено!");
                        return;
                    }

                    // Если путь получен, сохраняем ваш массив байтов в файл
                    try
                    {
                        // Записываем все байты из массива в выбранный файл
                        File.WriteAllBytes(path, fileBytes);
                        //MessageBox.Show("Информация", $"Файл сохранен: {path}");
                    }
                    catch (System.Exception e)
                    {
                        MessageBox.Show("Ошибка", $"Ошибка при сохранении файла: {e.Message}");
                    }
#endif
                }

            }

        }
        catch (Exception)
        {
            MessageBox.Show("Ошибка", "Файл не удалось сохранить!");
        }
    }

    public void HideButtons()
    {
        if (my_message || Settings.isAdmin)
        {
            showed = false;
            editButton.gameObject.SetActive(false);
            restoreButton.gameObject.SetActive(false);
            deleteButton.gameObject.SetActive(false);
            quoteButton.gameObject.SetActive(false);
            messageEditor.gameObject.SetActive(false);
            message.gameObject.SetActive(true);
            editOkButton.gameObject.SetActive(false);
            copyButton.gameObject.SetActive(false);
            this.message.interactable = false;
            this.messageSelectButton.gameObject.SetActive(true);
            this.edited = false;
        } else
        {
            showed = false;
            editButton.gameObject.SetActive(false);
            restoreButton.gameObject.SetActive(false);
            deleteButton.gameObject.SetActive(false);
            if (!this.is_admin_history)
            {
                quoteButton.gameObject.SetActive(true);
            }
            messageEditor.gameObject.SetActive(false);
            message.gameObject.SetActive(true);
            burgerButton.gameObject.SetActive(false);
            editOkButton.gameObject.SetActive(false);
            copyButton.gameObject.SetActive(false);
            this.message.interactable = false;
            this.messageSelectButton.gameObject.SetActive(true);
            this.edited = false;
        }
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

    public void Depadding()
    {
        VerticalLayoutGroup layoutGroup = GetComponent<VerticalLayoutGroup>();
        if (layoutGroup != null)
        {
            layoutGroup.padding.left = 10;
            layoutGroup.padding.right = 10;
        }
    }

    public static string FormatBytes(long bytes)
    {
        string[] units = { "Б", "КБ", "МБ", "ГБ" };
        int unitIndex = 0;
        double size = bytes;

        // Пока размер больше 1024 и не достигли последней единицы
        while (size >= 1024 && unitIndex < units.Length - 1)
        {
            size /= 1024;
            unitIndex++;
        }

        // Форматируем результат
        string result = $"{size:F2} {units[unitIndex]}";

        return result;
    }

    private IEnumerator CenterCoroutine()
    {
        yield return new WaitForSeconds(0.5f);

        RectTransform target = (RectTransform)transform;
        RectTransform content = messages.content;
        RectTransform viewport = messages.viewport;

        // Обновляем layout (критично при разной высоте)
        LayoutRebuilder.ForceRebuildLayoutImmediate(content);

        // Центр элемента
        Vector3 worldPoint = target.TransformPoint(target.rect.center);
        Vector3 localPoint = content.InverseTransformPoint(worldPoint);

        // Центр viewport
        Vector3 viewportLocal = content.InverseTransformPoint(
            viewport.TransformPoint(viewport.rect.center)
        );

        // Смещение
        Vector2 delta = (Vector2)(viewportLocal - localPoint);

        // Новая позиция content
        Vector2 targetPos = content.anchoredPosition + new Vector2(0, delta.y);

        // Ограничение
        float maxY = Mathf.Max(0, content.rect.height - viewport.rect.height);
        targetPos.y = Mathf.Clamp(targetPos.y, 0, maxY);

        // Плавный скролл
        Vector2 startPos = content.anchoredPosition;
        float time = 0f;

        while (time < 0.25f)
        {
            time += Time.deltaTime;
            content.anchoredPosition = Vector2.Lerp(startPos, targetPos, time / 0.25f);
            yield return null;
        }

        content.anchoredPosition = targetPos;
    }

    public string FormatDate(string dateString)
    {
        DateTime date = DateTime.ParseExact(dateString, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);

        return date.ToString("dd MMMM HH:mm", new CultureInfo("ru-RU"));
    }
}
