using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ChatSwitcher : MonoBehaviour
{
    private bool switched = false;
    [SerializeField] private FastButton switcher;
    [SerializeField] private TMP_Text buttonText;

    [Header("Аудио SFX")]
    [SerializeField] private AudioClip buttonClick;

    [Header("Громкость")]
    [Range(0f, 1f)]
    [SerializeField] private float clickVolume = 0.7f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        switcher.onClick.AddListener(SwitchWithSound);
        switched = false;
        buttonText.text = "💬";
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void SwitchWithSound()
    {
        AudioManager.PlayOneShot(buttonClick, clickVolume);
        Switch();
    }
    public void Switch()
    {
        if (switched)
        {
            switched = false;
            buttonText.text = "💬";

            GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
            foreach (GameObject obj in allObjects)
            {
                if (obj.name == "BackPanel" && obj.scene.isLoaded)
                {
                    obj.SetActive(true);
                }
                if (obj.name == "ChatPanel" && obj.scene.isLoaded)
                {
                    obj.SetActive(false);
                }

            }
            
        } else
        {
            if (Settings.CurretChat != null)
            {
                switched = true;
                buttonText.text = "📜";

                GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
                foreach (GameObject obj in allObjects)
                {
                    if (obj.name == "BackPanel" && obj.scene.isLoaded)
                    {
                        obj.SetActive(false);
                    }
                    if (obj.name == "ChatPanel" && obj.scene.isLoaded)
                    {
                        obj.SetActive(true);
                    }

                }
            }
        }
    }
}
