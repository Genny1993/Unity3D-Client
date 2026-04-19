using UnityEngine;
using System.Collections.Generic;

public class GlobalKeyboardAdjuster : MonoBehaviour
{
    public int size = 0;
    public bool debugLog = false;

    private List<RectTransform> mainLayers = new List<RectTransform>();
    private int lastAppliedSize = -1;

    private static GlobalKeyboardAdjuster instance;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void AutoInitialize()
    {
        if (instance == null)
        {
            GameObject globalObject = new GameObject("_GlobalKeyboardAdjuster");
            instance = globalObject.AddComponent<GlobalKeyboardAdjuster>();
            DontDestroyOnLoad(globalObject);

            if (instance.debugLog)
                Debug.Log("[GlobalKeyboardAdjuster] Автоматически создан и запущен");
        }
    }

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        FindAllMainLayers();
        if (debugLog)
            Debug.Log($"[GlobalKeyboardAdjuster] Инициализирован. Найдено MainLayer: {mainLayers.Count}");
    }

    void Update()
    {
        FindAllMainLayers();

        if (debugLog)
            Debug.Log($"[GlobalKeyboardAdjuster] Обновление. Найдено MainLayer: {mainLayers.Count}");

        ApplyShift();
        lastAppliedSize = size;

        if (debugLog)
            Debug.Log($"[GlobalKeyboardAdjuster] Применён сдвиг: {size}px");
    }

    private void FindAllMainLayers()
    {
        mainLayers.Clear();

        GameObject[] allObjects = FindObjectsByType<GameObject>();

        foreach (GameObject obj in allObjects)
        {
            if (obj.name == "MainLayer")
            {
                RectTransform rect = obj.GetComponent<RectTransform>();
                if (rect != null)
                {
                    mainLayers.Add(rect);
                }
            }
        }
    }

    private void ApplyShift()
    {
        if (mainLayers.Count == 0) return;

        foreach (RectTransform layer in mainLayers)
        {
            if (layer != null)
            {
                SetBottomOffset(layer, GetAndroidKeyboardHeight());
            }
        }
    }

    public void SetShiftSize(int newSize)
    {
        size = newSize;

        if (debugLog)
            Debug.Log($"[GlobalKeyboardAdjuster] Размер сдвига установлен: {size}px");
    }

    public void RefreshMainLayers()
    {
        FindAllMainLayers();
        ApplyShift();
        lastAppliedSize = size;
    }

    private void SetBottomOffset(RectTransform rect, float bottomValue)
    {
        Vector2 offsetMin = rect.offsetMin;
        offsetMin.y = bottomValue;
        rect.offsetMin = offsetMin;
    }

    void OnEnable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        FindAllMainLayers();
        ApplyShift();
        lastAppliedSize = size;
    }

    private int GetAndroidKeyboardHeight()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
    try
    {
        using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        {
            AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            AndroidJavaObject view = currentActivity.Get<AndroidJavaObject>("mUnityPlayer").Call<AndroidJavaObject>("getView");
            AndroidJavaObject rect = new AndroidJavaObject("android.graphics.Rect");
            
            view.Call("getWindowVisibleDisplayFrame", rect);
            
            int visibleHeight = rect.Call<int>("height");
            int screenHeight = Screen.height;
            int keyboardHeight = screenHeight - visibleHeight;
            
            rect.Dispose();
            view.Dispose();
            currentActivity.Dispose();
            unityPlayer.Dispose();
            
            if (keyboardHeight > 0 && keyboardHeight < screenHeight / 2)
            {
                return keyboardHeight;
            }
            
            return 0;
        }
    }
    catch (System.Exception e)
    {
        if (debugLog)
            Debug.LogError($"[GlobalKeyboardAdjuster] Ошибка: {e.Message}");
        return 0;
    }
#else
        return size;
#endif
    }
}