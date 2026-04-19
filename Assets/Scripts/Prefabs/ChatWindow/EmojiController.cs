using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;

public class EmojiController : MonoBehaviour
{
    [SerializeField] private GameObject targetPanel;
    [SerializeField] private TMP_InputField messageInput;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Если панель не назначена в инспекторе, пробуем найти её на текущем объекте
        if (targetPanel == null)
            targetPanel = gameObject;

        // Получаем все кнопки на панели и подписываем их
        SubscribeAllButtons();
    }

    private void SubscribeAllButtons()
    {
        // Получаем все компоненты Button на панели (включая вложенные)
        UnityEngine.UI.Button[] allButtons = targetPanel.GetComponentsInChildren<UnityEngine.UI.Button>();

        // Проходим по всем кнопкам и подписываем их на один метод
        foreach (UnityEngine.UI.Button button in allButtons)
        {
            // Убираем старые слушатели, чтобы не было дублирования
            button.onClick.RemoveAllListeners();

            // Получаем имя кнопки в переменную
            string buttonName = button.GetComponentInChildren<TMP_Text>()?.text ?? "";
            // Подписываем кнопку на метод с передачей её самой и имени
            button.onClick.AddListener(() => OnAnyButtonClick(button, buttonName));
        }
    }

    private void OnAnyButtonClick(UnityEngine.UI.Button clickedButton, string buttonName)
    {


        // Получаем позицию каретки
        int pos = messageInput.stringPosition;
        Settings.lastCaretPosition = messageInput.caretPosition;

        // Получаем текущий текст
        string currentText = messageInput.text;

        // Вставляем эмодзи в позицию
        messageInput.text = messageInput.text.Insert(pos, buttonName);

        // Перемещаемся на длину эмодзи
        messageInput.stringPosition = pos + buttonName.Length;

        if (Settings.isPCProgram)
        {
            messageInput.ActivateInputField();
            messageInput.caretPosition = Settings.lastCaretPosition;
            messageInput.selectionFocusPosition = messageInput.caretPosition;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
