using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [Header("Ссылки")]
    public Transform player;
    public GameObject grabVisual;

    [Header("Скорости")]
    public float walkSpeed = 2f;
    public float runSpeed = 4f;

    private float currentSpeed;
    private float visionRange;
    private Rigidbody2D rb;
    private Animator anim;
    private bool isCutsceneTriggered = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        if (grabVisual != null) grabVisual.SetActive(false);
    }

    void FixedUpdate()
    {
        if (isCutsceneTriggered) return;

        // 1. ЛОГИКА АРТЕФАКТОВ (Влияют только на скорость и зрение)
        if (GameSession.collectedArtifacts < 3)
        {
            visionRange = 5f; // Видит вблизи
            currentSpeed = walkSpeed;
        }
        else if (GameSession.collectedArtifacts >= 3 && GameSession.collectedArtifacts < 6)
        {
            visionRange = 100f; // Видит через всю карту
            currentSpeed = walkSpeed;
        }
        else if (GameSession.collectedArtifacts >= 6)
        {
            visionRange = 100f; // Видит через всю карту
            currentSpeed = runSpeed; // Начинает бегать
        }

        float dist = Vector2.Distance(transform.position, player.position);

        // 2. ЛОГИКА ПОИМКИ
        if (dist < 1.5f)
        {
            isCutsceneTriggered = true;
            rb.linearVelocity = Vector2.zero;
            if (anim != null) anim.SetFloat("Speed", 0f);

            // Если собраны ВСЕ 9 артефактов — включаем панель выбора
            if (GameSession.collectedArtifacts >= 9)
            {
                CutsceneManager manager = FindAnyObjectByType<CutsceneManager>();
                if (manager != null) manager.StartCutscene();
            }
            // Если НЕ все собраны — включаем QTE (схватку)
            else
            {
                StartAttack();
            }
            return;
        }

        // 3. ДВИЖЕНИЕ К ИГРОКУ
        if (dist < visionRange)
        {
            Vector2 direction = (player.position - transform.position).normalized;
            Vector2 newPos = rb.position + direction * currentSpeed * Time.fixedDeltaTime;

            // Wrap Around (если нужно)
            // rb.position = new Vector2(Mathf.Clamp(newPos.x, minX, maxX), newPos.y); // Если используете границы

            rb.MovePosition(newPos);

            if (anim != null)
            {
                anim.SetFloat("InputX", direction.x);
                anim.SetFloat("InputY", direction.y);
                anim.SetFloat("Speed", (currentSpeed == runSpeed) ? 2f : 1f);
            }
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
            if (anim != null)
            {
                anim.SetFloat("InputX", 0f); anim.SetFloat("InputY", 0f); anim.SetFloat("Speed", 0f);
            }
        }
    }

    void StartAttack()
    {
        if (anim != null) anim.SetFloat("Speed", 0f);

        float dirX = player.position.x - transform.position.x;

        if (dirX > 0)
        {
            transform.localScale = new Vector3(1, 1, 1);
            if (anim != null) anim.SetFloat("AttackDir", 1f);
        }
        else
        {
            transform.localScale = new Vector3(-1, 1, 1);
            if (anim != null) anim.SetFloat("AttackDir", -1f);
        }

        Invoke(nameof(StartGrabSequence), 0.5f);
    }

    void StartGrabSequence()
    {
        QTEManager qte = FindAnyObjectByType<QTEManager>();
        if (qte != null) qte.StartQTE(this.gameObject, grabVisual);
    }

    public void OnPlayerEscaped()
    {
        // Телепортируем монстра подальше, чтобы дать игроку передышку
        float escapeX = player.position.x > 0 ? -350f : 350f;
        rb.position = new Vector2(escapeX, rb.position.y);

        if (anim != null) anim.SetFloat("Speed", 0f);

        // Через 3 секунды монстр снова может атаковать
        Invoke(nameof(ResetTrigger), 3f);
    }

    void ResetTrigger()
    {
        isCutsceneTriggered = false;
    }
}