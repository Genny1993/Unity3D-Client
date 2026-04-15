using UnityEngine;

using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class FixedPixelSizeInputField : MonoBehaviour
{
    [Header("Настройки")]
    [Tooltip("размер текста в пикселях")]
    public float desiredPixelSize = 30f;

    [Tooltip("Эталонное разрешение")]
    public Vector2 referenceResolution = new Vector2(1920f, 1080f);

    private TMP_InputField inputField;
    private TMP_Text textComponent;
    private float lastScreenWidth;
    private float lastScreenHeight;
    private bool isInitialized = false;

    void Awake()
    {
        inputField = GetComponent<TMP_InputField>();

        if (inputField == null)
        {
            Debug.LogError($"Скрипт {GetType().Name} требует компонент TMP_InputField на объекте {gameObject.name}");
            enabled = false;
            return;
        }

        textComponent = inputField.textComponent;

        if (textComponent == null)
        {
            Debug.LogError($"TMP_InputField на объекте {gameObject.name} не имеет текстового компонента!");
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

        //Пределы
        compensatedFontSize = Mathf.Clamp(compensatedFontSize, 10f, 100f);

        textComponent.fontSize = compensatedFontSize;


        if (inputField.placeholder != null)
        {
            TMP_Text placeholderText = inputField.placeholder as TMP_Text;
            if (placeholderText != null)
            {
                placeholderText.fontSize = compensatedFontSize;
            }
        }
    }

    //Ручное обновления
    public void ForceUpdate()
    {
        ApplyFixedPixelSize();
    }
}