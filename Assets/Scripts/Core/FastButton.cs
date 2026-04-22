using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class FastButton : Button
{
    public override void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            onClick.Invoke();
        }

        base.OnPointerDown(eventData);
    }

    public override void OnPointerClick(PointerEventData eventData)
    {
        // отключаем стандартный клик, чтобы не было двойного вызова
    }
}