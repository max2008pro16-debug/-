using UnityEngine;

public class ReturnTo2D : MonoBehaviour
{
    public string sceneToLoad = "SampleScene";

    void OnTriggerEnter(Collider other)
    {
        // 1. Пишем в консоль, что в дверь что-то влетело
        Debug.Log("В дверь вошел объект: " + other.gameObject.name);

        // 2. Проверяем, есть ли на нем скрипт игрока
        if (other.GetComponent<Simple3DPlayer>() != null)
        {
            Debug.Log("Это точно игрок! Запускаю переход...");

            // 3. Проверяем, существует ли наш Fader
            if (SceneFader.Instance != null)
            {
                SceneFader.Instance.FadeToScene(sceneToLoad);
            }
            else
            {
                // Если Fader не найден — выдаем ошибку
                Debug.LogError("ОШИБКА: SceneFader не найден! Вы забыли добавить объект Fader в 3D-сцену?");
                // На всякий случай делаем прямой переход, чтобы вы не застряли
                UnityEngine.SceneManagement.SceneManager.LoadScene(sceneToLoad);
            }
        }
    }
}