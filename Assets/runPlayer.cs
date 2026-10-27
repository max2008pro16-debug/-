using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Animator))]
public class PlayerMovement2D : MonoBehaviour
{
    [Header("Настройки скорости")]
    public float walkSpeed = 5f;
    public float sprintSpeed = 10f;
    public float acceleration = 15f;

    [Header("Настройки ускорения (Шифт)")]
    public float maxSprintTime = 3f;
    public float sprintRecoveryRate = 1.5f;

    private Rigidbody2D rb;
    private Animator anim;
    private Vector2 moveInput;
    private float currentSpeed;
    private float sprintTimer;
    private bool isSprinting;

    // Переменная для хранения последнего направления (0-вниз, 1-вверх, 2-влево, 3-вправо)
    private int lastDirection = 0;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();

        // Замораживаем вращение, чтобы физика не крутила персонажа
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        currentSpeed = 0f;
        sprintTimer = maxSprintTime;
    }

    void Update()
    {
        // 1. Получаем ввод (WASD и стрелки)
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        moveInput = new Vector2(h, v).normalized;

        // 2. Определяем направление взгляда (если движемся)
        if (moveInput != Vector2.zero)
        {
            // Если движемся по диагонали, приоритет отдаем той оси, куда нажали сильнее
            if (Mathf.Abs(h) > Mathf.Abs(v))
            {
                lastDirection = h > 0 ? 3 : 2; // 3 - вправо, 2 - влево
            }
            else
            {
                lastDirection = v > 0 ? 1 : 0; // 1 - вверх, 0 - вниз
            }
        }

        // 3. Логика Шифта (Ускорение и таймер)
        bool shiftPressed = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

        if (shiftPressed && sprintTimer > 0f && moveInput != Vector2.zero)
        {
            isSprinting = true;
            sprintTimer -= Time.deltaTime;
        }
        else
        {
            isSprinting = false;
            if (sprintTimer < maxSprintTime)
            {
                sprintTimer += sprintRecoveryRate * Time.deltaTime;
                sprintTimer = Mathf.Clamp(sprintTimer, 0f, maxSprintTime);
            }
        }
    }

    void FixedUpdate()
    {
        // 4. Меняем скорость
        float targetSpeed = 0f;

        // Если есть ввод - задаем скорость, если нет - 0
        if (moveInput != Vector2.zero)
        {
            targetSpeed = isSprinting ? sprintSpeed : walkSpeed;
        }

        // Ускорение (разгон) оставляем плавным, чтобы старт не был резким
        currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, acceleration * Time.fixedDeltaTime);

        // 5. Двигаем персонажа
        if (moveInput != Vector2.zero)
        {
            rb.MovePosition(rb.position + moveInput * currentSpeed * Time.fixedDeltaTime);
        }
        else
        {
            // МГНОВЕННО обнуляем скорость, если кнопки отпущены
            currentSpeed = 0f;
        }

        // 6. ПЕРЕДАЕМ ДАННЫЕ В ANIMATOR
        anim.SetInteger("Direction", lastDirection);

        // Считаем скорость для анимации
        float animSpeed = currentSpeed / sprintSpeed;

        // Если стоим на месте, принудительно шлем 0, чтобы анимация замерла
        if (moveInput == Vector2.zero)
        {
            animSpeed = 0f;
        }

        anim.SetFloat("Speed", animSpeed);
    }
}