using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [Header("Ссылки")]
    public Transform player;
    public GameObject grabVisual;

    [Header("Скорости")]
    public float walkSpeed = 2f; // Скорость ходьбы монстра
    public float runSpeed = 4f;  // Скорость бега монстра

    [Header("Границы карты (Wrap Around)")]
    public bool wrapOnX = true;
    public float minX = -400f;
    public float maxX = 400f;

    private float currentSpeed;
    private float visionRange;
    private Rigidbody2D rb;
    private Animator anim;
    private bool isCutsceneTriggered = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    void FixedUpdate()
    {
        if (isCutsceneTriggered) return;

        // 1. Логика зрения и скорости в зависимости от артефактов
        if (GameSession.collectedArtifacts < 3)
        {
            visionRange = 5f;
            currentSpeed = walkSpeed;
        }
        else if (GameSession.collectedArtifacts >= 3 && GameSession.collectedArtifacts < 6)
        {
            visionRange = 100f;
            currentSpeed = walkSpeed;
        }
        else if (GameSession.collectedArtifacts >= 6 && GameSession.collectedArtifacts < 9)
        {
            visionRange = 100f;
            currentSpeed = runSpeed;
        }
        else if (GameSession.collectedArtifacts >= 9)
        {
            visionRange = 100f;
            currentSpeed = runSpeed;

            float dist = Vector2.Distance(transform.position, player.position);
            if (dist < 1.5f)
            {
                TriggerQTE();
                return;
            }
        }

        // 2. Движение к игроку
        float playerDistance = Vector2.Distance(transform.position, player.position);

        if (playerDistance < visionRange)
        {
            Vector2 direction = (player.position - transform.position).normalized;
            Vector2 newPos = rb.position + direction * currentSpeed * Time.fixedDeltaTime;

            // Wrap Around
            if (wrapOnX)
            {
                float width = maxX - minX;
                if (width > 0f)
                {
                    if (newPos.x > maxX) newPos.x = minX + (newPos.x - maxX);
                    else if (newPos.x < minX) newPos.x = maxX - (minX - newPos.x);
                }
            }

            rb.MovePosition(newPos);

            // --- ПЕРЕДАЧА В АНИМАТОР ---
            if (anim != null)
            {
                anim.SetFloat("InputX", direction.x);
                anim.SetFloat("InputY", direction.y);
                // Если бежим (артефактов > 6), отправляем 2, иначе 1 (ходьба)
                anim.SetFloat("Speed", (currentSpeed == runSpeed) ? 2f : 1f);
            }
        }
        else
        {
            // Игрок не виден — стоим (Idle)
            rb.linearVelocity = Vector2.zero;
            if (anim != null)
            {
                anim.SetFloat("InputX", 0f);
                anim.SetFloat("InputY", 0f);
                anim.SetFloat("Speed", 0f);
            }
        }
    }

    void TriggerQTE()
    {
        isCutsceneTriggered = true;
        rb.linearVelocity = Vector2.zero;
        if (anim != null) anim.SetFloat("Speed", 0f);

        QTEManager qte = FindAnyObjectByType<QTEManager>();
        if(qte != null) qte.StartQTE(this.gameObject, grabVisual);
    }

    public void OnPlayerEscaped()
    {
        float escapeX = player.position.x > 0 ? -350f : 350f;
        rb.position = new Vector2(escapeX, rb.position.y);
        Invoke(nameof(ResetTrigger), 3f);
    }

    void ResetTrigger()
    {
        isCutsceneTriggered = false;
    }
}