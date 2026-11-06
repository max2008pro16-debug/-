using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerInteraction : MonoBehaviour
{
    public KeyCode interactKey = KeyCode.E;
    public GameObject hintUI; // Ссылка на наш текст
    private InteractableHouse currentHouse;

    void OnTriggerEnter2D(Collider2D other)
    {
        var house = other.GetComponent<InteractableHouse>();
        if (house != null)
        {
            currentHouse = house;
            if (hintUI != null) hintUI.SetActive(true); // Показываем текст
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        var house = other.GetComponent<InteractableHouse>();
        if (house != null && house == currentHouse)
        {
            currentHouse = null;
            if (hintUI != null) hintUI.SetActive(false); // Прячем текст
        }
    }

    void Update()
    {
        if (currentHouse != null && Input.GetKeyDown(interactKey))
        {
            GameSession.savedPlayerPosition = transform.position;
            GameSession.hasSavedPosition = true;

            // ВАЖНО: Проверяем, существует ли SceneFader перед вызовом
            if (SceneFader.Instance != null)
            {
                // Если у вас старый вариант с одной сценой:
                SceneFader.Instance.FadeToScene(currentHouse.sceneToLoad);

                // Если вы уже сделали рандомные комнаты, используйте эту строчку вместо верхней:
                // SceneFader.Instance.FadeToScene(currentHouse.GetRandomRoom());
            }
            else
            {
                Debug.LogError("Ошибка: GameManager со скриптом SceneFader отсутствует в сцене!");
            }
        }
    }
}