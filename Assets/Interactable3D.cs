using UnityEngine;

public class Interactable3D : MonoBehaviour
{
    // Метод, который будет вызываться при нажатии кнопки E
    public virtual void OnInteract()
    {
        Debug.Log("Предмет подобран: " + gameObject.name);
        Destroy(gameObject); // Удаляем предмет со сцены
    }
}