using UnityEngine;
using UnityEngine.SceneManagement;

public class ReturnTo2D : MonoBehaviour
{
    // Сюда нужно вписать ТОЧНОЕ имя вашей 2D-сцены
    public string sceneToLoad = "SampleScene";

    void OnTriggerEnter(Collider other)
    {
        // Если в дверь вошел объект, у которого есть скрипт Simple3DPlayer (то есть наш игрок)
        if (other.GetComponent<Simple3DPlayer>() != null)
        {
            SceneManager.LoadScene(sceneToLoad);
        }
    }
}