using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public static class FileManager
{
    private static string settingsPath;
    private static string keyPath;

    public static IEnumerator Initialize()
    {
        settingsPath = Path.Combine(Application.persistentDataPath, "settings.json");
        keyPath = Path.Combine(Application.persistentDataPath, "crypt.key");

        // Копируем из StreamingAssets ТОЛЬКО если файлов нет
        if (!File.Exists(settingsPath))
            yield return CopyFromStreamingAssets("settings.json", settingsPath);

        if (!File.Exists(keyPath))
            yield return CopyFromStreamingAssets("crypt.key", keyPath);
    }

    private static IEnumerator CopyFromStreamingAssets(string fileName, string destPath)
    {
        string sourcePath = Path.Combine(Application.streamingAssetsPath, fileName);

        using (UnityWebRequest request = UnityWebRequest.Get(sourcePath))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                File.WriteAllBytes(destPath, request.downloadHandler.data);
            }
            else
            {
                MessageBox.Show("Ошибка", $"Ошибка копирования {fileName}: {request.error}");
            }
        }
    }

    public static string ReadSettings()
    {
        return File.ReadAllText(settingsPath);
    }

    public static void WriteSettings(string content)
    {
        File.WriteAllText(settingsPath, content);
    }

    public static string ReadKey()
    {
        return File.ReadAllText(keyPath);
    }

    public static void WriteKey(string content)
    {
        File.WriteAllText(keyPath, content);
    }
}
