using TMPro;
using UnityEngine;

public class FixTMPDeleteBug : MonoBehaviour
{
    public TMP_InputField input;

    public string lastText = "";
    public int lastCaret = 0;

    void Update()
    {
        if (!input.isFocused) return;

        // Если вдруг удалилось слишком много
        if (lastText.Length - input.text.Length > 1)
        {
            input.text = lastText.Remove(lastCaret - 1, 1);
            input.caretPosition = lastCaret - 1;
        }

        lastText = input.text;
        lastCaret = input.caretPosition;
    }
}
