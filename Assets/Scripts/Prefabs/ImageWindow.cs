using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
public class ImageWindow : MonoBehaviour
{
    private Action onCloseCallback;
    private int count_messages;
    private string aid = "";

    [Header("UI Elements")]
    [SerializeField] private Button closeButton;
    [SerializeField] private Image image;
    [SerializeField] private ScrollRect Scroll;


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

        if (closeButton != null)
        {
            closeButton.onClick.AddListener(OnCloseClicked);
        }

    }

    public async void Initialize(string aid, Action onClose = null)
    {
        this.aid = aid;
        bool f = await this.LoadImage();

    }

    private void OnCloseClicked()
    {
        Close();
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

    async Task<bool> LoadImage()
    {
        var formData = new List<KeyValuePair<string, string>>
        {
            new KeyValuePair<string, string>("pack[service]", "file"),
            new KeyValuePair<string, string>("pack[method]", "download"),
            new KeyValuePair<string, string>("pack[access_key]", Settings.AuthKey),
            new KeyValuePair<string, string>("pack[info][id]", this.aid)
        };

        using (HttpClient client = new HttpClient())
        {
            // Отправляем POST запрос с form-urlencoded данными
            string json = JsonConverter.To(formData);
            json = Crypt.Encrypt(json);
            var content = new StringContent(json, Encoding.UTF8, "text/plain");

            using (var response = await client.PostAsync(Settings.Url, content))
            {
                response.EnsureSuccessStatusCode();

                // Получаем файл как массив байтов
                byte[] fileBytes = await response.Content.ReadAsByteArrayAsync();
                fileBytes = Crypt.DecryptBytes(Encoding.UTF8.GetString(fileBytes));

                Texture2D originalTexture = new Texture2D(2, 2);
                if (!originalTexture.LoadImage(fileBytes))
                {
                    // Ошибка загрузки
                    return false;
                }

                Texture2D finalTexture = originalTexture;

                // Если ширина больше высоты — поворачиваем
                if (originalTexture.width > originalTexture.height)
                {
                    finalTexture = RotateTexture90Clockwise(originalTexture);
                }

                // Конвертируем в Sprite
                Sprite sprite = Sprite.Create(
                    finalTexture,
                    new Rect(0, 0, finalTexture.width, finalTexture.height),
                    new Vector2(0.5f, 0.5f)
                );
                this.image.sprite = sprite;

                // Не забываем уничтожить временные текстуры, если они больше не нужны
                if (finalTexture != originalTexture)
                {
                    Destroy(originalTexture);
                }

                Scroll.horizontalNormalizedPosition = 1f;
            }
        }

        return true;
    }

    private Texture2D RotateTexture90Clockwise(Texture2D src)
{
    int width = src.height;  // После поворота ширина = исходная высота
    int height = src.width;  // После поворота высота = исходная ширина

    Texture2D rotated = new Texture2D(width, height, src.format, false);
    Color[] srcPixels = src.GetPixels();
    Color[] dstPixels = new Color[width * height];

    for (int y = 0; y < height; y++)
    {
        for (int x = 0; x < width; x++)
        {
            // Поворот на 90 градусов по часовой стрелке:
            // новый_x = y
            // новый_y = ширина - 1 - x
            int srcX = height - 1 - y;
            int srcY = x;
            dstPixels[y * width + x] = srcPixels[srcY * src.width + srcX];
        }
    }

    rotated.SetPixels(dstPixels);
    rotated.Apply();
    return rotated;
}
}

