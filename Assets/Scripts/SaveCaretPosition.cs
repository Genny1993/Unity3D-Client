using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;

public class SaveCaretPosition : MonoBehaviour, IDeselectHandler
{
    [Header("UI Elements")]
    [SerializeField] private TMP_InputField messageInput;

    public void OnDeselect(BaseEventData eventData)
    {
        Settings.lastCaretPosition = messageInput.caretPosition;
    }
}
