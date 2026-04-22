using UnityEngine;
using UnityEngine.UI;
public class DynamicAspectRatioFitter : MonoBehaviour
{
    private Image image;
    private AspectRatioFitter aspectFitter;

    private void Awake()
    {
        image = GetComponent<Image>();
        aspectFitter = GetComponent<AspectRatioFitter>();

        // Убедимся, что режим именно Width Controls Height
        aspectFitter.aspectMode = AspectRatioFitter.AspectMode.WidthControlsHeight;
    }

    private void OnEnable()
    {
        UpdateAspectRatio();
    }

    private void Start()
    {
        UpdateAspectRatio();
    }

    private void Update()
    {
        // Опционально: проверяем, не изменился ли спрайт
        if (image.sprite != null)
        {
            float currentAspect = GetSpriteAspectRatio();
            if (Mathf.Abs(aspectFitter.aspectRatio - currentAspect) > 0.001f)
            {
                UpdateAspectRatio();
            }
        }
    }

    public void UpdateAspectRatio()
    {
        if (image.sprite == null)
        {
            Debug.LogWarning("Нет спрайта на Image компоненте", this);
            return;
        }

        float aspectRatio = GetSpriteAspectRatio();
        aspectFitter.aspectRatio = aspectRatio;
    }

    private float GetSpriteAspectRatio()
    {
        if (image.sprite == null) return 1f;

        Rect rect = image.sprite.rect;
        return rect.width / rect.height;
    }

    // Для ручного вызова (например, при смене картинки из кода)
    public void ForceUpdate()
    {
        UpdateAspectRatio();
    }
}