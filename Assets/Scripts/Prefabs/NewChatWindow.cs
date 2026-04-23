using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NewChatWindow : MonoBehaviour
{
    private string id;

    [Header("UI Elements")]
    [SerializeField] private FastButton createButton;
    [SerializeField] private FastButton closeButton;
    [SerializeField] private FastButton cancelButton;

    [SerializeField] public TMP_InputField nameInput;

    [SerializeField] private GameObject adminPanel;

    [Header("Ссылка на окошко")]
    [SerializeField] private GameObject panelToAnimate;

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

        if (createButton != null)
            createButton.onClick.AddListener(OnCreateClicked);

        if (closeButton != null)
            closeButton.onClick.AddListener(OnCloseClicked);

        if (cancelButton != null)
            cancelButton.onClick.AddListener(OnCloseClicked);

    }

    public void Initialize(GameObject admin_panel)
    {
        this.adminPanel = admin_panel;
    }

    async void OnCreateClicked()
    {
        AudioManager.PlayOneShot(buttonClick, clickVolume);
        createButton.interactable = false;

        var formData = new List<KeyValuePair<string, string>>
        {
            new KeyValuePair<string, string>("pack[service]", "chat"),
            new KeyValuePair<string, string>("pack[method]", "create"),
            new KeyValuePair<string, string>("pack[access_key]", Settings.AuthKey),
            new KeyValuePair<string, string>("pack[info][name]", nameInput.text.Trim()),
        };

        try
        {
            JObject result = await Sender.SendAndGet(formData);
            AdminWindow adminPanel = this.adminPanel.GetComponent<AdminWindow>();
            if(adminPanel != null)
            {
                await adminPanel.GetChatList();
            }
            this.Close();
        }
        catch (Exception)
        {
        }
        finally
        {
            createButton.interactable = true;
        }
    }

    private void OnCloseClicked()
    {
        Close();
    }

    private void Close()
    {
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
        if (cancelButton != null)
            cancelButton.onClick.RemoveListener(OnCloseClicked);

        if (closeButton != null)
            closeButton.onClick.RemoveListener(OnCloseClicked);

        if (createButton != null)
            createButton.onClick.RemoveListener(OnCreateClicked);
    }
}
