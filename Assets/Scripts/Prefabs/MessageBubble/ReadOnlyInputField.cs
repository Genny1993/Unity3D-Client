using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Компонент, делающий TMP_InputField доступным только для чтения.
/// Текст можно выделять и копировать, но нельзя редактировать или удалять.
/// Прокрутка передается родительскому ScrollRect.
/// </summary>
public class ReadOnlyInputField : MonoBehaviour, IPointerClickHandler, IScrollHandler
{
    public TMP_InputField inputField;
    private string currentText;
    private bool isInitialized = false;
    private ScrollRect parentScrollRect;

    void Awake()
    {
        // Если переменная не назначена в инспекторе, пробуем найти компонент на этом же объекте
        if (inputField == null)
        {
            inputField = GetComponent<TMP_InputField>();
            if (inputField == null)
            {
                Debug.LogError("ReadOnlyInputField: TMP_InputField не найден! Пожалуйста, назначьте его в инспекторе.", this);
                return;
            }
        }

        // Находим родительский ScrollRect
        parentScrollRect = GetComponentInParent<ScrollRect>();
    }

    void Start()
    {
        // Сохраняем начальный текст ПОСЛЕ того, как поле полностью инициализировано
        currentText = inputField.text;
        isInitialized = true;

        // Убеждаемся, что текст отображается корректно
        inputField.text = currentText;
    }

    void OnEnable()
    {
        if (inputField == null) return;

        // Отписываемся сначала, чтобы избежать двойной подписки
        OnDisable();

        // Подписываемся на события ввода, чтобы блокировать изменения
        inputField.onValidateInput += ValidateInput;
        // Блокируем возможность изменения текста через другие методы
        inputField.onValueChanged.AddListener(OnValueChanged);
    }

    void OnDisable()
    {
        if (inputField == null) return;

        // Отписываемся, чтобы избежать ошибок
        inputField.onValidateInput -= ValidateInput;
        inputField.onValueChanged.RemoveListener(OnValueChanged);
    }

    // Этот метод вызывается при каждой попытке ввести символ
    private char ValidateInput(string text, int charIndex, char addedChar)
    {
        // Запрещаем ввод любого символа
        return '\0';
    }

    // Этот метод следит за изменением текста и откатывает его назад
    private void OnValueChanged(string newText)
    {
        // Игнорируем изменения до полной инициализации
        if (!isInitialized) return;

        // Если текст изменился, возвращаем сохраненный
        if (inputField.text != currentText)
        {
            inputField.text = currentText;
        }
    }

    // Обработка прокрутки колесиком мыши
    public void OnScroll(PointerEventData eventData)
    {
        // Если есть родительский ScrollRect, передаем ему событие прокрутки
        if (parentScrollRect != null)
        {
            // Отправляем событие прокрутки родителю
            ExecuteEvents.Execute(parentScrollRect.gameObject, eventData, ExecuteEvents.scrollHandler);
        }
    }

    // Обработка кликов (оставляем для выделения текста)
    public void OnPointerClick(PointerEventData eventData)
    {
        // Ничего не блокируем, чтобы выделение работало
        // При правом клике можно было бы добавить кастомное меню с Copy
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            // Здесь можно добавить кастомное контекстное меню с опцией Copy
            // Но стандартное меню не появляется, так что оставляем пустым
        }
    }

    // Обновляем сохраненный текст (можно вызвать извне, если нужно программно изменить текст)
    public void UpdateStoredText()
    {
        if (inputField != null)
        {
            currentText = inputField.text;
        }
    }

    // Метод для программной установки текста
    public void SetText(string newText)
    {
        if (inputField != null)
        {
            currentText = newText;
            inputField.text = newText;
        }
    }
}