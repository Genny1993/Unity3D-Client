using UnityEngine;
using TMPro;

public class FixedPixelSizeText : MonoBehaviour
{
    [Header("Настройки")]
    [Tooltip("Размер текста в пикселях")]
    public float desiredPixelSize = 10f;

    [Tooltip("Эталонное разрешение")]
    public Vector2 referenceResolution = new Vector2(1920f, 1080f);

    private TMP_Text textComponent;
    private float lastScreenWidth;
    private float lastScreenHeight;
    private bool isInitialized = false;

    void Awake()
    {
        textComponent = GetComponent<TMP_Text>();

        if (textComponent == null)
        {
            Debug.LogError($"Скрипт {GetType().Name} требует компонент TMP_Text на объекте {gameObject.name}");
            enabled = false;
            return;
        }
    }

    void OnEnable()
    {
        ApplyFixedPixelSize();
        isInitialized = true;
    }

    void Start()
    {
        if (!isInitialized)
        {
            ApplyFixedPixelSize();
            isInitialized = true;
        }
    }

    void Update()
    {
        if (Mathf.Approximately(lastScreenWidth, Screen.width) &&
            Mathf.Approximately(lastScreenHeight, Screen.height))
            return;

        lastScreenWidth = Screen.width;
        lastScreenHeight = Screen.height;

        ApplyFixedPixelSize();
    }

    void ApplyFixedPixelSize()
    {
        if (textComponent == null) return;

        float currentWindowScale = (float)Screen.width / referenceResolution.x;

        // Защита от деления на ноль
        if (currentWindowScale < 0.01f) currentWindowScale = 0.01f;

        float compensatedFontSize = desiredPixelSize / currentWindowScale;

        // Ограничение
        compensatedFontSize = Mathf.Clamp(compensatedFontSize, 10f, 100f);

        textComponent.fontSize = compensatedFontSize;
    }

    // ручное обновление
    public void ForceUpdate()
    {
        ApplyFixedPixelSize();
    }
}