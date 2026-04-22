using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;
public class HistoryWindow : MonoBehaviour
{
    private Action onCloseCallback;
    private int count_messages;

    [Header("UI Elements")]
    [SerializeField] private Button closeButton;
    [SerializeField] private Button prevButton;
    [SerializeField] private Button nextButton;
    [SerializeField] private ScrollRect historyList;

    [SerializeField] private ScrollRect messages;
    [SerializeField] public TMP_InputField messageInput;
    [SerializeField] private GameObject statusBar;
    [SerializeField] private GameObject quoteBarMain;
    [SerializeField] private TMP_Text quoteLabel;

    [Header("Ссылка на окошко")]
    [SerializeField] private GameObject panelToAnimate;

    [Header("Настройки префаба")]
    [SerializeField] private string prefabPath = "Prefabs/MessageBubble";

    [Header("Физика падения (2D)")]
    [SerializeField] private float throwForce = 1000f;      // Сила броска
    [SerializeField] private float upForce = 1000f;         // Вертикальная сила
    [SerializeField] private float randomTorque = 5f;  // Сила вращения по Z (в градусах)

    [Header("Гравитация")]
    [SerializeField] private float gravityScale = 1000f;
    [SerializeField] private float mass = 1f;

    [Header("Исчезновение")]
    [SerializeField] private float fadeDelay = 0f;
    [SerializeField] private float fadeDuration = 0.3f;

    [Header("Аудио SFX")]
    [SerializeField] private AudioClip buttonClick;

    [Header("Громкость")]
    [Range(0f, 1f)]
    [SerializeField] private float clickVolume = 0.7f;

    private Rigidbody2D panelRigidbody2D;  // Меняем на 2D физику
    private CanvasGroup panelCanvasGroup;

    void Awake()
    {
        if (panelToAnimate == null)
        {
            panelToAnimate = transform.GetChild(0).gameObject;
            if (panelToAnimate == null)
            {
                Debug.LogError("Не найдена дочерняя Panel!");
                return;
            }
        }

        // Добавляем 2D Rigidbody на Panel
        panelRigidbody2D = panelToAnimate.GetComponent<Rigidbody2D>();
        if (panelRigidbody2D == null)
            panelRigidbody2D = panelToAnimate.AddComponent<Rigidbody2D>();

        panelRigidbody2D.bodyType = RigidbodyType2D.Kinematic;

        // Добавляем CanvasGroup для прозрачности
        panelCanvasGroup = panelToAnimate.GetComponent<CanvasGroup>();
        if (panelCanvasGroup == null)
            panelCanvasGroup = panelToAnimate.AddComponent<CanvasGroup>();

        // Настройки 2D физики
        panelRigidbody2D.gravityScale = 1f;     // Влияние гравитации
        panelRigidbody2D.bodyType = RigidbodyType2D.Kinematic; // Пока кинематическое
        panelRigidbody2D.mass = 1f;
        panelRigidbody2D.linearDamping = 0.0f;           // Сопротивление воздуху
        panelRigidbody2D.angularDamping = 0.0f;    // Сопротивление вращению

        // Добавляем 2D коллайдер на Panel
        AddColliderToPanel2D();

        panelCanvasGroup.alpha = 1f;
        panelCanvasGroup.interactable = true;
        panelCanvasGroup.blocksRaycasts = true;
    }

    void AddColliderToPanel2D()
    {
        Collider2D collider = panelToAnimate.GetComponent<Collider2D>();
        if (collider == null)
        {
            RectTransform rectTransform = panelToAnimate.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                BoxCollider2D boxCollider = panelToAnimate.AddComponent<BoxCollider2D>();
                boxCollider.size = new Vector2(rectTransform.rect.width, rectTransform.rect.height);
                boxCollider.offset = Vector2.zero;
            }
            else
            {
                BoxCollider2D boxCollider = panelToAnimate.AddComponent<BoxCollider2D>();
                boxCollider.size = new Vector2(400, 300);
            }
        }
    }


    private void Start()
    {

        if (closeButton != null)
        {
            closeButton.onClick.AddListener(OnCloseClicked);
        }

        if (prevButton != null)
        {
            prevButton.onClick.AddListener(OnPrevClicked);
        }

        if (nextButton != null)
        {
            nextButton.onClick.AddListener(OnNextClicked);
        }

    }

    public async void Initialize(string chatId, ScrollRect messageList, TMP_InputField i_f, GameObject status_bar, GameObject quote_bar, TMP_Text quoteLabel, Action onClose = null)
    {
        nextButton.gameObject.SetActive(false);
        prevButton.gameObject.SetActive(false);

        Settings.Page = 0;
        onCloseCallback = onClose;

        Settings.ChatSelected = chatId;
        this.messages = messageList;
        this.messageInput = i_f;
        this.statusBar = status_bar;
        this.quoteBarMain = quote_bar;
        this.quoteLabel = quoteLabel;
        
        bool f = await this.LoadPage();

        if (Settings.Page <= 0)
        {
            nextButton.gameObject.SetActive(false);
        }
        else
        {
            nextButton.gameObject.SetActive(true);
        }

        if (Settings.CountMessages < 100)
        {
            prevButton.gameObject.SetActive(false);
        }
        else
        {
            prevButton.gameObject.SetActive(true);
        }

    }

    private void OnCloseClicked()
    {
        Close();
    }

    private async void OnPrevClicked()
    {
        ++Settings.Page;
        bool f = await this.LoadPage();
        AudioManager.PlayOneShot(buttonClick, clickVolume);

        if (Settings.Page <= 0)
        {
            nextButton.gameObject.SetActive(false);
        }
        else
        {
            nextButton.gameObject.SetActive(true);
        }

        if (Settings.CountMessages < 100)
        {
            prevButton.gameObject.SetActive(false);
        }
        else
        {
            prevButton.gameObject.SetActive(true);
        }
    }

    private async void OnNextClicked()
    {
        --Settings.Page;
        bool f = await this.LoadPage();
        AudioManager.PlayOneShot(buttonClick, clickVolume);

        if (Settings.Page <= 0)
        {
            nextButton.gameObject.SetActive(false);
        }
        else
        {
            nextButton.gameObject.SetActive(true);
        }

        if (Settings.CountMessages < 100)
        {
            prevButton.gameObject.SetActive(false);
        }
        else
        {
            prevButton.gameObject.SetActive(true);
        }
    }

    private void Close()
    {
        Settings.Page = 0;
        Settings.CountMessages = 0;
        if (closeButton != null) closeButton.interactable = false;

        AudioManager.PlayOneShot(buttonClick, clickVolume);

        panelRigidbody2D.bodyType = RigidbodyType2D.Dynamic;
        panelRigidbody2D.gravityScale = gravityScale;  // Применяем гравитацию
        panelRigidbody2D.mass = mass;                  // Применяем массу

        // Добавляем силу в 2D пространстве
        Vector2 throwDirection = new Vector2(
            UnityEngine.Random.Range(-1000f, 1000f), // Случайное отклонение влево/вправо
            upForce                                 // Вверх
        ).normalized;

        panelRigidbody2D.AddForce(throwDirection * throwForce, ForceMode2D.Impulse);

        // Добавляем вращение по оси Z (это правильно для 2D)
        float torque = UnityEngine.Random.Range(-randomTorque, randomTorque);
        panelRigidbody2D.AddTorque(torque, ForceMode2D.Impulse);

        // Запускаем исчезновение
        StartCoroutine(FadePanelAndDestroy());
    }

    IEnumerator FadePanelAndDestroy()
    {
        yield return new WaitForSeconds(fadeDelay);

        float elapsedTime = 0f;
        float startAlpha = panelCanvasGroup.alpha;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            panelCanvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, elapsedTime / fadeDuration);
            yield return null;
        }

        panelCanvasGroup.alpha = 0f;

        yield return new WaitForSeconds(0.2f);

        // Уничтожаем Panel
        Destroy(panelToAnimate);

        // Уничтожаем Canvas
        Destroy(gameObject);
    }

    void OnDestroy()
    {
        if (closeButton != null)
            closeButton.onClick.RemoveListener(OnCloseClicked);
    }

    async Task<bool> LoadPage()
    {
        var formData = new List<KeyValuePair<string, string>>
        {
            new KeyValuePair<string, string>("pack[service]", "message"),
            new KeyValuePair<string, string>("pack[method]", "getPage"),
            new KeyValuePair<string, string>("pack[access_key]", Settings.AuthKey),
            new KeyValuePair<string, string>("pack[info][chat_id]", Settings.ChatSelected),
            new KeyValuePair<string, string>("pack[info][page]", Settings.Page.ToString())
        };

        try
        {
            prevButton.interactable = false;
            nextButton.interactable = false;

            Newtonsoft.Json.Linq.JObject result = await Sender.SendAndGet(formData);
            JArray UserArray = result["info"] as JArray;

            if (UserArray != null)
            {
                Settings.CountMessages = UserArray.Count;

                Transform contentTransform = historyList.content;
                if (contentTransform == null)
                {
                    Debug.LogError("ScrollView не имеет контейнера Content!");
                    return false;
                }
                
                foreach (Transform child in contentTransform)
                {
                    Destroy(child.gameObject);
                }

                GameObject messagePrefab = Resources.Load<GameObject>(prefabPath);
                if (messagePrefab == null)
                {
                    Debug.LogError($"Не удалось загрузить префаб по пути: {prefabPath}");
                    return false;
                }
                
                bool wasEnabled = historyList.enabled;
                historyList.enabled = false;

                foreach (JToken item in UserArray)
                {
                    if (item is JObject obj)
                    {
                        // Создаём экземпляр префаба
                       GameObject newMessageItem = Instantiate(messagePrefab, contentTransform);

                        MessageBubble message = newMessageItem.GetComponent<MessageBubble>();
                        message.Initialize(
                            item["id"]?.ToString() ?? "",
                            item["name"]?.ToString() ?? "",
                            item["message"]?.ToString() ?? "",
                            item["date"]?.ToString() ?? "",
                            item["qid"]?.ToString() ?? "",
                            item["qname"]?.ToString() ?? "",
                            item["qmessage"]?.ToString() ?? "",
                            (item["is_my"]?.ToString() ?? "") == "1" ? true : false,
                            item["aid"]?.ToString() ?? "",
                            item["aname"]?.ToString() ?? "",
                            item["asize"]?.ToString() ?? "",
                            item["apreview"]?.ToString() ?? "",
                            item["atype"]?.ToString() ?? "",
                            historyList,
                            messageInput,
                            statusBar,
                            quoteBarMain,
                            quoteLabel
                        );
                    }
                }

                historyList.enabled = wasEnabled;
                // Прокрутка до самого низа
                StartCoroutine(ScrollToBottomNextFrame());

            }

        }
        catch (Exception)
        {
        }
        finally
        {
            prevButton.interactable = true;
            nextButton.interactable = true;
        }

        return true;
    }

    IEnumerator ScrollToBottomNextFrame()
    {
        yield return new WaitForSeconds(0.1f);
        historyList.verticalNormalizedPosition = 0f;
    }
}

