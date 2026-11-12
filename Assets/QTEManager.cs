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
    public float throwForce = 10f; // Сила отброса игрока

    private int currentPresses = 0;
    private float currentTime;
    private bool isQTEActive = false;

    private PlayerHealth playerHealth;
    private GameObject monsterObject;
    private GameObject grabVisualRef;

    private SpriteRenderer playerSprite;
    private SpriteRenderer monsterSprite;
    private Rigidbody2D playerRb;
    private MonoBehaviour playerMovementScript; // Ссылка на скрипт движения

    void Start()
    {
        if (qtePanel != null) qtePanel.SetActive(false);

        // Находим игрока и его компоненты
        playerHealth = FindAnyObjectByType<PlayerHealth>();
        if (playerHealth != null)
        {
            playerSprite = playerHealth.GetComponent<SpriteRenderer>();
            playerRb = playerHealth.GetComponent<Rigidbody2D>();

            // Пытаемся найти скрипт движения (замените PlayerMovement2D на название вашего скрипта!)
            playerMovementScript = playerHealth.GetComponent<PlayerMovement2D>();
        }
    }

    public void StartQTE(GameObject monster, GameObject grabImage)
    {
        monsterObject = monster;
        grabVisualRef = grabImage;
        isQTEActive = true;
        currentPresses = 0;
        currentTime = timeLimit;

        // 1. Прячем игрока и монстра
        if (playerSprite != null) playerSprite.enabled = false;
        monsterSprite = monsterObject.GetComponent<SpriteRenderer>();
        if (monsterSprite != null) monsterSprite.enabled = false;

        // 2. Показываем картинку захвата
        if (grabVisualRef != null) grabVisualRef.SetActive(true);

        // 3. Замораживаем игрока
        if (playerMovementScript != null)
        {
            // Включаем isFrozen (если вы используете мой первый вариант)
            var field = playerMovementScript.GetType().GetField("isFrozen");
            if (field != null) field.SetValue(playerMovementScript, true);

            // Если используете свой скрипт, просто напишите: playerMovementScript.enabled = false;
        }
        if (playerRb != null) playerRb.linearVelocity = Vector2.zero; // Останавливаем физику

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

        if (grabVisualRef != null) grabVisualRef.SetActive(false);

        if (success)
        {
            Debug.Log("Игрок вырвался!");

            // 1. Включаем спрайты обратно
            if (playerSprite != null) playerSprite.enabled = true;
            if (monsterSprite != null) monsterSprite.enabled = true;

            // 2. Отнимаем здоровье и включаем анимацию полета
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(1);
                playerHealth.GetComponent<Animator>().SetBool("IsThrown", true);

                // 3. ФИЗИЧЕСКИЙ ОТБРОС ИГРОКА
                if (playerRb != null && monsterObject != null)
                {
                    // Вычисляем направление от монстра к игроку
                    Vector2 throwDirection = (playerHealth.transform.position - monsterObject.transform.position).normalized;
                    // Если они стояли слишком близко, бросаем просто назад (вверх или вниз)
                    if (throwDirection == Vector2.zero) throwDirection = Vector2.up;

                    // Применяем силу
                    playerRb.AddForce(throwDirection * throwForce, ForceMode2D.Impulse);
                }

                // Выключаем анимацию полета через 1 секунду
                Invoke(nameof(ResetThrownAnim), 1f);
            }

            // 4. Монстр телепортируется
            if (monsterObject != null)
            {
                EnemyAI enemy = monsterObject.GetComponent<EnemyAI>();
                if (enemy != null) enemy.OnPlayerEscaped();
            }
        }
        else
        {
            Debug.Log("Игрок не справился!");
            if (playerHealth != null) playerHealth.TakeDamage(3);
        }
    }

    void ResetThrownAnim()
    {
        if (playerHealth != null)
        {
            playerHealth.GetComponent<Animator>().SetBool("IsThrown", false);

            // РАЗМОРАЖИВАЕМ ИГРОКА (возвращаем управление)
            if (playerMovementScript != null)
            {
                var field = playerMovementScript.GetType().GetField("isFrozen");
                if (field != null) field.SetValue(playerMovementScript, false);
                // Или: playerMovementScript.enabled = true;
            }
        }
    }
}