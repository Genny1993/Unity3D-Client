using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public static class Settings
{
    public static string Url;
    public static string AuthKey;
    public static string ChatSelected;
    public static string CurretChat;
    public static string LastMessageId;
    public static string QuotedId = "";
    public static int lastCaretPosition = 0;
    public static int currentMessagesListHeight = 900;
    public static bool fileBar = false;
    public static bool quoteBar = false;
    public static int CountMessages = 0;
    public static int Page = 0;
}

public static class MessageBox
{
    private static GameObject messageBoxPrefab;

    // Загрузка префаба (вызовите один раз при старте игры)
    public static void Initialize()
    {
        if (messageBoxPrefab == null)
        {
            messageBoxPrefab = Resources.Load<GameObject>("Prefabs/MessageBoxCanvas");
            if (messageBoxPrefab == null)
                Debug.LogError("MessageBox prefab not found in Resources/Prefabs/");
        }
    }

    // Показать окно сообщения
    public static void Show(string title, string message, System.Action onClose = null)
    {
        if (messageBoxPrefab == null)
        {
            Debug.LogError("MessageBox not initialized! Call MessageBox.Initialize() first.");
            return;
        }

        // Создаем экземпляр окна
        GameObject windowObject = UnityEngine.Object.Instantiate(messageBoxPrefab);

        // Находим компонент MessageBoxWindow
        MessageBoxWindow window = windowObject.GetComponent<MessageBoxWindow>();

        if (window == null)
        {
            Debug.LogError("MessageBoxWindow component not found on prefab!");
            UnityEngine.Object.Destroy(windowObject);
            return;
        }

        // Инициализируем окно
        window.Initialize(title, message, onClose);
    }
}

public static class Crypt
{
    private static string key = "";

    private static readonly int KeySize = 256; // AES-256
    private static readonly int BlockSize = 128; // AES block size
    private static readonly int IvSize = 16; // 128 bits = 16 bytes

    public static string Encrypt(string plainText)
    {
        byte[] iv = new byte[IvSize];
        byte[] cipherText;

        using (Aes aes = Aes.Create())
        {
            aes.KeySize = KeySize;
            aes.BlockSize = BlockSize;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            // Генерируем случайный IV
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(iv);
            }
            aes.IV = iv;
            aes.Key = Convert.FromBase64String(key);
            // Шифруем
            using (var encryptor = aes.CreateEncryptor())
            using (var ms = new MemoryStream())
            {
                // Сначала записываем IV
                ms.Write(iv, 0, iv.Length);

                using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                using (var sw = new StreamWriter(cs))
                {
                    sw.Write(plainText);
                }

                cipherText = ms.ToArray();
            }
        }

        return Convert.ToBase64String(cipherText);
    }

    public static string Decrypt(string cipherTextBase64)
    {
        byte[] fullCipher = Convert.FromBase64String(cipherTextBase64);
        byte[] iv = new byte[IvSize];
        byte[] cipherText = new byte[fullCipher.Length - IvSize];

        // Извлекаем IV из начала
        Array.Copy(fullCipher, 0, iv, 0, IvSize);
        Array.Copy(fullCipher, IvSize, cipherText, 0, cipherText.Length);

        using (Aes aes = Aes.Create())
        {
            aes.KeySize = KeySize;
            aes.BlockSize = BlockSize;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            aes.IV = iv;
            aes.Key = Convert.FromBase64String(key);

            // Дешифруем
            using (var decryptor = aes.CreateDecryptor())
            using (var ms = new MemoryStream(cipherText))
            using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
            using (var sr = new StreamReader(cs))
            {
                return sr.ReadToEnd();
            }
        }
    }

    public static void LoadKey()
    {
        Crypt.key = FileManager.ReadKey();
    }
}

public static class JsonConverter
{
    public static string To(List<KeyValuePair<string, string>> list)
    {
        var result = new JObject();

        foreach (var kvp in list)
        {
            var keys = ParsePath(kvp.Key);
            SetValue(result, keys, kvp.Value);
        }

        return result.ToString(Formatting.Indented);
    }

    private static List<string> ParsePath(string path)
    {
        var keys = new List<string>();
        var regex = new Regex(@"([^\[]+)(?:\[([^\]]+)\])*");

        // Разбираем строку вида "pack[info][login]"
        var parts = path.Split(new[] { '[', ']' }, StringSplitOptions.RemoveEmptyEntries);
        keys.AddRange(parts);

        return keys;
    }

    private static void SetValue(JObject obj, List<string> keys, string value)
    {
        if (keys.Count == 0) return;

        var current = obj;

        for (int i = 0; i < keys.Count - 1; i++)
        {
            var key = keys[i];
            if (current[key] == null)
            {
                current[key] = new JObject();
            }
            current = (JObject)current[key];
        }

        current[keys[keys.Count - 1]] = value;
    }
}

public static class Sender
{
    public static async Task<JObject> SendRequest(List<KeyValuePair<string, string>> list)
    {
        var handler = new HttpClientHandler
        {
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
            MaxRequestContentBufferSize = 2147483647  // 2 ГБ
        };

        using (HttpClient client = new HttpClient(handler))
        {
            client.Timeout = TimeSpan.FromMinutes(30);
            client.DefaultRequestHeaders.ExpectContinue = true;

            // Отправляем POST запрос с form-urlencoded данными
            string json = JsonConverter.To(list);
            json = Crypt.Encrypt(json);
            var content = new StringContent(json, Encoding.UTF8, "text/plain");

            HttpResponseMessage response = await client.PostAsync(Settings.Url, content);



            string responseBody = await response.Content.ReadAsStringAsync();
            JObject jsonObject = new JObject();
            //Дешифровка
            try
            {

                responseBody = Crypt.Decrypt(responseBody);
                jsonObject = JObject.Parse(responseBody);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка" + ex.Message, responseBody.ToString());
            }

            return jsonObject;
        }
    }

    public static async Task<JObject> SendAndGet(List<KeyValuePair<string, string>> list)
    {
        JObject result;
        try
        {
            result = await SendRequest(list);
        }
        catch (Exception ex)
        {
            MessageBox.Show("Ошибка", $"Ошибка при подключении: {ex.Message}");
            throw new Exception("Error");
        }

        string status = result["status"]?.ToString() ?? "";
        if (status == "OK")
        {
            return result;
        }
        else if (status == "AUTH_ERROR")
        {
            MessageBox.Show("Ошибка авторизации", "У вас нет прав на совершение этого действия");
            throw new Exception("Error");
        }
        else if (status == "C_ERROR")
        {
            MessageBox.Show("Ошибка", result["info"]["message"].ToString());
            throw new Exception("Error");
        }
        else
        {
            MessageBox.Show("Ошибка", result.ToString());
            throw new Exception("Error");
        }
    }
}

public static class UIManager
{
    private static Canvas mainCanvas;

    public static void ShowLoginWindow()
    {
        // Находим Canvas
        if (mainCanvas == null)
        {
            GameObject canvasObj = GameObject.Find("MainCanvas");

            if (canvasObj == null)
            {
                Debug.LogError("Canvas не найден!");
                return;
            }
            else
            {
                mainCanvas = canvasObj.GetComponent<Canvas>();
            }
        }

        // Загружаем и показываем окно
        GameObject loginPrefab = Resources.Load<GameObject>("Prefabs/LoginWindow");
        if (loginPrefab != null)
        {
            UnityEngine.Object.Instantiate(loginPrefab, mainCanvas.transform);
        }
    }

    public static void ShowRegisterWindow()
    {
        // Находим Canvas
        if (mainCanvas == null)
        {
            GameObject canvasObj = GameObject.Find("MainCanvas");

            if (canvasObj == null)
            {
                Debug.LogError("Canvas не найден!");
                return;
            }
            else
            {
                mainCanvas = canvasObj.GetComponent<Canvas>();
            }
        }

        // Загружаем и показываем окно
        GameObject loginPrefab = Resources.Load<GameObject>("Prefabs/RegisterWindow");
        if (loginPrefab != null)
        {
            UnityEngine.Object.Instantiate(loginPrefab, mainCanvas.transform);
        }
    }


    public static void ShowChatWindow()
    {
        // Находим Canvas
        if (mainCanvas == null)
        {
            GameObject canvasObj = GameObject.Find("MainCanvas");

            if (canvasObj == null)
            {
                Debug.LogError("Canvas не найден!");
                return;
            }
            else
            {
                mainCanvas = canvasObj.GetComponent<Canvas>();
            }
        }

        // Загружаем и показываем окно
        GameObject loginPrefab = Resources.Load<GameObject>("Prefabs/ChatWindow");
        if (loginPrefab != null)
        {
            UnityEngine.Object.Instantiate(loginPrefab, mainCanvas.transform);
        }
    }
}

public static class AudioManager
{
    private static AudioSource oneShotSource;

    // Инициализация (вызвать один раз при старте игры)
    public static void Initialize()
    {
        if (oneShotSource == null)
        {
            GameObject audioObject = new GameObject("OneShotAudioSource");
            UnityEngine.Object.DontDestroyOnLoad(audioObject);
            oneShotSource = audioObject.AddComponent<AudioSource>();
            oneShotSource.playOnAwake = false;
        }
    }

    // Проигрывание звука один раз (OneShot)
    public static void PlayOneShot(AudioClip clip, float volume = 1f)
    {
        if (oneShotSource == null)
        {
            Debug.LogError("AudioManager не инициализирован! Вызовите Initialize() перед использованием.");
            return;
        }

        if (clip != null)
        {
            oneShotSource.PlayOneShot(clip, volume);
        }
        else
        {
            Debug.LogWarning("AudioClip не назначен!");
        }
    }
}

public static class MessageShowerWindow
{
    private static GameObject messageShowerPrefab;

    // Загрузка префаба (вызовите один раз при старте игры)
    public static void Initialize()
    {
        if (messageShowerPrefab == null)
        {
            messageShowerPrefab = Resources.Load<GameObject>("Prefabs/MessageShower");
            if (messageShowerPrefab == null)
                Debug.LogError("MessageShower prefab not found in Resources/Prefabs/");
        }
    }

    // Показать окно сообщения
    public static void Show(string id, ScrollRect messageList, TMP_InputField i_f, GameObject status_bar, GameObject quote_bar, TMP_Text quoteLabel)
    {
        if (messageShowerPrefab == null)
        {
            Debug.LogError("MessageShower not initialized! Call MessageShower.Initialize() first.");
            return;
        }

        // Создаем экземпляр окна
        GameObject windowObject = UnityEngine.Object.Instantiate(messageShowerPrefab);

        // Находим компонент MessageBoxWindow
        MessageShower window = windowObject.GetComponent<MessageShower>();

        if (window == null)
        {
            Debug.LogError("MessageShower component not found on prefab!");
            UnityEngine.Object.Destroy(windowObject);
            return;
        }

        // Инициализируем окно
        window.Initialize(id, messageList, i_f, status_bar, quote_bar, quoteLabel);
    }
}

public static class HistoryWindowStart
{
    private static GameObject messageShowerPrefab;

    // Загрузка префаба (вызовите один раз при старте игры)
    public static void Initialize()
    {
        if (messageShowerPrefab == null)
        {
            messageShowerPrefab = Resources.Load<GameObject>("Prefabs/HistoryWindow");
            if (messageShowerPrefab == null)
                Debug.LogError("MessageShower prefab not found in Resources/Prefabs/");
        }
    }

    // Показать окно сообщения
    public static void Show(string id, ScrollRect messageList, TMP_InputField i_f, GameObject status_bar, GameObject quote_bar, TMP_Text quoteLabel)
    {
        if (messageShowerPrefab == null)
        {
            Debug.LogError("HistoryWindow not initialized! Call HistoryWindow.Initialize() first.");
            return;
        }

        // Создаем экземпляр окна
        GameObject windowObject = UnityEngine.Object.Instantiate(messageShowerPrefab);

        // Находим компонент HistoryWindow
        HistoryWindow window = windowObject.GetComponent<HistoryWindow>();

        if (window == null)
        {
            Debug.LogError("HistoryWindow component not found on prefab!");
            UnityEngine.Object.Destroy(windowObject);
            return;
        }

        // Инициализируем окно
        window.Initialize(id, messageList, i_f, status_bar, quote_bar, quoteLabel);
    }
}

public static class SettingsWindowStart
{
    private static GameObject settingsWindowPrefab;

    // Загрузка префаба (вызовите один раз при старте игры)
    public static void Initialize()
    {
        if (settingsWindowPrefab == null)
        {
            settingsWindowPrefab = Resources.Load<GameObject>("Prefabs/SettingsWindow");
            if (settingsWindowPrefab == null)
                Debug.LogError("SettingsWindow prefab not found in Resources/Prefabs/");
        }
    }

    // Показать окно сообщения
    public static void Show()
    {
        if (settingsWindowPrefab == null)
        {
            Debug.LogError("SettingsWindow not initialized! Call SettingsWindow.Initialize() first.");
            return;
        }

        // Создаем экземпляр окна
        GameObject windowObject = UnityEngine.Object.Instantiate(settingsWindowPrefab);
    }
}

public static class FileInfo
{
    public static string FileName = "";
    public static long FileSize = 0;
    public static string FileType = "";
    public static string FileContentBase64 = "";

    // Метод для загрузки файла
    public static bool LoadFile(string filePath)
    {
        try
        {
            if (!System.IO.File.Exists(filePath))
                return false;

            // Получаем имя файла
            FileName = Path.GetFileName(filePath);

            // Получаем размер файла
            var fileInfo = new System.IO.FileInfo(filePath);
            FileSize = fileInfo.Length;

            // Определяем тип файла (на основе расширения)
            FileType = GetFileType(filePath);

            // Читаем содержимое и конвертируем в Base64
            byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);
            FileContentBase64 = Convert.ToBase64String(fileBytes);

            return true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Ошибка загрузки файла: {ex.Message}");
            return false;
        }
    }

    // Метод для определения типа файла
    private static string GetFileType(string filePath)
    {
        string extension = Path.GetExtension(filePath).ToLower();

        // Изображения
        if (extension == ".jpg" || extension == ".jpeg" || extension == ".png" ||
            extension == ".gif" || extension == ".bmp" || extension == ".ico" ||
            extension == ".webp" || extension == ".tiff")
            return "image/" + extension.TrimStart('.');

        // Видео
        if (extension == ".mp4" || extension == ".avi" || extension == ".mov" ||
            extension == ".wmv" || extension == ".flv" || extension == ".mkv" ||
            extension == ".webm" || extension == ".mpeg" || extension == ".mpg")
            return "video/" + extension.TrimStart('.');

        // Аудио
        if (extension == ".mp3" || extension == ".wav" || extension == ".ogg" ||
            extension == ".flac" || extension == ".aac" || extension == ".m4a")
            return "audio/" + extension.TrimStart('.');

        // Исполняемые файлы
        if (extension == ".exe" || extension == ".msi" || extension == ".bat" ||
            extension == ".cmd" || extension == ".com")
            return "application/x-msdownload";

        // Текстовые файлы
        if (extension == ".txt" || extension == ".csv" || extension == ".log")
            return "text/plain";

        // Документы
        if (extension == ".pdf")
            return "application/pdf";
        if (extension == ".doc" || extension == ".docx")
            return "application/msword";
        if (extension == ".xls" || extension == ".xlsx")
            return "application/vnd.ms-excel";
        if (extension == ".ppt" || extension == ".pptx")
            return "application/vnd.ms-powerpoint";

        // HTML/XML
        if (extension == ".html" || extension == ".htm")
            return "text/html";
        if (extension == ".xml")
            return "application/xml";
        if (extension == ".json")
            return "application/json";

        // По умолчанию
        return "application/octet-stream";
    }

    // Метод для сброса информации
    public static void Clear()
    {
        FileName = "";
        FileSize = 0;
        FileType = "";
        FileContentBase64 = "";
    }

    // Метод для отображения информации (для отладки)
    public static string GetFileInfo()
    {
        return $"Имя: {FileName}\nРазмер: {FileSize} байт\nТип: {FileType}\nBase64 длина: {FileContentBase64.Length} символов";
    }
}