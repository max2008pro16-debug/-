using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class Simple3DPlayer : MonoBehaviour
{
    public float speed = 5f;
    public float mouseSensitivity = 2f;
    public Transform cameraTransform;
    public float interactDistance = 4f; // Увеличили дистанцию до 4

    private CharacterController controller;
    private float xRotation = 0f;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
        transform.Rotate(Vector3.up * mouseX);
        xRotation = Mathf.Clamp(xRotation - mouseY, -90f, 90f);

        if (cameraTransform != null)
            cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        Vector3 move = transform.right * x + transform.forward * z;
        controller.Move(move * speed * Time.deltaTime);

        // --- Взаимодействие ---
        if (cameraTransform == null) return; // Защита, если камера не привязана

        Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);
        RaycastHit hit;

        // Рисуем красный луч в окне Scene, чтобы вы видели, куда он стреляет
        Debug.DrawRay(cameraTransform.position, cameraTransform.forward * interactDistance, Color.red);

        if (Physics.Raycast(ray, out hit, interactDistance))
        {
            // Если луч попал в объект, пишем в консоль, во что именно он попал
            Debug.Log("Луч попал в: " + hit.collider.gameObject.name);

            Interactable3D interactable = hit.collider.GetComponent<Interactable3D>();

            if (interactable != null && Input.GetKeyDown(KeyCode.E))
            {
                interactable.OnInteract();
            }
        }
    }
}