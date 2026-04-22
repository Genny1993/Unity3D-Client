using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

public class InputFieldGlobalFix : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Init()
    {
        var go = new GameObject("~InputFieldGlobalFix");
        DontDestroyOnLoad(go);
        go.AddComponent<InputFieldGlobalFix>();
    }

    void OnEnable()
    {
        InputSystem.onEvent += OnInputEvent;
    }

    void OnDisable()
    {
        InputSystem.onEvent -= OnInputEvent;
    }

    private void OnInputEvent(InputEventPtr eventPtr, InputDevice device)
    {
        // Нас интересуют только тапы / клики
        if (!(device is Pointer))
            return;

        if (!eventPtr.IsA<StateEvent>() && !eventPtr.IsA<DeltaStateEvent>())
            return;

        var pointer = device as Pointer;

        if (pointer == null || !pointer.press.isPressed)
            return;

        HandleTouch(pointer.position.ReadValue());
    }

    void HandleTouch(Vector2 position)
    {
        if (EventSystem.current == null)
            return;

        var currentSelected = EventSystem.current.currentSelectedGameObject;

        if (currentSelected == null)
            return;

        var inputField = currentSelected.GetComponent<TMP_InputField>();

        if (inputField == null)
            return;

        PointerEventData pointerData = new PointerEventData(EventSystem.current);
        pointerData.position = position;

        var results = new System.Collections.Generic.List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        bool clickedOnSameInput = false;

        foreach (var r in results)
        {
            if (r.gameObject == currentSelected)
            {
                clickedOnSameInput = true;
                break;
            }
        }

        if (!clickedOnSameInput)
        {
            inputField.DeactivateInputField();
            EventSystem.current.SetSelectedGameObject(null);
        }
    }
}