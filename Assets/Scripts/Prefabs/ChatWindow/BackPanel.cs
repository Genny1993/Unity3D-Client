using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

public class BackPanel : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Button chatButton;
    [SerializeField] private Button userButton;
    [SerializeField] private GameObject chatList;
    [SerializeField] private GameObject userList;
    [SerializeField] private TMP_InputField messageInput;

    [Header("Аудио SFX")]
    [SerializeField] private AudioClip buttonClick;

    [Header("Громкость")]
    [Range(0f, 1f)]
    [SerializeField] private float clickVolume = 0.7f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (chatButton != null)
        {
            chatButton.onClick.AddListener(OnChatClicked);
        }

        if (userButton != null)
        {
            userButton.onClick.AddListener(OnUserClicked);
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnChatClicked()
    {
        AudioManager.PlayOneShot(buttonClick, clickVolume);
        chatList.SetActive(true);
        userList.SetActive(false);

        messageInput.ActivateInputField();
        messageInput.caretPosition = Settings.lastCaretPosition;
        messageInput.selectionFocusPosition = messageInput.caretPosition;
    }

    void OnUserClicked()
    {
        AudioManager.PlayOneShot(buttonClick, clickVolume);
        chatList.SetActive(false);
        userList.SetActive(true);

        messageInput.ActivateInputField();
        messageInput.caretPosition = Settings.lastCaretPosition;
        messageInput.selectionFocusPosition = messageInput.caretPosition;
    }
}
