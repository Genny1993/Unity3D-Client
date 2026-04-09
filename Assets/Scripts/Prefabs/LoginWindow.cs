using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoginWindow : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TMP_InputField ServerInput;
    [SerializeField] private TMP_InputField LoginInput;
    [SerializeField] private TMP_InputField PasswordInput;
    [SerializeField] private Button LoginButton;
    [SerializeField] private Button RegisterButton;

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

        LoadDataFromJson();


    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private async void OnLoginButtonClick()
    {
        try
        {
            AudioManager.PlayOneShot(buttonClick, clickVolume);
            // Создаем объект с данными
            var data = new FormData
            {
                Url = ServerInput.text,
                Login = LoginInput.text,
                Password = PasswordInput.text
            };

            // Сериализуем в JSON и сохраняем
            string jsonData = JsonConvert.SerializeObject(data, Formatting.Indented);
            jsonData = Crypt.Encrypt(jsonData);
            FileManager.WriteSettings(jsonData);
        }
        catch (Exception ex)
        {
            MessageBox.Show("Ошибка", $"Ошибка при сохранении настроек: {ex.Message}");
        }

        if (LoginInput.text.Trim() == "" ||
                PasswordInput.text == "" ||
                ServerInput.text.Trim() == "")
        {
            MessageBox.Show("Ошибка", "Пожалуйста, заполните все поля!");
            return;
        }
        
        var formData = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("pack[service]", "account"),
                new KeyValuePair<string, string>("pack[method]", "login"),
                new KeyValuePair<string, string>("pack[access_key]", ""),
                new KeyValuePair<string, string>("pack[info][login]", LoginInput.text.Trim()),
                new KeyValuePair<string, string>("pack[info][password]", PasswordInput.text)
            };

        try
        {
            LoginButton.interactable = false;
            Settings.Url = ServerInput.text.Trim();
            Newtonsoft.Json.Linq.JObject result = await Sender.SendAndGet(formData);
            Settings.AuthKey = result["info"]?.ToString() ?? "";

            //Открытие окна чата
            Close(UIManager.ShowChatWindow);
        }
        catch (Exception) { }
        finally { LoginButton.interactable = true; }
    }

    private void OnRegisterButtonClick()
    {
        AudioManager.PlayOneShot(buttonClick, clickVolume);
        if (string.IsNullOrWhiteSpace(ServerInput.text))
        {
            MessageBox.Show("Ошибка", "Пожалуйста, заполните URL перед регистрацией");
            return;
        }
        Settings.Url = ServerInput.text;
        Close(UIManager.ShowRegisterWindow);
    }

    private void LoadDataFromJson()
    {
        try
        {
            string jsonData = FileManager.ReadSettings();
            jsonData = Crypt.Decrypt(jsonData);
            var formData = JsonConvert.DeserializeObject<FormData>(jsonData);

            if (formData != null)
            {
                ServerInput.text = formData.Url;
                LoginInput.text = formData.Login;
                PasswordInput.text = formData.Password;
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show("Ошибка", $"Ошибка при загрузке настроек: {ex.Message}");
        }
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

    public class FormData
    {
        public string Url { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
    }
}
