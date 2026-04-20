using System.Collections;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
public class FixTMPDeleteBug : MonoBehaviour
{
    public TMP_InputField input;

    public string lastText = "";
    public int lastCaret = 0;
    public int lastSelectionStart = 0;
    public int lastSelectionEnd = 0;
    public bool isUpdating = false;

    void OnEnable()
    {
        if (input == null) input = GetComponent<TMP_InputField>();
        if (input != null)
        {
            input.onValueChanged.AddListener(OnValueChanged);
            SaveState();
        }
    }

    void OnDisable()
    {
        if (input != null) input.onValueChanged.RemoveListener(OnValueChanged);
    }

    void SaveState()
    {
        lastText = input.text;
        lastCaret = input.caretPosition;
        lastSelectionStart = input.selectionStringAnchorPosition;
        lastSelectionEnd = input.selectionStringFocusPosition;
    }

    void OnValueChanged(string newText)
    {
        if (isUpdating) return;

        bool hadSelection = lastSelectionStart != lastSelectionEnd;
        int deletedCount = lastText.Length - newText.Length;

        // Баг: удалилось слишком много символов
        if (deletedCount > 1 || (hadSelection && newText.Length == 0 && lastText.Length > 0))
        {
            isUpdating = true;

            if (hadSelection)
            {
                // Удаляем только выделенный текст
                int start = Mathf.Min(lastSelectionStart, lastSelectionEnd);
                int length = Mathf.Abs(lastSelectionEnd - lastSelectionStart);

                if (start >= 0 && length > 0 && start + length <= lastText.Length)
                {
                    input.text = lastText.Remove(start, length);
                    input.caretPosition = start;
                }
            }
            else
            {
                // Удаляем 1 символ перед кареткой
                if (lastCaret > 0 && lastText.Length > 0)
                {
                    int deleteIndex = lastCaret - 1;
                    // Пропускаем суррогатные пары для эмодзи
                    while (deleteIndex > 0 && char.IsLowSurrogate(lastText[deleteIndex]))
                    {
                        deleteIndex--;
                    }

                    input.text = lastText.Remove(deleteIndex, lastCaret - deleteIndex);
                    input.caretPosition = deleteIndex;
                }
            }

            // Сбрасываем выделение
            input.selectionStringAnchorPosition = input.caretPosition;
            input.selectionStringFocusPosition = input.caretPosition;

            SaveState();
            isUpdating = false;
        }
        else
        {
            SaveState();
        }
    }
}
