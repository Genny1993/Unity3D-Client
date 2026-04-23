using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RegisterWindow : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TMP_InputField LoginInput;
    [SerializeField] private TMP_InputField NameInput;
    [SerializeField] private TMP_InputField PasswordInput;
    [SerializeField] private TMP_InputField Password2Input;
    [SerializeField] private FastButton LoginButton;
    [SerializeField] private FastButton RegisterButton;

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

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (LoginButton != null)
            LoginButton.onClick.AddListener(OnLoginButtonClick);

        if (RegisterButton != null)
            RegisterButton.onClick.AddListener(OnRegisterButtonClick);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnLoginButtonClick()
    {
        AudioManager.PlayOneShot(buttonClick, clickVolume);
        Close(UIManager.ShowLoginWindow);
    }

    private async void OnRegisterButtonClick()
    {
        
        AudioManager.PlayOneShot(buttonClick, clickVolume);
        
        if (LoginInput.text.Trim() == "" ||
                NameInput.text.Trim() == "" ||
                PasswordInput.text == "" ||
                Password2Input.text == "")
        {
            MessageBox.Show("Ошибка", "Пожалуйста, заполните все поля!");
            return;
        }

        if (PasswordInput.text != Password2Input.text)
        {
            MessageBox.Show("Ошибка",  "Пароли не совпадают!");
            return;
        }

        var formData = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("pack[service]", "account"),
                new KeyValuePair<string, string>("pack[method]", "simpleReg"),
                new KeyValuePair<string, string>("pack[access_key]", ""),
                new KeyValuePair<string, string>("pack[info][login]", LoginInput.text.Trim()),
                new KeyValuePair<string, string>("pack[info][name]", NameInput.text.Trim()),
                new KeyValuePair<string, string>("pack[info][password]", PasswordInput.text)
            };

        try
        {
            RegisterButton.interactable = false;
            Newtonsoft.Json.Linq.JObject result = await Sender.SendAndGet(formData);
            MessageBox.Show("Информация", "Регистрация успешно завершена. Напишите администратору для активации вашей учетной записи");
            Close(UIManager.ShowLoginWindow);
        }
        catch (Exception) { }
        finally { RegisterButton.interactable = true; }
    }

    public void Nothing()
    {

    }

    private void Close(Action action)
    {

        // Отключаем кнопки
        if (LoginButton != null) LoginButton.interactable = false;
        if (RegisterButton != null) RegisterButton.interactable = false;

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
        StartCoroutine(FadePanelAndDestroy(action));
    }

    IEnumerator FadePanelAndDestroy(Action action)
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

        action?.Invoke();
        // Уничтожаем Panel
        Destroy(panelToAnimate);

    }

    void OnDestroy()
    {
        if (LoginButton != null)
            LoginButton.onClick.RemoveListener(OnLoginButtonClick);

        if (RegisterButton != null)
            RegisterButton.onClick.RemoveListener(OnRegisterButtonClick);
    }
}
