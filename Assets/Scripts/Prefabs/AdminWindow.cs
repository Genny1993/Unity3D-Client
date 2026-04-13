using UnityEngine;
using UnityEngine.UI;

public class AdminWindow : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Button addUserButton;
    [SerializeField] private Button refreshUserButton;
    [SerializeField] private Button addChatButton;
    [SerializeField] private Button refreshChatButton;

    [Header("Аудио SFX")]
    [SerializeField] private AudioClip buttonClick;

    [Header("Громкость")]
    [Range(0f, 1f)]
    [SerializeField] private float clickVolume = 0.7f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (addUserButton != null)
            addUserButton.onClick.AddListener(AddUserButtonClick);
        if (refreshUserButton != null)
            refreshUserButton.onClick.AddListener(RefreshUserButtonClick);
        if (addChatButton != null)
            addChatButton.onClick.AddListener(AddChatButtonClick);
        if (refreshChatButton != null)
            refreshChatButton.onClick.AddListener(RefreshChatButtonClick);
    }

    void AddUserButtonClick()
    {
        AudioManager.PlayOneShot(buttonClick, clickVolume);
    }

    void RefreshUserButtonClick()
    {
        AudioManager.PlayOneShot(buttonClick, clickVolume);
    }

    void AddChatButtonClick()
    {
        AudioManager.PlayOneShot(buttonClick, clickVolume);
    }

    void RefreshChatButtonClick()
    {
        AudioManager.PlayOneShot(buttonClick, clickVolume);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Close()
    {
        Destroy(gameObject);
    }
}
