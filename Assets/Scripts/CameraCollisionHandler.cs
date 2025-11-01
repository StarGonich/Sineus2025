using UnityEngine;

public class CameraCollisionHandler : MonoBehaviour
{
    [Header("Collision Settings")]
    public float minDistance = 0.5f;
    public float maxDistance = 10f; // Уменьшили дистанцию
    public float smoothness = 5f; // Увеличили плавность
    public LayerMask collisionMask = ~0;

    private Vector3 dollyDir;
    private float distance;
    private float currentDistance;
    private Vector3 desiredCameraPos;

    void Awake()
    {
        dollyDir = transform.localPosition.normalized;
        distance = transform.localPosition.magnitude;
        currentDistance = distance;
    }

    void LateUpdate()
    {
        HandleCameraCollision();
    }

    void HandleCameraCollision()
    {
        // Желаемая позиция камеры
        desiredCameraPos = transform.parent.TransformPoint(dollyDir * maxDistance);

        RaycastHit hit;
        float targetDistance = maxDistance;

        // Проверяем столкновение с более гладким подходом
        if (Physics.SphereCast(transform.parent.position, 0.2f,
            (desiredCameraPos - transform.parent.position).normalized,
            out hit, maxDistance, collisionMask))
        {
            targetDistance = Mathf.Clamp(hit.distance * 0.9f, minDistance, maxDistance);
        }

        // Плавное изменение дистанции
        currentDistance = Mathf.Lerp(currentDistance, targetDistance, smoothness * Time.deltaTime);

        // Применяем новую позицию
        transform.localPosition = Vector3.Lerp(
            transform.localPosition,
            dollyDir * currentDistance,
            smoothness * Time.deltaTime
        );
    }
}