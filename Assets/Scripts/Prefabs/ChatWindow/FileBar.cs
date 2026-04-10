using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class FilePanel : MonoBehaviour
{
    [Header("Ссылки на UI")]
    [SerializeField] private GameObject fileBar;
    [SerializeField] private GameObject statusBar;
    [SerializeField] private ScrollRect messagesList;
    [SerializeField] private Button deleteFile;
    [SerializeField] private TMP_Text fileLabel;
    [SerializeField] private TMP_InputField messageInput;

    [Header("Аудио SFX")]
    [SerializeField] private AudioClip buttonClick;

    [Header("Громкость")]
    [Range(0f, 1f)]
    [SerializeField] private float clickVolume = 0.7f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (deleteFile != null)
           deleteFile.onClick.AddListener(OnDeleteFileButtonClick);
    }

    void OnDeleteFileButtonClick()
    {
        AudioManager.PlayOneShot(buttonClick, clickVolume);

        Settings.fileBar = false;
        FileInfo.Clear();
        fileLabel.text = "";
        fileBar.SetActive(false);

        if (Settings.quoteBar == false && Settings.fileBar == false)
        {
            statusBar.SetActive(false);

            RectTransform rect = messagesList.GetComponent<RectTransform>();
            Vector2 size = rect.sizeDelta;
            size.y = Settings.currentMessagesListHeight + 40;
            Settings.currentMessagesListHeight = (int)size.y;
            rect.sizeDelta = size;
        }

        messageInput.ActivateInputField();
        messageInput.caretPosition = Settings.lastCaretPosition;
        messageInput.selectionFocusPosition = messageInput.caretPosition;
    }
}
