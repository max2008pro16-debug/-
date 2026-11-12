using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class CutsceneManager : MonoBehaviour
{
    [Header("UI Элементы")]
    public GameObject cutscenePanel; // Черная панель с текстом
    public Button choiceButton1; // Кнопка 1 (Доступна при 9 артефактах)
    public Button choiceButton2; // Кнопка 2 (Доступна при 9 артефактах + записках)

    void Start()
    {
        // При старте игры катсцена выключена
        if (cutscenePanel != null) cutscenePanel.SetActive(false);
    }

    public void StartCutscene()
    {
        cutscenePanel.SetActive(true);

        // Замораживаем время (чтобы все остановилось)
        Time.timeScale = 0f;

        // Отвязываем курсор мыши, чтобы можно было нажимать кнопки
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Проверяем, собраны ли записки
        if (GameSession.collectedNotes >= GameSession.maxNotes)
        {
            choiceButton2.interactable = true; // Открываем вторую кнопку
        }
        else
        {
            choiceButton2.interactable = false; // Блокируем вторую кнопку
        }

        // Первая кнопка всегда открыта (так как мы дожили до 9 артефактов)
        choiceButton1.interactable = true;
    }

    public void ChooseEnding1()
    {
        Time.timeScale = 1f;
        Debug.Log("Выбрана концовка 1");
        // SceneManager.LoadScene("Ending_1"); 
    }

    public void ChooseEnding2()
    {
        Time.timeScale = 1f;
        Debug.Log("Выбрана концовка 2 (Истинная)");
        // SceneManager.LoadScene("Ending_2"); 
    }
}