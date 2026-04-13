using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UserPrefab : MonoBehaviour
{
    public string userId;
    private string LastActivity;
    private string isI;

    [Header("UI Elements")]
    [SerializeField] private TMP_Text userName;
    [SerializeField] private GameObject onlineCircle;
    [SerializeField] private GameObject panel;
    [SerializeField] private Button settingsButton;

    [Header("Аудио SFX")]
    [SerializeField] private AudioClip buttonClick;

    [Header("Громкость")]
    [Range(0f, 1f)]
    [SerializeField] private float clickVolume = 0.7f;

    public void Initializate(string user_id, string user_name, string online_status, string last_activity, string is_i, string is_admin)
    {
        userId = user_id;
        userName.text = user_name;
        LastActivity = last_activity;
        isI = is_i;

        OnlineCircle circle = onlineCircle.GetComponent<OnlineCircle>();
        circle.SetOnline(online_status == "1" ? true : false);
        
        if(is_i == "1")
        {
            circle.SetOnline(true);
        }

        panel.GetComponent<Image>().color = is_i == "1" ? new Color32(128, 166, 255, 180) : new Color32(255, 192, 203, 180);
        if(is_admin == "1")
        {
            userName.text = "<b>" + userName.text + "</b>";
        }

        if (settingsButton != null)
            settingsButton.onClick.AddListener(OnSettingsButtonClick);
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnSettingsButtonClick()
    {
        AudioManager.PlayOneShot(buttonClick, clickVolume);
        if(isI == "1")
        {
            SettingsWindowStart.Show();
        }
    }
}
