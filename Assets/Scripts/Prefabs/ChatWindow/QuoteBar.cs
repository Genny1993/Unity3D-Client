using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class QuotePanel : MonoBehaviour
{
    [Header("Ссылки на UI")]
    [SerializeField] private GameObject quoteBar;
    [SerializeField] private GameObject statusBar;
    [SerializeField] private ScrollRect messagesList;
    [SerializeField] private Button deleteQuote;
    [SerializeField] private TMP_Text quoteLabel;
    [SerializeField] private TMP_InputField messageInput;

    [Header("Аудио SFX")]
    [SerializeField] private AudioClip buttonClick;

    [Header("Громкость")]
    [Range(0f, 1f)]
    [SerializeField] private float clickVolume = 0.7f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (deleteQuote != null)
           deleteQuote.onClick.AddListener(OnDeleteQuoteButtonClick);
    }

    void OnDeleteQuoteButtonClick()
    {

        AudioManager.PlayOneShot(buttonClick, clickVolume);

        Settings.QuotedId = "";
        quoteLabel.text = "";
        quoteBar.SetActive(false);

        Settings.quoteBar = false;

        if(Settings.quoteBar == false && Settings.fileBar == false)
        {
            statusBar.SetActive(false);

            RectTransform rect = messagesList.GetComponent<RectTransform>();
            Vector2 size = rect.sizeDelta;
            size.y = Settings.currentMessagesListHeight + 40;
            Settings.currentMessagesListHeight = (int)size.y;
            rect.sizeDelta = size;
        }

        if (Settings.isPCProgram)
        {
            messageInput.ActivateInputField();
            messageInput.caretPosition = Settings.lastCaretPosition;
            messageInput.selectionFocusPosition = messageInput.caretPosition;
        }
    }
}
