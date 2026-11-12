using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class Simple3DPlayer : MonoBehaviour
{
    [Header("Настройки движения")]
    public float speed = 5f;
    public float mouseSensitivity = 2f;

    [Header("Настройки прыжка")]
    public float gravity = -15f; // Сделал чуть сильнее, чтобы падал быстрее и приятнее
    public float jumpHeight = 1.5f;

    public Transform cameraTransform;
    public Transform holdPoint;
    public float interactDistance = 3f;

    public Animator anim;

    private CharacterController controller;
    private float xRotation = 0f;
    private float verticalVelocity;
    private GameObject heldObject;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        if (anim == null) anim = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        // 1. Вращение камеры
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
        transform.Rotate(Vector3.up * mouseX);
        xRotation = Mathf.Clamp(xRotation - mouseY, -90f, 90f);
        if (cameraTransform != null)
            cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // 2. Читаем ввод WASD
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        Vector3 move = transform.right * x + transform.forward * z;

        // 3. Прыжок и Гравитация
        bool isGrounded = controller.isGrounded;

        if (isGrounded && verticalVelocity < 0f)
        {
            verticalVelocity = -2f;

            // ОТКЛЮЧАЕМ АНИМАЦИЮ ПРЫЖКА, КОГДА КАСАЕМСЯ ЗЕМЛИ
            if (anim != null && anim.GetBool("IsJumping") == true)
            {
                anim.SetBool("IsJumping", false);
            }
        }

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);

            // ВКЛЮЧАЕМ АНИМАЦИЮ ПРЫЖКА
            if (anim != null) anim.SetBool("IsJumping", true);
        }

        verticalVelocity += gravity * Time.deltaTime;

        // 4. ДВИЖЕНИЕ (Объединяем горизонтальное и вертикальное)
        Vector3 finalMove = move * speed; // Горизонтальное движение
        finalMove.y = verticalVelocity;   // Добавляем падение/прыжок

        // Двигаем персонажа ОДИН раз со всеми векторами
        controller.Move(finalMove * Time.deltaTime);

        // 5. Анимации
        if (anim != null)
        {
            if (heldObject == null)
            {
                anim.SetFloat("InputX", x);
                anim.SetFloat("InputY", z);
            }
            else
            {
                float currentSpeed = new Vector2(x, z).magnitude;
                anim.SetFloat("Speed", currentSpeed);
            }
        }

        // 6. Взаимодействие (E)
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (heldObject == null)
                TryPickUp();
            else
                DropObject();
        }
    }

    void TryPickUp()
    {
        Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactDistance))
        {
            // Проверяем Артефакт
            Artifact artifact = hit.collider.GetComponent<Artifact>();
            if (artifact != null) { artifact.Collect(); return; }

            // Проверяем Записку
            Note note = hit.collider.GetComponent<Note>();
            if (note != null) { note.Read(); return; }

            // 2. ПРОВЕРЯЕМ КОРОБКУ (ОБЫЧНЫЙ ПРЕДМЕТ)
            if (hit.collider.GetComponent<Pickupable>() != null)
            {
                heldObject = hit.collider.gameObject;

                if (heldObject.GetComponent<Rigidbody>())
                    heldObject.GetComponent<Rigidbody>().isKinematic = true;

                if (heldObject.GetComponent<Collider>())
                    heldObject.GetComponent<Collider>().enabled = false;

                heldObject.transform.SetParent(holdPoint);
                heldObject.transform.localPosition = Vector3.zero;
                heldObject.transform.localRotation = Quaternion.identity;

                if (anim != null) anim.SetBool("IsCarrying", true);
            }
        }
    }

    void DropObject()
    {
        if (heldObject != null)
        {
            heldObject.transform.SetParent(null);

            Vector3 dropPosition = cameraTransform.position + cameraTransform.forward * 1.2f;
            RaycastHit hit;

            if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out hit, 1.2f))
            {
                dropPosition = hit.point - cameraTransform.forward * 0.5f;
            }

            heldObject.transform.position = dropPosition;

            Rigidbody rb = heldObject.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = false;
                rb.AddForce(cameraTransform.forward * 2f, ForceMode.Impulse);
                rb.AddTorque(new Vector3(Random.Range(-2f, 2f), Random.Range(-2f, 2f), Random.Range(-2f, 2f)), ForceMode.Impulse);
            }

            if (heldObject.GetComponent<Collider>())
                heldObject.GetComponent<Collider>().enabled = true;

            if (anim != null) anim.SetBool("IsCarrying", false);

            heldObject = null;
        }
    }
}