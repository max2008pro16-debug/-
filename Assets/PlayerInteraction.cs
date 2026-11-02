using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerInteraction : MonoBehaviour
{
    public KeyCode interactKey = KeyCode.E;
    private InteractableHouse currentHouse;

    void OnTriggerEnter2D(Collider2D other)
    {
        var house = other.GetComponent<InteractableHouse>();
        if (house != null) currentHouse = house;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        var house = other.GetComponent<InteractableHouse>();
        if (house != null && house == currentHouse) currentHouse = null;
    }

    void Update()
    {
        if (currentHouse != null && Input.GetKeyDown(interactKey))
        {
            // 1. ЗАПОМИНАЕМ ПОЗИЦИЮ ИГРОКА ПЕРЕД ВЫХОДОМ
            GameSession.savedPlayerPosition = transform.position;
            GameSession.hasSavedPosition = true;

            // 2. Загружаем нужную 3D-комнату
            SceneManager.LoadScene(currentHouse.sceneToLoad);
        }
    }
}