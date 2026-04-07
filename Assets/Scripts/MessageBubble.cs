using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering.Universal;

public class MessageBubble : MonoBehaviour
{
    public string id;

    [Header("UI Elements")]
    [SerializeField] private TMP_Text username;
    [SerializeField] private TMP_Text message;
    [SerializeField] private TMP_Text time;
    [SerializeField] private GameObject panel;

    [Header("Canvas")]
    public Canvas targetCanvas; // Ссылка на Canvas, куда создавать звездочки

    [Header("Настройки частиц")]
    public GameObject starParticlePrefab; // Префаб звездочки (должен иметь Image или SpriteRenderer)
    public int starCount = 15;            // Количество звездочек

    [Header("Настройки падения")]
    public float destroyAfter = 3f;       // Через сколько секунд удалить звездочки


    public void Initialize(string id, string username, string message, string time, bool my_message)
    {
        this.id = id;
        this.username.text = username;
        this.message.text = message;
        this.time.text = time;


        if (my_message)
        {
            VerticalLayoutGroup layoutGroup = GetComponent<VerticalLayoutGroup>();
            if (layoutGroup != null)
            {
                layoutGroup.padding.left = 600;
                layoutGroup.padding.right = 20;
            }

            Image img = panel.GetComponent<Image>();
            if (img != null)
            {
                img.color = new Color32(128, 166, 255, 180);
            }

            this.username.enabled = false;
        }
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

    void Update()
    {
    }

    public void Ignite()
    {
        if (targetCanvas == null)
        {
            GameObject canvasObj = GameObject.Find("MainCanvas");

            if (canvasObj == null)
            {
                Debug.LogError("Canvas не найден!");
                return;
            }
            else
            {
               targetCanvas = canvasObj.GetComponent<Canvas>();
            }
        }

        // Вспышка света

        // Выброс звездочек
        if (starParticlePrefab == null)
        {
            Debug.LogError("STAR PREFAB = NULL! Префаб звездочки не назначен!");
            return;
        }

        if (targetCanvas == null)
        {
            Debug.LogError("TARGET CANVAS = NULL!");
            return;
        }

        for (int i = 0; i < starCount; i++)
        {
            // Создаем звездочку как дочерний объект Canvas
            GameObject star = Instantiate(starParticlePrefab, targetCanvas.transform);

            // Позиция как у текущего сообщения
            star.transform.position = transform.position;

            // Случайное смещение
            Vector3 randomOffset = new Vector3(
                UnityEngine.Random.Range(-100f, 100f),
                UnityEngine.Random.Range(-50f, 150f),
                0
            );
            star.transform.position += randomOffset;

            // поднимаем звездочку на передний план
            RectTransform starRect = star.GetComponent<RectTransform>();
            if (starRect != null)
            {
                // Делаем звездочку последним дочерним объектом (рисуется поверх всех)
                starRect.SetAsLastSibling();
            }

            // Добавляем UI анимацию вместо Rigidbody
            StartCoroutine(AnimateUIStar(star));

            Destroy(star, destroyAfter);
        }
    }

    private IEnumerator AnimateUIStar(GameObject star)
    {
        RectTransform rect = star.GetComponent<RectTransform>();
        if (rect == null) yield break;

        Vector2 velocity = new Vector2(
            UnityEngine.Random.Range(-200f, 200f),
            UnityEngine.Random.Range(200f, 400f)
        );

        float gravity = 600f;
        float elapsed = 0f;

        while (elapsed < destroyAfter && star != null)
        {
            elapsed += Time.deltaTime;
            velocity.y -= gravity * Time.deltaTime;
            rect.anchoredPosition += velocity * Time.deltaTime;
            rect.Rotate(0, 0, 360f * Time.deltaTime);

            // Затухание
            Image img = star.GetComponent<Image>();
            if (img != null)
            {
                Color c = img.color;
                c.a = Mathf.Lerp(1f, 0f, elapsed / destroyAfter);
                img.color = c;
            }

            yield return null;
        }
    }

    public IEnumerator IgniteWithDelay()
    {
        yield return new WaitForSeconds(0.2f);
        this.Ignite();
    }
}
