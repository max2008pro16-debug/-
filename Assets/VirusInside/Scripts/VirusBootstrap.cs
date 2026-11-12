using UnityEngine;
using UnityEngine.SceneManagement; // Нужно для перезагрузки сцен

public class VirusBootstrap : MonoBehaviour
{
    // Тот самый Instance, к которому вы обращаетесь в VirusGame.cs
    public static VirusBootstrap Instance { get; private set; }

    private void Awake()
    {
        // Настройка синглтона
        if (Instance == null)
        {
            Instance = this;
            // Если этот объект должен жить при переходе между сценами:
            // DontDestroyOnLoad(gameObject); 
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Тот самый метод, который вызывается в VirusGame.cs
    public void RestartGame(bool hardRestart)
    {
        // Здесь должна быть ваша логика перезапуска.
        // Например, загрузка первой сцены (меню) или текущей сцены:

        Debug.Log("Игра перезапускается. HardRestart: " + hardRestart);

        // Пример перезагрузки текущей сцены:
        // SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}