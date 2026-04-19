using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InputFieldScrollFix : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    private TMP_InputField inputField;
    private ScrollRect parentScrollRect;

    void Start()
    {
        inputField = GetComponent<TMP_InputField>();

        // Ищем ScrollRect на родителях
        Transform parent = transform.parent;
        while (parent != null && parentScrollRect == null)
        {
            parentScrollRect = parent.GetComponent<ScrollRect>();
            parent = parent.parent;
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!inputField.interactable && parentScrollRect != null)
        {
            ExecuteEvents.Execute(parentScrollRect.gameObject, eventData, ExecuteEvents.beginDragHandler);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!inputField.interactable && parentScrollRect != null)
        {
            ExecuteEvents.Execute(parentScrollRect.gameObject, eventData, ExecuteEvents.dragHandler);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!inputField.interactable && parentScrollRect != null)
        {
            ExecuteEvents.Execute(parentScrollRect.gameObject, eventData, ExecuteEvents.endDragHandler);
        }
    }
}