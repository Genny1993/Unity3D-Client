using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

public class StartMessengerInitializer : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static async void OnBeforeSceneLoad()
    {
        AudioManager.Initialize(); //Инициализируем проигрывание звуков
        MessageBox.Initialize(); //Инициализируем MessageBox
        MessageShowerWindow.Initialize();
        HistoryWindowStart.Initialize();
        SettingsWindowStart.Initialize();
        NewUserWindowStart.Initialize();
        NewChatWindowStart.Initialize();
        ImageWindowStart.Initialize();

        //Загружаем файловый менеджер
        GameObject go = new GameObject("FileManagerInit");
        var runner = go.AddComponent<CoroutineRunner>();
        runner.StartCoroutine(InitializeFileManagerAndCrypt());
        Object.DontDestroyOnLoad(go);


        //Показываем форму логина
        await LoadCryptKey();
    }

    private static async Task LoadCryptKey()
    {
        await Crypt.LoadKey();
        UIManager.ShowLoginWindow();
    }

    private static IEnumerator InitializeFileManagerAndCrypt()
    {
        yield return FileManager.Initialize();
    }

    private class CoroutineRunner : MonoBehaviour { }
}