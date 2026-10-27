using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Цель")]
    public Transform hero;                 // Ссылка на героя

    [Header("Панорамирование стрелками")]
    public float panSpeed = 8f;            // Скорость движения камеры
    public float maxDistance = 6f;         // Максимальное отклонение от героя

    [Header("Возврат к герою")]
    [Tooltip("Чем больше — тем резче возврат. Поставь 20+ для мгновенного")]
    public float returnSpeed = 10f;

    [Header("Проверка движения героя")]
    public Rigidbody2D heroRb2D;           // (опционально) 2D-герой
    public Rigidbody heroRb3D;             // (опционально) 3D-герой
    [Tooltip("Если нет Rigidbody — проверяем эти клавиши движения героя")]
    public KeyCode[] heroMoveKeys = { KeyCode.W, KeyCode.A, KeyCode.S, KeyCode.D,
                                      KeyCode.LeftArrow, KeyCode.RightArrow };

    private Vector3 offset;                // Смещение камеры от героя
    private bool returning;                // Флаг возврата

    void Start()
    {
        if (hero != null)
            offset = transform.position - hero.position;
    }

    void LateUpdate()
    {
        if (hero == null) return;

        // 1. Если герой начал движение — включаем возврат
        if (IsHeroMoving())
            returning = true;

        // 2. Возврат к герою (плавно / мгновенно)
        if (returning)
        {
            offset = Vector3.Lerp(offset, Vector3.zero, returnSpeed * Time.deltaTime);
            if (offset.sqrMagnitude < 0.0001f)
            {
                offset = Vector3.zero;
                returning = false;  // вернулись — можно снова панорамировать
            }
        }
        // 3. Панорамирование стрелками
        else
        {
            Vector3 input = Vector3.zero;
            if (Input.GetKey(KeyCode.LeftArrow)) input.x -= 1f;
            if (Input.GetKey(KeyCode.RightArrow)) input.x += 1f;
            if (Input.GetKey(KeyCode.UpArrow)) input.y += 1f;
            if (Input.GetKey(KeyCode.DownArrow)) input.y -= 1f;

            if (input.sqrMagnitude > 0f)
            {
                offset += input.normalized * panSpeed * Time.deltaTime;
                offset = Vector3.ClampMagnitude(offset, maxDistance);
            }
        }

        // 4. Финальная позиция
        transform.position = hero.position + offset;
    }

    private bool IsHeroMoving()
    {
        // По физике (Rigidbody)
        if (heroRb2D != null && heroRb2D.linearVelocity.sqrMagnitude > 0.01f) return true;
        // Для Unity < 2023: heroRb2D.velocity
        if (heroRb3D != null && heroRb3D.linearVelocity.sqrMagnitude > 0.01f) return true;

        // Или по нажатию клавиш движения героя
        foreach (var key in heroMoveKeys)
            if (Input.GetKey(key)) return true;

        return false;
    }
}