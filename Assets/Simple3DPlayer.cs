using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class Simple3DPlayer : MonoBehaviour
{
    public float speed = 5f;
    public float mouseSensitivity = 2f;
    public Transform cameraTransform; // Сюда мы перетащим камеру

    private CharacterController controller;
    private float xRotation = 0f;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked; // Прячем курсор мыши при старте
    }

    void Update()
    {
        // 1. Вращение камеры мышкой
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
        transform.Rotate(Vector3.up * mouseX); // Крутим всего персонажа влево/вправо
        xRotation = Mathf.Clamp(xRotation - mouseY, -90f, 90f); // Ограничиваем взгляд вверх/вниз

        if (cameraTransform != null)
            cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // 2. Движение на WASD
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        Vector3 move = transform.right * x + transform.forward * z; // Направление движения
        controller.Move(move * speed * Time.deltaTime); // Двигаем игрока
    }
}