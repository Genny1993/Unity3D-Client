using System.Collections;
using UnityEngine;

public class StartMessengerInitializer : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void OnBeforeSceneLoad()
    {
        AudioManager.Initialize(); //Инициализируем проигрывание звуков
        MessageBox.Initialize(); //Инициализируем MessageBox
        MessageShowerWindow.Initialize();
        HistoryWindowStart.Initialize();
        SettingsWindowStart.Initialize();

        //Загружаем файловый менеджер
        GameObject go = new GameObject("FileManagerInit");
        var runner = go.AddComponent<CoroutineRunner>();
        runner.StartCoroutine(InitializeFileManagerAndCrypt());
        Object.DontDestroyOnLoad(go);

        //Загружаем ключ шифрования
        Crypt.LoadKey();

        //Показываем форму логина
        UIManager.ShowLoginWindow();
    }

    private static IEnumerator InitializeFileManagerAndCrypt()
    {
        yield return FileManager.Initialize();
        Crypt.LoadKey(); // теперь внутри Crypt.LoadKey() уже можно использовать FileManager
    }

    private class CoroutineRunner : MonoBehaviour { }
}