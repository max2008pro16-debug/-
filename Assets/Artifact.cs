using UnityEngine;

public class Artifact : MonoBehaviour
{
    // Этот метод вызовет игрок при нажатии Е
    public void Collect()
    {
        // Увеличиваем счетчик в глобальной памяти
        GameSession.collectedArtifacts++;
        Debug.Log("Собрано артефактов: " + GameSession.collectedArtifacts);

        // Уничтожаем предмет в 3D-мире
        Destroy(gameObject);
    }
}