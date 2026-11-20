using UnityEngine;
using UnityEngine.UI;

public class QTEManager : MonoBehaviour
{
    [Header("UI Элементы")]
    public GameObject qtePanel;
    public Slider progressSlider;
    public Slider timeSlider;

    [Header("Настройки QTE")]
    public float timeLimit = 3f;
    public int requiredPresses = 15;
    public float throwForce = 10f;

    private int currentPresses = 0;
    private float currentTime;
    private bool isQTEActive = false;

    private PlayerHealth playerHealth;
    private GameObject monsterObject;
    private GameObject grabVisualRef; // Объект CutsceneGrab
    private Animator grabAnimator;    // Аниматор катсцены

    private SpriteRenderer playerSprite;
    private SpriteRenderer monsterSprite;
    private Rigidbody2D playerRb;
    private MonoBehaviour playerMovementScript;

    void Start()
    {
        if (qtePanel != null) qtePanel.SetActive(false);

        playerHealth = FindAnyObjectByType<PlayerHealth>();
        if (playerHealth != null)
        {
            playerSprite = playerHealth.GetComponent<SpriteRenderer>();
            playerRb = playerHealth.GetComponent<Rigidbody2D>();

            playerMovementScript = playerHealth.GetComponent<PlayerMovement2D>();
        }
    }

    public void StartQTE(GameObject monster, GameObject grabImage)
    {
        monsterObject = monster;
        grabVisualRef = grabImage;
        grabAnimator = grabVisualRef.GetComponent<Animator>(); // Берем аниматор с CutsceneGrab

        isQTEActive = true;
        currentPresses = 0;
        currentTime = timeLimit;

        // 1. Прячем отдельных игрока и монстра
        if (playerSprite != null) playerSprite.enabled = false;
        monsterSprite = monsterObject.GetComponent<SpriteRenderer>();
        if (monsterSprite != null) monsterSprite.enabled = false;

        // 2. Включаем катсцену (CutsceneGrab) и ставим анимацию захвата
        if (grabVisualRef != null)
        {
            grabVisualRef.SetActive(true);
            if (grabAnimator != null) grabAnimator.SetBool("IsThrowing", false); // Включает Grab_Anim
        }

        // 3. Замораживаем игрока
        if (playerMovementScript != null)
        {
            var field = playerMovementScript.GetType().GetField("isFrozen");
            if (field != null) field.SetValue(playerMovementScript, true);
            else playerMovementScript.enabled = false; // Если нет поля isFrozen
        }
        if (playerRb != null) playerRb.linearVelocity = Vector2.zero;

        // 4. Настраиваем UI
        qtePanel.SetActive(true);
        progressSlider.maxValue = requiredPresses;
        progressSlider.value = 0;
        timeSlider.maxValue = timeLimit;
        timeSlider.value = timeLimit;
    }

    void Update()
    {
        if (!isQTEActive) return;

        currentTime -= Time.deltaTime;
        timeSlider.value = currentTime;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            currentPresses++;
            progressSlider.value = currentPresses;
        }

        if (currentPresses >= requiredPresses) EndQTE(true);
        else if (currentTime <= 0) EndQTE(false);
    }

    void EndQTE(bool success)
    {
        isQTEActive = false;
        qtePanel.SetActive(false);

        if (success)
        {
            Debug.Log("Игрок вырвался! Включаем бросок...");

            // 1. ПЕРЕКЛЮЧАЕМ АНИМАЦИЮ НА БРОСОК
            if (grabAnimator != null) grabAnimator.SetBool("IsThrowing", true);

            // 2. ЖДЕМ 0.5 СЕКУНДЫ (пока проигрывается анимация броска), а затем запускаем полет
            Invoke(nameof(ExecuteThrow), 0.5f);
        }
        else
        {
            Debug.Log("Игрок не справился!");
            if (playerHealth != null) playerHealth.TakeDamage(3);
        }
    }

    void ExecuteThrow()
    {
        // 1. Выключаем катсцену захвата
        if (grabVisualRef != null) grabVisualRef.SetActive(false);

        // 2. Включаем спрайты игрока и монстра
        if (playerSprite != null) playerSprite.enabled = true;
        if (monsterSprite != null) monsterSprite.enabled = true;

        // 3. Игрок теряет здоровье, включается анимация полета и физический отброс
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(1);
            playerHealth.GetComponent<Animator>().SetBool("IsThrown", true);

            if (playerRb != null && monsterObject != null)
            {
                Vector2 throwDirection = (playerHealth.transform.position - monsterObject.transform.position).normalized;
                if (throwDirection == Vector2.zero) throwDirection = Vector2.up;

                // Берем текущий размер игрока (например, 0.43)
                float originalScaleX = Mathf.Abs(playerHealth.transform.localScale.x);

                if (throwDirection.x > 0)
                {
                    // Летим вправо
                    playerHealth.transform.localScale = new Vector3(originalScaleX, playerHealth.transform.localScale.y, playerHealth.transform.localScale.z);
                }
                else if (throwDirection.x < 0)
                {
                    // Летим влево
                    playerHealth.transform.localScale = new Vector3(-originalScaleX, playerHealth.transform.localScale.y, playerHealth.transform.localScale.z);
                }

                playerRb.AddForce(throwDirection * throwForce, ForceMode2D.Impulse);
            }

            // Через 1 секунду выключаем анимацию полета и размораживаем игрока
            Invoke(nameof(ResetThrownAnim), 1f);
        }

        // 4. Монстр телепортируется
        if (monsterObject != null)
        {
            EnemyAI enemy = monsterObject.GetComponent<EnemyAI>();
            if (enemy != null) enemy.OnPlayerEscaped();
        }
    }

    void ResetThrownAnim()
    {
        if (playerHealth != null)
        {
            playerHealth.GetComponent<Animator>().SetBool("IsThrown", false);

            // РАЗМОРАЖИВАЕМ ИГРОКА
            if (playerMovementScript != null)
            {
                var field = playerMovementScript.GetType().GetField("isFrozen");
                if (field != null) field.SetValue(playerMovementScript, false);
                else playerMovementScript.enabled = true;
            }
        }
    }
}