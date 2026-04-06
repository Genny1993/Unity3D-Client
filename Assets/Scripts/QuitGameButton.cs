using System;
using System.Collections.Generic;
using UnityEngine;

public class QuitGameButton : MonoBehaviour
{
    // Этот метод будет вызван при нажатии на кнопку
    public async void ExitGame()
    {
        if(Settings.AuthKey != "" && Settings.AuthKey != null)
        {
            var formData = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("pack[service]", "account"),
                new KeyValuePair<string, string>("pack[method]", "logout"),
                new KeyValuePair<string, string>("pack[access_key]", Settings.AuthKey),
                new KeyValuePair<string, string>("pack[info]", "")
            };

            try
            {
                Newtonsoft.Json.Linq.JObject result = await Sender.SendAndGet(formData);
                
            }
            catch (Exception) { }
            finally 
            {
                #if UNITY_EDITOR
                    UnityEditor.EditorApplication.isPlaying = false;
                #endif
                Application.Quit();
            }
        } else
        {
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #endif
            Application.Quit();
        }
    }
}
