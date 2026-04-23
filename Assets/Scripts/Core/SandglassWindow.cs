using UnityEngine;
using UnityEngine.UI;

public class SandglassWindow : MonoBehaviour
{
    [Header("Основное изображение")]
    [SerializeField] private Image targetImage; // Картинка, которую будем менять

    [Header("Список изображений")]
    [SerializeField] private Sprite[] imageSprites; // Массив из 5 картинок

    private int currentIndex = 0; // Индекс текущей картинки

    private void Start()
    {
        // Проверка на наличие всех необходимых компонентов
        if (targetImage == null)
        {
            Debug.LogError("Целевое изображение не назначено!");
        }

        if (imageSprites.Length != 5)
        {
            Debug.LogWarning($"Должно быть 5 картинок, а загружено {imageSprites.Length}");
        }

        // Устанавливаем первую картинку при старте, если массив не пуст
        if (imageSprites.Length > 0 && targetImage != null)
        {
            targetImage.sprite = imageSprites[0];
            currentIndex = 0;
        }
    }

    // Метод 1: Меняет картинку на следующую (циклично)
    public void ChangeToNextImage()
    {
        if (imageSprites.Length == 0 || targetImage == null) return;

        currentIndex = (currentIndex + 1) % imageSprites.Length;
        targetImage.sprite = imageSprites[currentIndex];
        Debug.Log($"Изменено на картинку {currentIndex + 1}");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ThisDestroy()
    {
        Destroy(gameObject, 0.2f);
    }
}
