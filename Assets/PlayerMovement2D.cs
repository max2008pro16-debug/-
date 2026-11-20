using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Animator))]
public class PlayerMovement2D : MonoBehaviour
{
    public bool isFrozen = false;
    [Header("Настройки скорости")]
    public float walkSpeed = 5f;
    public float sprintSpeed = 10f;
    public float acceleration = 15f;

    [Header("Настройки ускорения (Шифт)")]
    public float maxSprintTime = 3f;
    public float sprintRecoveryRate = 1.5f;

    [Header("Границы и перенос по X")]
    public bool wrapOnX = true;
    public float minX = -400f;
    public float maxX = 400f;

    [Header("Авто-границы из фона (опционально)")]
    public SpriteRenderer background;
    public bool autoBoundsFromBackground = false;

    private Rigidbody2D rb;
    private Animator anim;
    private Vector2 moveInput;
    private float currentSpeed;
    private float sprintTimer;
    private bool isSprinting;
    private int lastDirection = 0;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        if (GameSession.hasSavedPosition)
        {
            rb.position = GameSession.savedPlayerPosition;
            GameSession.hasSavedPosition = false;
        }

        currentSpeed = 0f;
        sprintTimer = maxSprintTime;
    }

    void Update()
    {
        if (isFrozen) return;

        // --- НОВАЯ СТРОКА: Возвращаем нормальный масштаб после броска ---
        // Так как у вас есть анимации налево и направо, скейл всегда должен быть положительным
        if (transform.localScale.x < 0)
        {
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }

        float h = 0f;
        if (Input.GetKey(KeyCode.D)) h += 1f;
        if (Input.GetKey(KeyCode.A)) h -= 1f;

        float v = 0f;
        if (Input.GetKey(KeyCode.W)) v += 1f;
        if (Input.GetKey(KeyCode.S)) v -= 1f;

        moveInput = new Vector2(h, v).normalized;

        if (moveInput != Vector2.zero)
        {
            if (Mathf.Abs(h) > Mathf.Abs(v))
                lastDirection = h > 0 ? 3 : 2;
            else
                lastDirection = v > 0 ? 1 : 0;
        }

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
        if (isFrozen) return;

        float targetSpeed = (moveInput != Vector2.zero)
            ? (isSprinting ? sprintSpeed : walkSpeed)
            : 0f;

        currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, acceleration * Time.fixedDeltaTime);

        Vector2 newPos = rb.position;

        if (moveInput != Vector2.zero)
        {
            newPos += moveInput * currentSpeed * Time.fixedDeltaTime;
        }
        else
        {
            currentSpeed = 0f;
        }

        newPos.x = ApplyXBounds(newPos.x);
        rb.MovePosition(newPos);

        anim.SetInteger("Direction", lastDirection);
        float animSpeed = (moveInput == Vector2.zero) ? 0f : currentSpeed / sprintSpeed;
        anim.SetFloat("Speed", animSpeed);
    }

    float ApplyXBounds(float x)
    {
        float width = maxX - minX;
        if (wrapOnX && width > 0f)
        {
            if (x > maxX) x = minX + (x - maxX);
            else if (x < minX) x = maxX - (minX - x);
        }
        else
        {
            x = Mathf.Clamp(x, minX, maxX);
        }
        return x;
    }
}