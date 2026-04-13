using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AdminPanelButton : MonoBehaviour
{
    private bool switched = false;
    [SerializeField] private Button button;
    [SerializeField] private TMP_Text buttonText;

    [Header("Аудио SFX")]
    [SerializeField] private AudioClip buttonClick;

    [Header("Громкость")]
    [Range(0f, 1f)]
    [SerializeField] private float clickVolume = 0.7f;

    void Start()
    {
        button.onClick.AddListener(Switch);
        switched = false;
        buttonText.text = "👑";
    }
    public void Switch()
    {
        AudioManager.PlayOneShot(buttonClick, clickVolume);

        if (switched)
        {
            switched = false;
            buttonText.text = "👑";

            UIManager.HideAdminWindow();
            UIManager.ShowChatWindow();
        } else
        {
            switched = true;
            buttonText.text = "💬";

            UIManager.HideChatWindow();
            UIManager.ShowAdminWindow();
        }
    }
}
