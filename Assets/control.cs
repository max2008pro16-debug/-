using UnityEngine;
using UnityEngine.UIElements;

public class CameraController2D : MonoBehaviour
{
    [Header("Цель (персонаж)")]
    public Transform target;

    [Header("Свободное перемещение стрелками")]
    public float freeMoveSpeed = 10f;

    [Header("Следование за персонажем")]
    public bool snapToPlayerOnMove = true;
    public float followSmoothTime = 0.12f;

    [Header("Границы и перенос по X")]
    [Tooltip("true — камера телепортируется на другую сторону (как игрок). false — упирается в границу.")]
    public bool wrapOnX = true;
    public float minX = -50f;
    public float maxX = 50f;

    [Header("Ограничение по Y (опционально)")]
    public bool useYBounds = true;
    public float minY = -50f;
    public float maxY = 50f;

    private Vector3 velocity;
    private bool following = true;
    private bool wasPlayerMovingLastFrame = false;
    private float lastTargetX;

    void Start()
    {

        if (target == null)
        {
            GameObject p = GameObject.FindWithTag("Player");
            if (p != null) target = p.transform;
        }

        if (target != null) lastTargetX = target.position.x;
    }

    void LateUpdate()
    {
        bool playerMoving = IsPlayerMoving();

        if (playerMoving && (!wasPlayerMovingLastFrame || !following))
        {
            following = true;
            if (snapToPlayerOnMove) SnapToPlayer();
        }
        wasPlayerMovingLastFrame = playerMoving;

        if (!playerMoving)
        {
            float h = 0f;
            if (Input.GetKey(KeyCode.RightArrow)) h += 1f;
            if (Input.GetKey(KeyCode.LeftArrow)) h -= 1f;

            float v = 0f;
            if (Input.GetKey(KeyCode.UpArrow)) v += 1f;
            if (Input.GetKey(KeyCode.DownArrow)) v -= 1f;

            if (h != 0f || v != 0f)
            {
                following = false;
                Vector3 dir = new Vector3(h, v, 0f).normalized;
                transform.position += dir * freeMoveSpeed * Time.deltaTime;
            }
        }
        else if (following && target != null)
        {
            FollowTarget();
        }

        // Сглаживаем/переносим позицию камеры по X и ограничиваем Y
        WrapAndClamp();
    }

    bool IsPlayerMoving()
    {
        return Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) ||
               Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D);
    }

    void FollowTarget()
    {
        float wrapWidth = maxX - minX;
        float dx = target.position.x - lastTargetX;

        // Персонаж только что телепортировался через границу — телепортируемся вместе с ним,
        // иначе SmoothDamp протащит камеру через весь экран.
        if (wrapOnX && wrapWidth > 0f && Mathf.Abs(dx) > wrapWidth * 0.5f)
        {
            SnapToPlayer();
        }
        else
        {
            Vector3 desired = new Vector3(target.position.x, target.position.y, transform.position.z);
            transform.position = Vector3.SmoothDamp(
                transform.position, desired, ref velocity, followSmoothTime);
        }

        lastTargetX = target.position.x;
    }

    void SnapToPlayer()
    {
        if (target == null) return;
        transform.position = new Vector3(
            target.position.x, target.position.y, transform.position.z);
        velocity = Vector3.zero;
        lastTargetX = target.position.x;
    }

    void WrapAndClamp()
    {
        Vector3 p = transform.position;
        float width = maxX - minX;

        if (wrapOnX && width > 0f)
        {
            if (p.x > maxX) p.x -= width;
            else if (p.x < minX) p.x += width;
        }
        else
        {
            p.x = Mathf.Clamp(p.x, minX, maxX);
        }

        if (useYBounds)
            p.y = Mathf.Clamp(p.y, minY, maxY);

        transform.position = p;
    }
}