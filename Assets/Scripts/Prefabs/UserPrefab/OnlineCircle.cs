using UnityEngine;
using UnityEngine.UI;

public class OnlineCircle : MonoBehaviour
{
    public bool isOnline = false;

    void Start()
    {
        UpdateColor();
    }

    public void SetOnline(bool online)
    {
        isOnline = online;
        UpdateColor();
    }

    void UpdateColor()
    {
        GetComponent<Image>().color = isOnline ? new Color32(0, 130, 0, 255) : new Color32(130, 130, 130, 225);
    }
}
