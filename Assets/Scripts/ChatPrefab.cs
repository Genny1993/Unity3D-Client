using TMPro;
using UnityEngine;

public class ChatPrefab : MonoBehaviour
{
    public string chatId;
    [SerializeField] private TMP_Text chatName;

    public void Initializate(string chat_id, string chat_name)
    {
        chatId = chat_id;
        chatName.text = chat_name;
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
