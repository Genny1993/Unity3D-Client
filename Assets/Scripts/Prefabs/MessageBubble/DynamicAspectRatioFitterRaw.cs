using UnityEngine;
using UnityEngine.UI;

public class DynamicAspectRatioFitterRaw : MonoBehaviour
{
    private RawImage rawImage;
    private AspectRatioFitter aspectFitter;

    private void Awake()
    {
        rawImage = GetComponent<RawImage>();
        aspectFitter = GetComponent<AspectRatioFitter>();

        // Убедимся, что режим именно Width Controls Height
        if (aspectFitter != null)
        {
            aspectFitter.aspectMode = AspectRatioFitter.AspectMode.WidthControlsHeight;
        }
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
        // Проверяем, не изменилась ли текстура
        if (rawImage.texture != null)
        {
            float currentAspect = GetTextureAspectRatio();
            if (Mathf.Abs(aspectFitter.aspectRatio - currentAspect) > 0.001f)
            {
                UpdateAspectRatio();
            }
        }
    }

    public void UpdateAspectRatio()
    {
        if (rawImage.texture == null)
        {
            Debug.LogWarning("Нет текстуры на RawImage компоненте", this);
            return;
        }

        float aspectRatio = GetTextureAspectRatio();
        if (aspectFitter != null)
        {
            aspectFitter.aspectRatio = aspectRatio;
        }
    }

    private float GetTextureAspectRatio()
    {
        if (rawImage.texture == null) return 1f;

        return (float)rawImage.texture.width / rawImage.texture.height;
    }

    // Для ручного вызова (например, при смене картинки из кода)
    public void ForceUpdate()
    {
        UpdateAspectRatio();
    }
}